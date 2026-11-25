using System.Collections.Generic;
using System.Linq;
using DetectiveDave.Levels;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    public static class PlayerSetup
    {
        const string DialogTitle = "Set Up Player";

        [MenuItem("Window/Detective Dave/Set Up Player")]
        static void SetUpFromWindowMenu() => SetUp();

        public static void SetUp()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Fail("Exit Play Mode before setting up the player. Nothing was changed.");
                return;
            }
            var error = ApplyOrError();
            if (error == null) Succeed(AssetDatabase.LoadAssetAtPath<Object>(PlayerSetupPaths.PrefabPath));
            else Fail(error);
        }

        public static string ApplyOrError()
        {
            var tiles = LoadTiles();
            var missing = FindMissingAssets(tiles);
            if (missing.Length > 0) return "Nothing was changed. Missing assets:\n" + string.Join("\n", missing);
            var settings = PlayerSettingsAsset.EnsureWithSprites(
                PlayerSpriteImport.ImportFrames(PlayerSetupPaths.WalkFramePattern),
                PlayerSpriteImport.ImportFrames(PlayerSetupPaths.ClimbFramePattern),
                PlayerSpriteImport.ImportDeath(),
                PlayerSetupPaths.DecorationPaths.Select(PlayerSpriteImport.ImportDecoration).ToArray());
            var prefab = PlayerPrefabBuilder.Ensure();
            var wiringError = PlayerSceneWiring.Wire(prefab, settings, tiles);
            AssetDatabase.SaveAssets();
            return wiringError ?? GuardSetup.ApplyOrError() ?? DogSetup.ApplyOrError() ?? LightingSetup.ApplyOrError() ?? MusicSetup.ApplyOrError() ?? CheatSetup.ApplyOrError();
        }

        static TileType[] LoadTiles() => PlayerSetupPaths.TileNames.Select(LoadTile).ToArray();

        static string[] FindMissingAssets(TileType[] tiles)
        {
            var missingTiles = PlayerSetupPaths.TileNames.Where((name, index) => !tiles[index]).Select(name => string.Format(PlayerSetupPaths.TilePattern, name));
            var missingFrames = MissingFrames(PlayerSetupPaths.WalkFramePattern).Concat(MissingFrames(PlayerSetupPaths.ClimbFramePattern)).Concat(MissingDeathSprite());
            var missingDecorations = PlayerSetupPaths.DecorationPaths.Where(path => !AssetImporter.GetAtPath(path));
            return missingTiles.Concat(missingFrames).Concat(missingDecorations).Concat(MissingScene()).ToArray();
        }

        static IEnumerable<string> MissingFrames(string pathPattern) =>
            Enumerable.Range(1, PlayerSetupPaths.FrameCount).Select(index => string.Format(pathPattern, index)).Where(path => !AssetImporter.GetAtPath(path));

        static IEnumerable<string> MissingDeathSprite() =>
            AssetImporter.GetAtPath(PlayerSetupPaths.DeathSpritePath) ? Enumerable.Empty<string>() : new[] { PlayerSetupPaths.DeathSpritePath };

        static IEnumerable<string> MissingScene() =>
            AssetDatabase.LoadAssetAtPath<SceneAsset>(PlayerSetupPaths.ScenePath) ? Enumerable.Empty<string>() : new[] { PlayerSetupPaths.ScenePath };

        static TileType LoadTile(string name) => AssetDatabase.LoadAssetAtPath<TileType>(string.Format(PlayerSetupPaths.TilePattern, name));

        static void Fail(string reason) => ShowProblem(DialogTitle, reason);

        internal static void ShowProblem(string title, string reason)
        {
            Debug.LogError(title + ": " + reason);
            EditorUtility.DisplayDialog(title, reason, "OK");
        }

        static void Succeed(Object prefab)
        {
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            EditorUtility.DisplayDialog(DialogTitle, "The player, the guards, the lighting and the music are set up in Game.unity. Press Play in the Game scene to try it.", "OK");
        }
    }
}
