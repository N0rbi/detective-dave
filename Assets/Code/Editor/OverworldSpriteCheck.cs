using System.Collections.Generic;
using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    public static class OverworldSpriteCheck
    {
        [MenuItem("Window/Detective Dave/Check Overworld Sprites")]
        static void Run()
        {
            var settings = AssetDatabase.LoadAssetAtPath<OverworldSettings>(OverworldSetupPaths.SettingsPath);
            if (!settings)
            {
                EditorUtility.DisplayDialog("Check Overworld Sprites", "No Overworld Settings asset found. Run Set Up Overworld first.", "OK");
                return;
            }
            var problems = Check(new Dictionary<string, Sprite>
            {
                { "Folder Sprite", settings.FolderSprite },
                { "Solved Sprite", settings.SolvedSprite },
                { "Desk Top Sprite", settings.DeskTopSprite },
                { "Description Paper Sprite", settings.DescriptionPaperSprite },
                { "Letter Paper Sprite", settings.LetterPaperSprite },
                { "Ring Sprite", settings.RingSprite },
                { "Shift Keycap Sprite", settings.ShiftKeycapSprite },
            });
            string message = problems.Count == 0 ? "Every required sprite is set to a real picture." : string.Join("\n", problems);
            Debug.Log("Overworld sprite check:\n" + message);
            EditorUtility.DisplayDialog("Check Overworld Sprites", message, "OK");
        }

        public static List<string> Check(Dictionary<string, Sprite> required)
        {
            var problems = new List<string>();
            foreach (var pair in required)
            {
                if (!pair.Value) problems.Add(pair.Key + ": empty");
                else if (pair.Value.name.StartsWith("placeholder_")) problems.Add(pair.Key + ": still the placeholder " + pair.Value.name);
            }
            return problems;
        }
    }
}
