using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DetectiveDave.GuardPlay;
using DetectiveDave.LevelPlay;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    static class GuardSceneWiring
    {
        const string SavedSceneFirst = "Game.unity is open with unsaved changes and the guards are not wired into it yet. Save Game.unity (Ctrl+S) and try again.";

        public static string Wire(GuardSettings settings)
        {
            var scene = EditorSceneManager.GetSceneByPath(PlayerSetupPaths.ScenePath);
            var wasLoaded = scene.isLoaded;
            if (!wasLoaded && LooksWired()) return null;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(PlayerSetupPaths.ScenePath, OpenSceneMode.Additive);
            try
            {
                return WireScene(scene, settings);
            }
            catch (System.Exception exception)
            {
                return "Wiring the guards into Game.unity failed: " + exception.Message;
            }
            finally
            {
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        static bool LooksWired()
        {
            var text = File.ReadAllText(PlayerSetupPaths.ScenePath);
            return text.Contains(ScriptGuid(typeof(GuardController))) && !Regex.IsMatch(text, @"^\s*settings: \{fileID: 0\}", RegexOptions.Multiline);
        }

        static string ScriptGuid(System.Type script) =>
            AssetDatabase.FindAssets("t:MonoScript " + script.Name)
                .FirstOrDefault(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid)).GetClass() == script) ?? "missing";

        static string WireScene(Scene scene, GuardSettings settings)
        {
            var loader = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<LevelLoader>(true)).FirstOrDefault(found => found);
            if (!loader) return "Game.unity has no LevelLoader object, so the guards could not be added to the scene.";
            if (!loader.GetComponent<PlayerController>()) return "The object with LevelLoader in Game.unity has no PlayerController. Run Set Up Player first.";
            var existing = loader.GetComponent<GuardController>();
            if (existing && IsWired(existing, settings)) return null;
            if (scene.isDirty) return SavedSceneFirst;
            var controller = existing ? existing : loader.gameObject.AddComponent<GuardController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("settings").objectReferenceValue = settings;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.SaveScene(scene);
            return null;
        }

        static bool IsWired(GuardController controller, GuardSettings settings) =>
            new SerializedObject(controller).FindProperty("settings").objectReferenceValue == settings;
    }
}
