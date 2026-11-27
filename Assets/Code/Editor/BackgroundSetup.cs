using System.Collections.Generic;
using System.Linq;
using DetectiveDave.Levels;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class BackgroundSetup
    {
        const string DialogTitle = "Set Up Background";

        [MenuItem("Window/Detective Dave/Set Up Background")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the background. Nothing was changed.");
                return;
            }
            var error = ApplyOrError(out var assigned);
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            EditorUtility.DisplayDialog(DialogTitle, Summary(assigned), "OK");
        }

        public static string ApplyOrError(out List<string> assigned)
        {
            assigned = new List<string>();
            if (!AssetImporter.GetAtPath(BackgroundSetupPaths.SkylinePath)) return "Nothing was changed. Missing asset:\n" + BackgroundSetupPaths.SkylinePath;
            var skyline = PlayerSpriteImport.ImportDecoration(BackgroundSetupPaths.SkylinePath);
            foreach (var art in AssetDatabase.FindAssets("t:KeyArt").Select(guid => AssetDatabase.LoadAssetAtPath<KeyArt>(AssetDatabase.GUIDToAssetPath(guid))).Where(found => found))
            {
                var serialized = new SerializedObject(art);
                var property = serialized.FindProperty("background");
                if (property.objectReferenceValue) continue;
                property.objectReferenceValue = skyline;
                serialized.ApplyModifiedProperties();
                assigned.Add(art.name);
            }
            AssetDatabase.SaveAssets();
            return null;
        }

        static string Summary(List<string> assigned) =>
            assigned.Count == 0
                ? "Nothing needed changing. Every Key Art already has a background."
                : "The skyline is now the default background of: " + string.Join(", ", assigned) + ".\nMove it per level: open the level, then Background Placement on the Level component.";
    }
}
