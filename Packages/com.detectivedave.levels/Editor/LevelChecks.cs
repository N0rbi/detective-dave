using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Editor
{
    // The package's own checks. Found and run by LevelValidator like any game check.

    /// <summary>Key Art, Shape tilemap, Background renderer, and where the level file lives.</summary>
    internal sealed class LevelSetupCheck : ILevelCheck
    {
        const string LevelsFolder = "Assets/Levels";
        const float ShapeCellSize = 0.25f;
        const int BackgroundSortingOrder = -100;

        public void Check(Level level, List<LevelIssue> issues)
        {
            bool editable = !EditorUtility.IsPersistent(level);

            if (!level.KeyArt)
            {
                var fallback = LevelAssets.FindDefaultKeyArt();
                issues.Add(new LevelIssue(MessageType.Error, "No Key Art: keys are drawn plain. Pick one in 'Key Art'.", level,
                    fallback ? $"Use '{fallback.name}'" : null, fallback ? () => Set(level, "keyArt", fallback) : null));
            }

            if (!level.Shape)
            {
                var child = level.GetComponentInChildren<Tilemap>(true);
                if (child)
                    issues.Add(new LevelIssue(MessageType.Error, "No Shape tilemap is set, so the level has no keys.", level, $"Use '{child.name}'", () => Set(level, "shape", child)));
                else
                    issues.Add(new LevelIssue(MessageType.Error, "The level has no Shape tilemap to paint on.", level,
                        editable ? "Create Shape" : null, editable ? () => CreateShape(level) : null));
            }
            else if (!level.Shape.transform.IsChildOf(level.transform))
                issues.Add(new LevelIssue(MessageType.Warning, $"The Shape tilemap '{level.Shape.name}' is not inside this level, so it is not saved with it. Use a tilemap under the level.", level.Shape));

            if (!level.BackgroundRenderer)
                issues.Add(new LevelIssue(MessageType.Info, "No Background renderer: the Key Art's default background is used, if it has one.", level,
                    editable ? "Add Background" : null, editable ? () => CreateBackground(level) : null));

            var path = AssetPathOf(level);
            if (!string.IsNullOrEmpty(path) && !path.StartsWith(LevelsFolder + "/"))
                issues.Add(new LevelIssue(MessageType.Warning, $"This level is in {System.IO.Path.GetDirectoryName(path)}. Levels belong in {LevelsFolder}.", level,
                    $"Move to {LevelsFolder}", () => MoveToLevels(path)));
        }

        /// <summary>The prefab file a level is edited from: the open prefab stage's, or the asset's own. Null for a level in a scene.</summary>
        internal static string AssetPathOf(Level level)
        {
            var stage = PrefabStageUtility.GetPrefabStage(level.gameObject);
            if (stage != null) return stage.assetPath;
            return EditorUtility.IsPersistent(level) ? AssetDatabase.GetAssetPath(level) : null;
        }

        internal static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }

        static void CreateShape(Level level)
        {
            var grid = new GameObject("Grid", typeof(Grid));
            grid.GetComponent<Grid>().cellSize = new Vector3(ShapeCellSize, ShapeCellSize, 0f);
            Undo.RegisterCreatedObjectUndo(grid, "Create Shape");
            Undo.SetTransformParent(grid.transform, level.transform, "Create Shape");
            grid.transform.localPosition = Vector3.zero;
            var shape = new GameObject("Shape", typeof(Tilemap), typeof(TilemapRenderer));
            Undo.RegisterCreatedObjectUndo(shape, "Create Shape");
            Undo.SetTransformParent(shape.transform, grid.transform, "Create Shape");
            shape.transform.localPosition = Vector3.zero;
            Set(level, "shape", shape.GetComponent<Tilemap>());
        }

        static void CreateBackground(Level level)
        {
            var background = new GameObject("Background", typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(background, "Add Background");
            Undo.SetTransformParent(background.transform, level.transform, "Add Background");
            background.transform.localPosition = Vector3.zero;
            background.GetComponent<SpriteRenderer>().sortingOrder = BackgroundSortingOrder;
            Set(level, "background", background.GetComponent<SpriteRenderer>());
        }

        static void MoveToLevels(string path)
        {
            LevelAssets.EnsureFolder(LevelsFolder);
            var target = AssetDatabase.GenerateUniqueAssetPath($"{LevelsFolder}/{System.IO.Path.GetFileName(path)}");
            var error = AssetDatabase.MoveAsset(path, target);
            if (!string.IsNullOrEmpty(error)) Debug.LogError($"Could not move {path} to {target}: {error}");
        }
    }

    /// <summary>What is painted: tiles that are not a kind of key, tiles whose kind was deleted, kinds the Key Art cannot draw.</summary>
    internal sealed class LevelTilesCheck : ILevelCheck
    {
        public void Check(Level level, List<LevelIssue> issues)
        {
            var shape = level.Shape;
            if (!shape) return;

            var foreign = new List<Vector3Int>();
            var missing = new List<Vector3Int>();
            var used = new HashSet<TileType>();
            foreach (var cell in shape.cellBounds.allPositionsWithin)
            {
                var tile = shape.GetTile(cell);
                if (tile is TileType type) used.Add(type);
                else if (tile) foreign.Add(cell);
                else if (shape.GetSprite(cell)) missing.Add(cell);   // the tile asset is gone, the cell still holds its picture
            }

            if (foreign.Count > 0)
                issues.Add(new LevelIssue(MessageType.Warning, $"{foreign.Count} painted cell(s) hold tiles that are not a kind of key, and are ignored. Paint with the Level Tiles palette.", shape,
                    "Erase them", () => Erase(shape, foreign)));
            if (missing.Count > 0)
                issues.Add(new LevelIssue(MessageType.Error, $"{missing.Count} cell(s) hold a kind of key that was deleted. Erase them and paint again.", shape,
                    "Erase them", () => Erase(shape, missing)));
            if (used.Count == 0 && foreign.Count == 0 && missing.Count == 0)
                issues.Add(new LevelIssue(MessageType.Info, "Nothing painted yet: pick a kind of key in the Level Tiles palette and paint the keys.", shape));

            var keyArt = level.KeyArt;
            if (!keyArt) return;
            foreach (var type in used.OrderBy(type => type.name))
            {
                if (keyArt.FindStyle(type) == null)
                {
                    issues.Add(new LevelIssue(MessageType.Warning, $"'{type.name}' is painted but '{keyArt.name}' has no style for it: those keys are drawn plain.", keyArt,
                        "Open Key Art", () => Selection.activeObject = keyArt));
                    continue;
                }
                var shapes = LevelValidator.MissingStandardShapes(keyArt, type);
                if (shapes.Count > 0)
                    issues.Add(new LevelIssue(MessageType.Warning, $"'{keyArt.name}' has no '{type.name}' sprite for: {string.Join(", ", shapes)}. Those keys are drawn as flat squares.", keyArt,
                        "Open Key Art", () => Selection.activeObject = keyArt));
            }
        }

        static void Erase(Tilemap shape, List<Vector3Int> cells)
        {
            Undo.RecordObject(shape, "Erase Tiles");
            foreach (var cell in cells) shape.SetTile(cell, null);
        }
    }

    /// <summary>Every kind of key is in the Level Tiles palette, so designers can paint it.</summary>
    internal sealed class LevelPaletteCheck : ILevelCheck
    {
        public void Check(Level level, List<LevelIssue> issues)
        {
            var types = LevelAssets.FindAllTileTypes();
            var palette = LevelPalette.Find();
            if (!palette)
            {
                issues.Add(new LevelIssue(MessageType.Warning, $"There is no Level Tiles palette at {LevelPalette.DefaultPath}.", null,
                    "Create Level Tiles", () =>
                    {
                        var created = LevelPalette.Create();
                        foreach (var type in LevelAssets.FindAllTileTypes()) LevelPalette.Add(created, type);
                    }));
                return;
            }
            foreach (var type in types.Where(type => !LevelPalette.Contains(palette, type)))
                issues.Add(new LevelIssue(MessageType.Warning, $"'{type.name}' is not in the Level Tiles palette, so it cannot be picked for painting.", type,
                    "Add to Level Tiles", () => LevelPalette.Add(palette, type)));
        }
    }
}
