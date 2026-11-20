using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class PlayerSpriteImport
    {
        const float SpritePixelsPerUnit = 800f;

        public static Sprite[] ImportFrames(string pathPattern) =>
            Enumerable.Range(1, PlayerSetupPaths.FrameCount).Select(index => ImportFrame(string.Format(pathPattern, index))).ToArray();

        public static Sprite ImportDecoration(string path) => Import(path, SpriteAlignment.Center);

        static Sprite ImportFrame(string path) => Import(path, SpriteAlignment.BottomCenter);

        static Sprite Import(string path, SpriteAlignment alignment)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            if (!IsConfigured(importer, textureSettings, alignment))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = SpritePixelsPerUnit;
                textureSettings.spriteAlignment = (int)alignment;
                importer.SetTextureSettings(textureSettings);
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static bool IsConfigured(TextureImporter importer, TextureImporterSettings textureSettings, SpriteAlignment alignment) =>
            importer.textureType == TextureImporterType.Sprite
            && importer.spriteImportMode == SpriteImportMode.Single
            && Mathf.Approximately(importer.spritePixelsPerUnit, SpritePixelsPerUnit)
            && textureSettings.spriteAlignment == (int)alignment;
    }
}
