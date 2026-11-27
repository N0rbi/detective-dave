using System.Linq;
using DetectiveDave.CheatPlay;
using DetectiveDave.KeyboardInput;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    static class CheatSetup
    {
        const string DialogTitle = "Set Up Cheats";
        const string SavedSceneFirst = "{0} is open with unsaved changes and the cheats are not set up in it yet. Save it (Ctrl+S) and try again.";

        [MenuItem("Window/Detective Dave/Set Up Cheats")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the cheats. Nothing was changed.");
                return;
            }
            AssetDatabase.Refresh();
            var error = ApplyOrError();
            if (error != null)
            {
                PlayerSetup.ShowProblem(DialogTitle, error);
                return;
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(CheatSetupPaths.SettingsPath);
            EditorUtility.DisplayDialog(DialogTitle, "Cheats are on. Type resetplayer in the game or the overworld to forget all solved cases and go back to the overworld.", "OK");
        }

        public static string ApplyOrError()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(CheatSetupPaths.MessageFontPath);
            if (!font) return "Nothing was changed. Missing asset:\n" + CheatSetupPaths.MessageFontPath;
            var settings = EnsureSettings(font);
            AssetDatabase.SaveAssets();
            return WireScene(PlayerSetupPaths.ScenePath, settings) ?? WireScene(OverworldSetupPaths.ScenePath, settings);
        }

        static CheatSettings EnsureSettings(Font font)
        {
            var settings = AssetDatabase.LoadAssetAtPath<CheatSettings>(CheatSetupPaths.SettingsPath);
            if (!settings)
            {
                EnsureFolder(CheatSetupPaths.CheatFolder);
                settings = ScriptableObject.CreateInstance<CheatSettings>();
                AssetDatabase.CreateAsset(settings, CheatSetupPaths.SettingsPath);
            }
            var serialized = new SerializedObject(settings);
            var fontProperty = serialized.FindProperty("messageFont");
            if (!fontProperty.objectReferenceValue) fontProperty.objectReferenceValue = font;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'), System.IO.Path.GetFileName(path));
        }

        static string WireScene(string scenePath, CheatSettings settings)
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)) return null;
            var scene = EditorSceneManager.GetSceneByPath(scenePath);
            var wasLoaded = scene.isLoaded;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                return Wire(scene, scenePath, settings);
            }
            catch (System.Exception exception)
            {
                return $"Setting up the cheats in {scenePath} failed: {exception.Message}";
            }
            finally
            {
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        static string Wire(Scene scene, string scenePath, CheatSettings settings)
        {
            var host = FindHost(scene);
            if (!host) return null;
            var link = host.GetComponent<CheatLink>();
            if (link && IsLinked(link, settings)) return null;
            if (scene.isDirty) return string.Format(SavedSceneFirst, scenePath);
            if (!link) link = host.AddComponent<CheatLink>();
            var serialized = new SerializedObject(link);
            serialized.FindProperty("settings").objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
            return null;
        }

        static GameObject FindHost(Scene scene) =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<KeyboardKeySource>(true)).Select(source => source.gameObject).FirstOrDefault();

        static bool IsLinked(CheatLink link, CheatSettings settings) => new SerializedObject(link).FindProperty("settings").objectReferenceValue == settings;
    }
}
