using System;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    [Flags]
    internal enum BoxSpriteProblem
    {
        None = 0,
        NoBorder = 1,
        NotFullRect = 2,
    }

    internal static class BoxSpriteCheck
    {
        internal static BoxSpriteProblem Classify(Vector4 borderPixels, SpriteMeshType meshType)
        {
            var problem = BoxSpriteProblem.None;
            if (!HasBorder(borderPixels)) problem |= BoxSpriteProblem.NoBorder;
            if (meshType != SpriteMeshType.FullRect) problem |= BoxSpriteProblem.NotFullRect;
            return problem;
        }

        internal static bool HasBorder(Vector4 borderPixels) =>
            borderPixels.x > 0f || borderPixels.y > 0f || borderPixels.z > 0f || borderPixels.w > 0f;

        internal static Vector4 BorderOf(int pixels) => new Vector4(pixels, pixels, pixels, pixels);

        internal static int SuggestedBorderPixels(int spriteWidth, int spriteHeight) =>
            Mathf.Max(1, Mathf.Min(spriteWidth, spriteHeight) / 3);

        internal static bool NeedsChange(Vector4 currentBorder, SpriteMeshType meshType, int borderPixels) =>
            currentBorder != BorderOf(borderPixels) || meshType != SpriteMeshType.FullRect;

        internal static string Describe(BoxSpriteProblem problem)
        {
            if (problem == BoxSpriteProblem.None) return string.Empty;
            var parts = new System.Collections.Generic.List<string>();
            if ((problem & BoxSpriteProblem.NoBorder) != 0) parts.Add("it has no 9-slice border");
            if ((problem & BoxSpriteProblem.NotFullRect) != 0) parts.Add("its Mesh Type is not Full Rect");
            return string.Join(" and ", parts);
        }

        internal static BoxSpriteProblem ProblemOf(Sprite sprite)
        {
            if (!sprite) return BoxSpriteProblem.None;
            var importer = ImporterOf(sprite);
            var meshType = importer ? MeshTypeOf(importer) : SpriteMeshType.FullRect;
            return Classify(sprite.border, meshType);
        }

        internal static TextureImporter ImporterOf(Sprite sprite)
        {
            var path = AssetDatabase.GetAssetPath(sprite);
            return string.IsNullOrEmpty(path) ? null : AssetImporter.GetAtPath(path) as TextureImporter;
        }

        internal static SpriteMeshType MeshTypeOf(TextureImporter importer)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            return settings.spriteMeshType;
        }

        internal static bool CanSetBorder(Sprite sprite, out string reason)
        {
            reason = string.Empty;
            var importer = sprite ? ImporterOf(sprite) : null;
            if (!importer)
            {
                reason = "The sprite is not an imported texture file.";
                return false;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                reason = "The texture is a sprite sheet. Set the border in the Sprite Editor.";
                return false;
            }
            return true;
        }

        internal static bool TrySetBorder(Sprite sprite, int borderPixels, out string message)
        {
            if (!CanSetBorder(sprite, out message)) return false;
            var importer = ImporterOf(sprite);
            if (!NeedsChange(importer.spriteBorder, MeshTypeOf(importer), borderPixels))
            {
                message = "Already set.";
                return true;
            }
            importer.spriteBorder = BorderOf(borderPixels);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            message = $"Border set to {borderPixels} px and Mesh Type to Full Rect.";
            return true;
        }
    }
}
