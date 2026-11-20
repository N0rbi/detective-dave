using System.Collections.Generic;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.EditorTools
{
    [InitializeOnLoad]
    public static class LevelHeightMigration
    {
        const string DrawnStretchField = "drawnHeightStretch";

        static LevelHeightMigration() => EditorApplication.delayCall += MigrateAllLevelAssets;

        public static bool NeedsMigration(Level level) =>
            level && !Mathf.Approximately(level.DrawnHeightStretch, LevelSettings.instance.KeyHeightStretch);

        public static void Migrate(Level level)
        {
            float target = LevelSettings.instance.KeyHeightStretch;
            float ratio = target / level.DrawnHeightStretch;
            foreach (var tilemap in level.GetComponentsInChildren<Tilemap>(true)) StretchTilemap(tilemap, level.BoardSpace, ratio);
            StampDrawnStretch(level, target);
        }

        public static void StretchTilemap(Tilemap tilemap, Transform boardSpace, float ratio)
        {
            tilemap.CompressBounds();
            var bounds = tilemap.cellBounds;
            if (bounds.size.x <= 0 || bounds.size.y <= 0) return;
            var old = ReadTiles(tilemap, bounds);
            var stretched = ScaledRange(tilemap, boardSpace, bounds, ratio);
            var cells = new List<Vector3Int>();
            var tiles = new List<TileBase>();
            foreach (var cell in stretched.allPositionsWithin)
            {
                var source = SourceCell(tilemap, boardSpace, cell, ratio);
                if (!old.TryGetValue(source, out var tile)) continue;
                cells.Add(cell);
                tiles.Add(tile);
            }
            tilemap.ClearAllTiles();
            tilemap.SetTiles(cells.ToArray(), tiles.ToArray());
        }

        static Dictionary<Vector3Int, TileBase> ReadTiles(Tilemap tilemap, BoundsInt bounds)
        {
            var tiles = new Dictionary<Vector3Int, TileBase>();
            foreach (var cell in bounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(cell);
                if (tile) tiles[cell] = tile;
            }
            return tiles;
        }

        static BoundsInt ScaledRange(Tilemap tilemap, Transform boardSpace, BoundsInt bounds, float ratio)
        {
            var low = ScaledBoardPoint(boardSpace, tilemap.CellToWorld(bounds.min), ratio);
            var high = ScaledBoardPoint(boardSpace, tilemap.CellToWorld(bounds.max), ratio);
            var a = tilemap.WorldToCell(boardSpace.TransformPoint(low));
            var b = tilemap.WorldToCell(boardSpace.TransformPoint(high));
            var min = Vector3Int.Min(a, b) - Vector3Int.one;
            var max = Vector3Int.Max(a, b) + Vector3Int.one;
            return new BoundsInt(min.x, min.y, 0, max.x - min.x + 1, max.y - min.y + 1, 1);
        }

        static Vector3 ScaledBoardPoint(Transform boardSpace, Vector3 world, float ratio)
        {
            var board = boardSpace.InverseTransformPoint(world);
            return new Vector3(board.x, board.y * ratio, board.z);
        }

        static Vector3Int SourceCell(Tilemap tilemap, Transform boardSpace, Vector3Int cell, float ratio)
        {
            var board = boardSpace.InverseTransformPoint(tilemap.GetCellCenterWorld(cell));
            var source = tilemap.WorldToCell(boardSpace.TransformPoint(new Vector3(board.x, board.y / ratio, board.z)));
            return new Vector3Int(source.x, source.y, 0);
        }

        static void StampDrawnStretch(Level level, float stretch)
        {
            var serialized = new SerializedObject(level);
            serialized.FindProperty(DrawnStretchField).floatValue = stretch;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void MigrateAllLevelAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            var folder = LevelSettings.instance.LevelsFolder;
            if (!AssetDatabase.IsValidFolder(folder)) return;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder })) MigrateAsset(AssetDatabase.GUIDToAssetPath(guid));
        }

        static void MigrateAsset(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var level = root.GetComponent<Level>();
                if (!NeedsMigration(level)) return;
                Migrate(level);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[Levels] '{path}' was redrawn for keys {LevelSettings.instance.KeyHeightStretch}x taller.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }

    public sealed class KeyHeightCheck : ILevelCheck
    {
        public void Check(Level level, List<LevelIssue> issues)
        {
            if (!LevelHeightMigration.NeedsMigration(level)) return;
            var message = $"This level was painted for keys {level.DrawnHeightStretch}x tall; the keys are now {LevelSettings.instance.KeyHeightStretch}x tall, so paint no longer lines up with the keys.";
            bool editable = PrefabStageUtility.GetPrefabStage(level.gameObject) != null;
            issues.Add(editable
                ? new LevelIssue(MessageType.Warning, message, level, "Fit paint to keys", () => LevelHeightMigration.Migrate(level))
                : new LevelIssue(MessageType.Warning, message, level));
        }
    }
}
