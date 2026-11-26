using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public static class SmokingDetectiveSetup
    {
        const string DialogTitle = "Set Up Smoking Detective";

        [MenuItem("Window/Detective Dave/Set Up Smoking Detective")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the smoking detective. Nothing was changed.");
                return;
            }
            var changes = new List<string>();
            var error = ApplyOrError(changes.Add);
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            EditorUtility.DisplayDialog(DialogTitle, SetupReport.Summary(changes, "The Smoking Detective is already in Overworld.unity."), "OK");
        }

        public static string ApplyOrError(System.Action<string> note)
        {
            AssetDatabase.Refresh();
            var missing = new[] { OverworldSetupPaths.SmokingBodyPath, OverworldSetupPaths.SmokingArmPath }.Where(path => !AssetImporter.GetAtPath(path)).ToArray();
            if (missing.Length > 0) return "Nothing was changed. Missing assets:\n" + string.Join("\n", missing);
            var sprites = new SmokingSprites(PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.SmokingBodyPath), PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.SmokingArmPath));
            return OverworldSceneBuilder.EditOrError(scene => SmokingDetectiveWiring.Ensure(scene, sprites, note));
        }
    }
}
