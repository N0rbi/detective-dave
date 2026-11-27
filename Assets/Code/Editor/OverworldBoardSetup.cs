using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class OverworldBoardSetup
    {
        const string DialogTitle = "Set Up Overworld Board";
        const string DeskTopField = "deskTopSprite";

        [MenuItem("Window/Detective Dave/Set Up Overworld Board")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the board. Nothing was changed.");
                return;
            }
            var settings = AssetDatabase.LoadAssetAtPath<OverworldSettings>(OverworldSetupPaths.SettingsPath);
            if (!settings)
            {
                PlayerSetup.ShowProblem(DialogTitle, "There is no Overworld Settings asset yet. Run Window > Detective Dave > Set Up Overworld first. Nothing was changed.");
                return;
            }
            var error = ApplyOrError(settings, out var changed);
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(DialogTitle, changed ? "Desk Top Sprite is now keyboard_stealth_board." : "Nothing needed changing. Desk Top Sprite is not a placeholder.", "OK");
        }

        public static string ApplyOrError(OverworldSettings settings, out bool changed)
        {
            changed = false;
            if (!AssetImporter.GetAtPath(OverworldSetupPaths.BoardPath)) return "Nothing was changed. The board picture is missing:\n" + OverworldSetupPaths.BoardPath + "\nMove keyboard_stealth_board.png there from Assets/Staging.";
            var board = PlayerSpriteImport.ImportDecoration(OverworldSetupPaths.BoardPath);
            var serialized = new SerializedObject(settings);
            var property = serialized.FindProperty(DeskTopField);
            var current = property.objectReferenceValue;
            if (!DeskTopChoice.ShouldReplace(current ? current.name : null)) return null;
            property.objectReferenceValue = board;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            changed = true;
            return null;
        }
    }
}
