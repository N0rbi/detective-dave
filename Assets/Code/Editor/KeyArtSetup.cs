using System.Collections.Generic;
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
            var tile = AssetDatabase.LoadAssetAtPath<TileType>(KeyArtSetupPaths.WalkableTilePath);
            if (!tile) return $"Nothing was changed. The Walkable tile type is missing at {KeyArtSetupPaths.WalkableTilePath}.";
            var box = Classify(KeyArtSetupPaths.StagedBoxPath, KeyArtSetupPaths.BoxPath);
            var enter = Classify(KeyArtSetupPaths.StagedEnterPath, KeyArtSetupPaths.EnterPath);
            var blocked = Blockers(box, enter);
            if (blocked.Count > 0) return "Nothing was changed.\n" + string.Join("\n", blocked);
            var moveError = Move(box, KeyArtSetupPaths.StagedBoxPath, KeyArtSetupPaths.BoxPath) ?? Move(enter, KeyArtSetupPaths.StagedEnterPath, KeyArtSetupPaths.EnterPath);
            if (moveError != null) return moveError;
            var importError = Configure(KeyArtSetupPaths.BoxPath, out var borderPixels) ?? Configure(KeyArtSetupPaths.EnterPath, out _);
            if (importError != null) return importError;
            var assigned = AssignWalkable(tile, LoadSprite(KeyArtSetupPaths.BoxPath), LoadSprite(KeyArtSetupPaths.EnterPath));
            AssetDatabase.SaveAssets();
            summary = Summary(borderPixels, assigned);
            return null;
        }

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
            if (state == KeyArtSlotState.Conflict) blockers.Add($"{stagedName} is in {KeyArtSetupPaths.StagingFolder} but {targetName} already exists in {KeyArtSetupPaths.WalkableFolder}. Remove one of them first.");
        }

        static string Move(KeyArtSlotState state, string from, string to)
        {
            if (state != KeyArtSlotState.NeedsMove) return null;
            if (!AssetDatabase.IsValidFolder(KeyArtSetupPaths.WalkableFolder)) return $"The folder {KeyArtSetupPaths.WalkableFolder} is missing.";
            var error = AssetDatabase.MoveAsset(from, to);
            return string.IsNullOrEmpty(error) ? null : $"{from} could not be moved: {error}";
        }

        static string Configure(string path, out int borderPixels)
        {
            borderPixels = 0;
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return $"{path} is not an imported texture.";
            if (!IsConfigured(importer)) ApplyImportSettings(importer);
            var sprite = LoadSprite(path);
            if (!sprite) return $"{path} did not import as a sprite.";
            if (path != KeyArtSetupPaths.BoxPath) return null;
            borderPixels = KeyArtBorder.RimPixels((int)sprite.rect.width, (int)sprite.rect.height);
            return BoxSpriteCheck.TrySetBorder(sprite, borderPixels, out var message) ? null : message;
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

        static string Summary(int borderPixels, List<string> assigned)
        {
            var where = assigned.Count == 0 ? "No Key Art has a Walkable style, so nothing was assigned." : "Box and Enter are set on the Walkable style of: " + string.Join(", ", assigned) + ".";
            return $"The Walkable key art is in {KeyArtSetupPaths.WalkableFolder} and imported with a {borderPixels} px 9-slice border on the Box.\n\n{where}\n\nOn the Walkable style the tint was set to white (the art is already coloured) and the old per-size sprites were cleared, because they would be drawn instead of the Box. Other styles and their Enter sprites were not touched.";
        }
    }
}
