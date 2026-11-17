using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// Bakes the painted shape of a <see cref="TileMapGroup"/> into its <see cref="LevelDefinition"/>.
    /// The inspector's Bake button, the Tools menu and the auto-bake (before Play, on save) all come through here.
    /// </summary>
    public static class LevelBaker
    {
        /// <summary>
        /// Reads the group's tilemaps into a terrain, without writing anything. The terrain covers the painted cells only,
        /// in board space (the group's parent's local space). Empty when the group has no tilemaps or nothing typed is painted.
        /// </summary>
        public static LevelTerrain Bake(TileMapGroup group)
        {
            if (!group) return new LevelTerrain();
            var layers = group.GetActiveLayers();
            if (layers.Count == 0) return new LevelTerrain();

            if (!TryGetPaintedBounds(layers, out var bounds)) return new LevelTerrain();

            var reference = layers[0].Tilemap;
            var frame = group.BoardSpace;
            Vector2 BoardCenter(Vector2Int cell) => frame.InverseTransformPoint(reference.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)));

            var first = BoardCenter(bounds.min);
            var step = new Vector2(
                Mathf.Abs(BoardCenter(bounds.min + Vector2Int.right).x - first.x),
                Mathf.Abs(BoardCenter(bounds.min + Vector2Int.up).y - first.y));
            if (step.x <= 0f || step.y <= 0f) return new LevelTerrain();

            return new LevelTerrain(bounds, first - step * 0.5f, step, cell => TileMapGroup.GetTileType(layers, cell));
        }

        /// <summary>
        /// Bakes the group into its level when the bake differs from what the level holds (so unchanged levels are never dirtied).
        /// Returns true when the level changed. Does nothing without a level. <paramref name="recordUndo"/>: false for automatic bakes.
        /// </summary>
        public static bool BakeInto(TileMapGroup group, bool recordUndo = true)
        {
            if (!group || !group.Level) return false;
            var terrain = Bake(group);
            var source = DescribeSource(group);
            var level = group.Level;
            if (terrain.SameAs(level.Terrain) && level.BakedFrom == source) return false;

            if (recordUndo) Undo.RecordObject(level, "Bake Level Shape");
            level.SetTerrain(terrain, source);
            EditorUtility.SetDirty(level);
            return true;
        }

        /// <summary>
        /// Bakes every group in the open scenes (and the open prefab, if any) into its level and saves the changed levels.
        /// Groups that share a level with another group are skipped and reported, since it would be unclear which one wins.
        /// Returns the number of levels that changed.
        /// </summary>
        public static int BakeOpenScenes(bool recordUndo = true)
        {
            int changed = 0;
            foreach (var group in GroupsSafeToBake(FindOpenGroups()))
            {
                if (!BakeInto(group, recordUndo)) continue;
                changed++;
                AssetDatabase.SaveAssetIfDirty(group.Level);
            }
            return changed;
        }

        /// <summary>True when the level already holds what the group would bake now. Also true for a group without a level.</summary>
        public static bool IsUpToDate(TileMapGroup group) => !group || !group.Level || Bake(group).SameAs(group.Level.Terrain);

        /// <summary>Every group in the loaded scenes and in the open prefab stage, inactive ones included.</summary>
        internal static List<TileMapGroup> FindOpenGroups()
        {
            var groups = new List<TileMapGroup>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    groups.AddRange(root.GetComponentsInChildren<TileMapGroup>(true));
            }
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot)
                groups.AddRange(stage.prefabContentsRoot.GetComponentsInChildren<TileMapGroup>(true));
            return groups;
        }

        /// <summary>The groups whose level no other group in <paramref name="groups"/> also targets; logs an error for the others.</summary>
        internal static IEnumerable<TileMapGroup> GroupsSafeToBake(IEnumerable<TileMapGroup> groups)
        {
            foreach (var byLevel in groups.Where(group => group && group.Level).GroupBy(group => group.Level))
            {
                var members = byLevel.ToList();
                if (members.Count == 1)
                {
                    yield return members[0];
                    continue;
                }
                Debug.LogError(
                    $"Level '{byLevel.Key.name}' is the target of {members.Count} Tile Map Groups ({string.Join(", ", members.Select(DescribeSource))}). " +
                    "Not baked: give each group its own Level Definition.", byLevel.Key);
            }
        }

        /// <summary>Where a group lives, for people: "Scene.unity: Keyboard Grid/Grid" or "Prefab.prefab: Grid".</summary>
        internal static string DescribeSource(TileMapGroup group)
        {
            if (!group) return string.Empty;
            var path = TransformPath(group.transform);
            var stage = PrefabStageUtility.GetPrefabStage(group.gameObject);
            string container = stage != null ? stage.assetPath
                : EditorUtility.IsPersistent(group) ? AssetDatabase.GetAssetPath(group)
                : group.gameObject.scene.path;
            if (string.IsNullOrEmpty(container)) container = group.gameObject.scene.name;
            return $"{System.IO.Path.GetFileName(container)}: {path}";
        }

        static string TransformPath(Transform transform)
        {
            var names = new List<string>();
            for (var current = transform; current; current = current.parent) names.Add(current.name);
            names.Reverse();
            return string.Join("/", names);
        }

        // The box around every cell that some layer resolves to a type: trims the empty edges a tilemap's cellBounds keeps after erasing.
        static bool TryGetPaintedBounds(IReadOnlyList<TileMapLayer> layers, out RectInt bounds)
        {
            int xMin = int.MaxValue, yMin = int.MaxValue, xMax = int.MinValue, yMax = int.MinValue;
            foreach (var layer in layers)
            {
                var area = layer.Tilemap.cellBounds;
                for (int y = area.yMin; y < area.yMax; y++)
                    for (int x = area.xMin; x < area.xMax; x++)
                    {
                        if (!layer.Resolve(layer.Tilemap.GetTile(new Vector3Int(x, y, 0)))) continue;
                        xMin = Mathf.Min(xMin, x);
                        yMin = Mathf.Min(yMin, y);
                        xMax = Mathf.Max(xMax, x + 1);
                        yMax = Mathf.Max(yMax, y + 1);
                    }
            }
            bounds = xMin > xMax ? default : new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            return xMin <= xMax;
        }
    }
}
