using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    static class PlayerSceneWiring
    {
        const string UnsetReference = @"^\s*(playerPrefab|settings|gap|wallGutter): \{fileID: 0\}";
        const string SavedSceneFirst = "Game.unity is open with unsaved changes and the player is not wired into it yet. Save Game.unity (Ctrl+S) and try again. Nothing was changed.";

        public static string Wire(PlayerView prefab, PlayerMovementSettings settings, TileType[] tiles)
        {
            var scene = EditorSceneManager.GetSceneByPath(PlayerSetupPaths.ScenePath);
            var wasLoaded = scene.isLoaded;
            if (!wasLoaded && LooksWired()) return null;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(PlayerSetupPaths.ScenePath, OpenSceneMode.Additive);
            try
            {
                return WireScene(scene, prefab, settings, tiles);
            }
            catch (System.Exception exception)
            {
                return "Wiring Game.unity failed: " + exception.Message;
            }
            finally
            {
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        static bool LooksWired()
        {
            var text = File.ReadAllText(PlayerSetupPaths.ScenePath);
            var scripts = new[] { typeof(PlayerController), typeof(LevelCellClassifier), typeof(LevelMessageView) };
            return scripts.All(script => text.Contains(ScriptGuid(script))) && !Regex.IsMatch(text, UnsetReference, RegexOptions.Multiline);
        }

        static string ScriptGuid(System.Type script) =>
            AssetDatabase.FindAssets("t:MonoScript " + script.Name)
                .FirstOrDefault(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid)).GetClass() == script) ?? "missing";

        static string WireScene(Scene scene, PlayerView prefab, PlayerMovementSettings settings, TileType[] tiles)
        {
            var loader = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<LevelLoader>(true)).FirstOrDefault(found => found);
            if (!loader) return "Game.unity has no LevelLoader object, so the player could not be added to the scene.";
            if (!loader.GetComponent<KeyboardGridView>()) return "The object with LevelLoader in Game.unity has no KeyboardGridView.";
            var wasDirty = scene.isDirty;
            if (wasDirty) return IsWired(loader, prefab, settings, tiles) ? null : SavedSceneFirst;
            EnsureComponent<LevelMessageView>(loader.gameObject);
            var classifier = EnsureComponent<LevelCellClassifier>(loader.gameObject);
            SetReferences(classifier, ClassifierReferences(tiles));
            var controller = EnsureComponent<PlayerController>(loader.gameObject);
            SetReferences(controller, ControllerReferences(prefab, settings));
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
            return null;
        }

        static bool IsWired(LevelLoader loader, PlayerView prefab, PlayerMovementSettings settings, TileType[] tiles)
        {
            var classifier = loader.GetComponent<LevelCellClassifier>();
            var controller = loader.GetComponent<PlayerController>();
            return loader.GetComponent<LevelMessageView>() && classifier && controller
                && HasReferences(classifier, ClassifierReferences(tiles)) && HasReferences(controller, ControllerReferences(prefab, settings));
        }

        static (string field, Object value)[] ClassifierReferences(TileType[] tiles) =>
            new (string, Object)[] { ("gap", tiles[0]), ("wallGutter", tiles[1]) };

        static (string field, Object value)[] ControllerReferences(PlayerView prefab, PlayerMovementSettings settings) =>
            new (string, Object)[] { ("playerPrefab", prefab), ("settings", settings) };

        static bool HasReferences(Object target, (string field, Object value)[] references)
        {
            var serialized = new SerializedObject(target);
            return references.All(reference => serialized.FindProperty(reference.field).objectReferenceValue == reference.value);
        }

        static T EnsureComponent<T>(GameObject target) where T : Component
        {
            var existing = target.GetComponent<T>();
            return existing ? existing : target.AddComponent<T>();
        }

        static void SetReferences(Object target, (string field, Object value)[] references)
        {
            var serialized = new SerializedObject(target);
            foreach (var (field, value) in references) serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }
    }
}
