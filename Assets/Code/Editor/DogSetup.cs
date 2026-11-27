using System.Linq;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    static class DogSetup
    {
        const string DialogTitle = "Set Up Dogs";

        [MenuItem("Window/Detective Dave/Set Up Dogs")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the dogs. Nothing was changed.");
                return;
            }
            var error = ApplyOrError();
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DogSetupPaths.SettingsPath);
            EditorUtility.DisplayDialog(DialogTitle, "The dogs are set up. Every Gap tile is now a dog.\nTune them in Assets/Dogs/Dog Settings.", "OK");
        }

        public static string ApplyOrError()
        {
            var missing = Paths(DogSetupPaths.AngryFramePattern).Concat(Paths(DogSetupPaths.CalmFramePattern)).Where(path => !AssetImporter.GetAtPath(path)).ToArray();
            if (missing.Length > 0) return "Nothing was changed for the dogs. Missing assets:\n" + string.Join("\n", missing);
            var settings = DogSettingsAsset.EnsureWithSprites(
                Paths(DogSetupPaths.AngryFramePattern).Select(PlayerSpriteImport.ImportDecoration).ToArray(),
                Paths(DogSetupPaths.CalmFramePattern).Select(PlayerSpriteImport.ImportDecoration).ToArray());
            var error = DogSceneWiring.Wire(settings);
            AssetDatabase.SaveAssets();
            return error;
        }

        static string[] Paths(string pattern) =>
            Enumerable.Range(1, DogSetupPaths.FrameCount).Select(index => string.Format(pattern, index)).ToArray();
    }
}
