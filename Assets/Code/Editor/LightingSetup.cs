using System.Linq;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.LevelPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    static class LightingSetup
    {
        const string DialogTitle = "Set Up Lighting";
        const string LitSpriteShader = "Universal Render Pipeline/2D/Sprite-Lit-Default";
        const string KeyMaterialPath = "Assets/Game/Keyboard/Art/KeyboardCellLit.mat";
        const string SavedSceneFirst = "Game.unity is open with unsaved changes and the lighting is not set up in it yet. Save Game.unity (Ctrl+S) and try again.";

        [MenuItem("Window/Detective Dave/Set Up Lighting")]
        static void SetUpFromWindowMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PlayerSetup.ShowProblem(DialogTitle, "Exit Play Mode before setting up the lighting. Nothing was changed.");
                return;
            }
            var error = ApplyOrError();
            if (error != null) PlayerSetup.ShowProblem(DialogTitle, error);
            else EditorUtility.DisplayDialog(DialogTitle, "The keys of Game.unity now react to the 2D lights, and the level lighting is on. Open a level and find Lighting in its inspector.", "OK");
        }

        public static string ApplyOrError()
        {
            var material = EnsureKeyMaterial(out var materialError);
            if (!material) return materialError;
            var scene = EditorSceneManager.GetSceneByPath(PlayerSetupPaths.ScenePath);
            var wasLoaded = scene.isLoaded;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(PlayerSetupPaths.ScenePath, OpenSceneMode.Additive);
            try
            {
                return WireScene(scene, material);
            }
            catch (System.Exception exception)
            {
                return "Setting up the lighting in Game.unity failed: " + exception.Message;
            }
            finally
            {
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        static Material EnsureKeyMaterial(out string error)
        {
            error = null;
            var existing = AssetDatabase.LoadAssetAtPath<Material>(KeyMaterialPath);
            if (existing) return existing;
            var shader = Shader.Find(LitSpriteShader);
            if (!shader)
            {
                error = $"The shader '{LitSpriteShader}' was not found, so the key material could not be created. Check that the project uses the Universal Render Pipeline with the 2D Renderer.";
                return null;
            }
            AssetDatabase.CreateAsset(new Material(shader) { name = "KeyboardCellLit" }, KeyMaterialPath);
            return AssetDatabase.LoadAssetAtPath<Material>(KeyMaterialPath);
        }

        static string WireScene(Scene scene, Material material)
        {
            var grids = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<KeyboardGridView>(true)).ToArray();
            var loader = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<LevelLoader>(true)).FirstOrDefault(found => found);
            if (grids.Length == 0 || !loader) return "Game.unity has no keyboard with a LevelLoader, so the lighting could not be set up.";
            if (IsDone(grids, loader, material)) return null;
            if (scene.isDirty) return SavedSceneFirst;
            foreach (var grid in grids) SetMaterial(grid, material);
            if (!loader.GetComponent<LevelLightingView>()) loader.gameObject.AddComponent<LevelLightingView>();
            EditorSceneManager.SaveScene(scene);
            return null;
        }

        static bool IsDone(KeyboardGridView[] grids, LevelLoader loader, Material material) =>
            loader.GetComponent<LevelLightingView>() && grids.All(grid => new SerializedObject(grid).FindProperty("cellMaterial").objectReferenceValue == material);

        static void SetMaterial(KeyboardGridView grid, Material material)
        {
            var serialized = new SerializedObject(grid);
            serialized.FindProperty("cellMaterial").objectReferenceValue = material;
            serialized.ApplyModifiedProperties();
        }
    }
}
