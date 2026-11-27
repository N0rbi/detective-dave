using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class KeyArtSetup
    {
        const string DialogTitle = "Set Up Key Art";

        [MenuItem("Window/Detective Dave/Set Up Key Art")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the key art. Nothing was changed.");
                return;
            }
            AssetDatabase.Refresh();
            var error = ApplyOrError(out var summary);
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            EditorUtility.DisplayDialog(DialogTitle, summary, "OK");
        }

        public static string ApplyOrError(out string summary)
        {
            summary = string.Empty;
            var error = ApplyWalkableOrError(out var walkableSummary);
            if (error != null) return error;
            var lines = new List<string> { walkableSummary };
            foreach (var row in KeyArtTileTable.Boxes) lines.Add(ApplyBox(row));
            foreach (var tileName in KeyArtTileTable.TilesDrawnAsWalkable) lines.Add(ApplyWalkableLook(tileName));
            lines.Add(UnassignedLine());
            AssetDatabase.SaveAssets();
            summary = string.Join("\n\n", lines);
            return null;
        }

        static string ApplyWalkableOrError(out string summary)
        {
            summary = string.Empty;
            var tile = AssetDatabase.LoadAssetAtPath<TileType>(KeyArtSetupPaths.WalkableTilePath);
            if (!tile) return $"Nothing was changed. The Walkable tile type is missing at {KeyArtSetupPaths.WalkableTilePath}.";
            var box = Classify(KeyArtSetupPaths.StagedBoxPath, KeyArtSetupPaths.BoxPath);
            var enter = Classify(KeyArtSetupPaths.StagedEnterPath, KeyArtSetupPaths.EnterPath);
            var blocked = Blockers(box, enter);
            if (blocked.Count > 0) return "Nothing was changed.\n" + string.Join("\n", blocked);
            var moveError = Move(box, KeyArtSetupPaths.StagedBoxPath, KeyArtSetupPaths.BoxPath, KeyArtSetupPaths.WalkableFolder) ?? Move(enter, KeyArtSetupPaths.StagedEnterPath, KeyArtSetupPaths.EnterPath, KeyArtSetupPaths.WalkableFolder);
            if (moveError != null) return moveError;
            var importError = Configure(KeyArtSetupPaths.BoxPath, out var borderPixels) ?? Configure(KeyArtSetupPaths.EnterPath, out _);
            if (importError != null) return importError;
            var assigned = AssignWalkable(tile, LoadSprite(KeyArtSetupPaths.BoxPath), LoadSprite(KeyArtSetupPaths.EnterPath));
            summary = Summary(borderPixels, assigned);
            return null;
        }

        static string ApplyBox(KeyArtTileRow row)
        {
            var tile = AssetDatabase.LoadAssetAtPath<TileType>(row.TilePath);
            if (!tile) return $"{row.TileName}: skipped, the tile type is missing at {row.TilePath}.";
            var state = Classify(row.StagedPath, row.TargetPath);
            if (state == KeyArtSlotState.Missing) return $"{row.TileName}: skipped, {row.StagedName} is not in {KeyArtSetupPaths.StagingFolder} and {row.TargetName} is not in {row.Folder}.";
            var moveError = Move(state, row.StagedPath, row.TargetPath, row.Folder);
            if (moveError != null) return $"{row.TileName}: skipped, {moveError}";
            var configureError = ConfigureBox(row, out var applied);
            if (configureError != null) return $"{row.TileName}: skipped, {configureError}";
            var assigned = AssignBox(tile, LoadSprite(row.TargetPath));
            var where = assigned.Count == 0 ? "No Key Art has this style, so nothing was assigned." : "Box set on: " + string.Join(", ", assigned) + ".";
            var change = state == KeyArtSlotState.NeedsReplace ? $"updated: {row.TargetName} (new pixels from {row.StagedName}, same file and links)" : $"{row.StagedName} is now {row.TargetName}";
            return $"{row.TileName}: {change} in {row.Folder}, border {applied.x:0}, {applied.y:0}, {applied.z:0}, {applied.w:0} px (left, bottom, right, top). {where} Tint set to white and the per-size sprites cleared; the Enter was left as it is.";
        }

        static string ApplyWalkableLook(string tileName)
        {
            var path = KeyArtSetupPaths.TilesFolder + "/" + tileName + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<TileType>(path);
            if (!tile) return $"{tileName}: skipped, the tile type is missing at {path}.";
            var box = LoadSprite(KeyArtSetupPaths.BoxPath);
            if (!box) return $"{tileName}: skipped, {KeyArtSetupPaths.BoxPath} is missing. Set up the Walkable art first.";
            var assigned = AssignBox(tile, box);
            var where = assigned.Count == 0 ? "No Key Art has this style, so nothing was assigned." : "Box set on: " + string.Join(", ", assigned) + ".";
            return $"{tileName}: now uses the Walkable box art ({KeyArtSetupPaths.BoxName}, same file, nothing copied), tint white. {where}";
        }

        static string UnassignedLine() =>
            $"{KeyArtTileTable.UnassignedStagedName}: not assigned to any tile type. It is already part of the climb art, so it stays in {KeyArtSetupPaths.StagingFolder}.";

        static KeyArtSlotState Classify(string stagedPath, string targetPath) =>
            KeyArtSlot.Classify(Exists(stagedPath), Exists(targetPath));

        static bool Exists(string path) => AssetDatabase.LoadMainAssetAtPath(path);

        static List<string> Blockers(KeyArtSlotState box, KeyArtSlotState enter)
        {
            var blockers = new List<string>();
            AddBlocker(blockers, box, KeyArtSetupPaths.StagedBoxName, KeyArtSetupPaths.BoxName);
            AddBlocker(blockers, enter, KeyArtSetupPaths.StagedEnterName, KeyArtSetupPaths.EnterName);
            return blockers;
        }

        static void AddBlocker(List<string> blockers, KeyArtSlotState state, string stagedName, string targetName)
        {
            if (state == KeyArtSlotState.Missing) blockers.Add($"{stagedName} is not in {KeyArtSetupPaths.StagingFolder} and {targetName} is not in {KeyArtSetupPaths.WalkableFolder}.");
        }

        static string Move(KeyArtSlotState state, string from, string to, string folder)
        {
            if (state == KeyArtSlotState.NeedsReplace) return ReplacePixels(from, to);
            if (state != KeyArtSlotState.NeedsMove) return null;
            if (!AssetDatabase.IsValidFolder(folder)) return $"The folder {folder} is missing.";
            var error = AssetDatabase.MoveAsset(from, to);
            return string.IsNullOrEmpty(error) ? null : $"{from} could not be moved: {error}";
        }

        static string ReplacePixels(string from, string to)
        {
            try
            {
                File.Copy(from, to, true);
                AssetDatabase.ImportAsset(to, ImportAssetOptions.ForceUpdate);
                SetAsideStagedCopy(from);
                return null;
            }
            catch (Exception exception)
            {
                return $"{from} could not replace {to}: {exception.Message}";
            }
        }

        static void SetAsideStagedCopy(string stagedPath)
        {
            Directory.CreateDirectory(KeyArtSetupPaths.ReplacedStagingFolder);
            var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var name = Path.GetFileName(stagedPath);
            File.Move(stagedPath, Path.Combine(KeyArtSetupPaths.ReplacedStagingFolder, stamp + "-" + name));
            var metaPath = stagedPath + ".meta";
            if (File.Exists(metaPath)) File.Move(metaPath, Path.Combine(KeyArtSetupPaths.ReplacedStagingFolder, stamp + "-" + name + ".meta"));
            AssetDatabase.Refresh();
        }

        static string Configure(string path, out int borderPixels)
        {
            borderPixels = 0;
            var error = EnsureImported(path, out var sprite);
            if (error != null) return error;
            if (path != KeyArtSetupPaths.BoxPath) return null;
            borderPixels = KeyArtBorder.RimPixels((int)sprite.rect.width, (int)sprite.rect.height);
            return BoxSpriteCheck.TrySetBorder(sprite, borderPixels, out var message) ? null : message;
        }

        static string ConfigureBox(KeyArtTileRow row, out Vector4 border)
        {
            border = default;
            var error = EnsureImported(row.TargetPath, out var sprite);
            if (error != null) return error;
            border = KeyArtBorder.BoxBorder((int)sprite.rect.width, (int)sprite.rect.height, row.TopBorderPixels);
            return BoxSpriteCheck.TrySetBorder(sprite, border, out var message) ? null : message;
        }

        static string EnsureImported(string path, out Sprite sprite)
        {
            sprite = null;
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return $"{path} is not an imported texture.";
            if (!IsConfigured(importer)) ApplyImportSettings(importer);
            sprite = LoadSprite(path);
            return sprite ? null : $"{path} did not import as a sprite.";
        }

        static bool IsConfigured(TextureImporter importer) =>
            importer.textureType == TextureImporterType.Sprite
            && importer.spriteImportMode == SpriteImportMode.Single
            && Mathf.Approximately(importer.spritePixelsPerUnit, KeyArtSetupPaths.PixelsPerUnit)
            && importer.filterMode == FilterMode.Bilinear
            && !importer.mipmapEnabled
            && importer.alphaIsTransparency
            && importer.textureCompression == TextureImporterCompression.Uncompressed
            && importer.maxTextureSize == KeyArtSetupPaths.MaxTextureSize
            && BoxSpriteCheck.MeshTypeOf(importer) == SpriteMeshType.FullRect;

        static void ApplyImportSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = KeyArtSetupPaths.PixelsPerUnit;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = KeyArtSetupPaths.MaxTextureSize;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        static List<string> AssignWalkable(TileType tile, Sprite box, Sprite enter)
        {
            var assigned = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:KeyArt"))
            {
                var keyArt = AssetDatabase.LoadAssetAtPath<KeyArt>(AssetDatabase.GUIDToAssetPath(guid));
                if (keyArt && AssignStyle(keyArt, tile, box, enter)) assigned.Add(keyArt.name);
            }
            return assigned;
        }

        static bool AssignStyle(KeyArt keyArt, TileType tile, Sprite box, Sprite enter)
        {
            var serialized = new SerializedObject(keyArt);
            var styles = serialized.FindProperty("tileStyles");
            for (int index = 0; index < styles.arraySize; index++)
            {
                var style = styles.GetArrayElementAtIndex(index);
                if (style.FindPropertyRelative("type").objectReferenceValue != tile) continue;
                style.FindPropertyRelative("box").objectReferenceValue = box;
                style.FindPropertyRelative("enter").objectReferenceValue = enter;
                style.FindPropertyRelative("tint").colorValue = Color.white;
                style.FindPropertyRelative("shapes").arraySize = 0;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(keyArt);
                return true;
            }
            return false;
        }

        static List<string> AssignBox(TileType tile, Sprite box)
        {
            var assigned = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:KeyArt"))
            {
                var keyArt = AssetDatabase.LoadAssetAtPath<KeyArt>(AssetDatabase.GUIDToAssetPath(guid));
                if (keyArt && AssignBoxStyle(keyArt, tile, box)) assigned.Add(keyArt.name);
            }
            return assigned;
        }

        static bool AssignBoxStyle(KeyArt keyArt, TileType tile, Sprite box)
        {
            var serialized = new SerializedObject(keyArt);
            var styles = serialized.FindProperty("tileStyles");
            for (int index = 0; index < styles.arraySize; index++)
            {
                var style = styles.GetArrayElementAtIndex(index);
                if (style.FindPropertyRelative("type").objectReferenceValue != tile) continue;
                style.FindPropertyRelative("box").objectReferenceValue = box;
                style.FindPropertyRelative("tint").colorValue = Color.white;
                RemoveSingleKeyShapes(style.FindPropertyRelative("shapes"));
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(keyArt);
                return true;
            }
            return false;
        }

        static void RemoveSingleKeyShapes(SerializedProperty shapes)
        {
            var wholeKeyFlags = new List<bool>();
            for (int index = 0; index < shapes.arraySize; index++)
                wholeKeyFlags.Add(shapes.GetArrayElementAtIndex(index).FindPropertyRelative("wholeKey").boolValue);
            foreach (var index in KeyArtShapeFilter.SingleKeyIndicesLastFirst(wholeKeyFlags))
                shapes.DeleteArrayElementAtIndex(index);
        }

        static string Summary(int borderPixels, List<string> assigned)
        {
            var where = assigned.Count == 0 ? "No Key Art has a Walkable style, so nothing was assigned." : "Box and Enter are set on the Walkable style of: " + string.Join(", ", assigned) + ".";
            return $"The Walkable key art is in {KeyArtSetupPaths.WalkableFolder} and imported with a {borderPixels} px 9-slice border on the Box.\n\n{where}\n\nOn the Walkable style the tint was set to white (the art is already coloured) and the old per-size sprites were cleared, because they would be drawn instead of the Box. Other styles and their Enter sprites were not touched.";
        }
    }
}
