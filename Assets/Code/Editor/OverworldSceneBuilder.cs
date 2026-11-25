using System.Linq;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// Builds Assets/Scenes/Overworld.unity from Game.unity with code, no hand-edited scene file.
    /// A plain copy is made at editor load so the build list always has the scene; Set Up Overworld then swaps the level pieces for the controller.
    /// </summary>
    [InitializeOnLoad]
    static class OverworldSceneBuilder
    {
        static readonly string[] GamePieces =
        {
            "PlayerController", "KeyboardBoardSampler", "KeyboardGridDebugOverlay", "KeyTerrainView",
            "LevelBackgroundView", "LevelCellClassifier", "LevelMessageView", "LevelLoader", "KeyboardGridInput",
        };

        static OverworldSceneBuilder() => EditorApplication.delayCall += EnsureStubScene;

        static void EnsureStubScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(OverworldSetupPaths.ScenePath)) return;
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(PlayerSetupPaths.ScenePath)) return;
            AssetDatabase.CopyAsset(PlayerSetupPaths.ScenePath, OverworldSetupPaths.ScenePath);
        }

        public static string WireOrError(PlayerView playerPrefab, PlayerMovementSettings magnifier, OverworldSettings settings, SmokingSprites smoking)
        {
            EnsureStubScene();
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(OverworldSetupPaths.ScenePath)) return "Assets/Scenes/Game.unity is missing, so the overworld scene could not be created.";
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return "Cancelled: the open scenes were not saved. Nothing was changed.";
            var scene = EditorSceneManager.OpenScene(OverworldSetupPaths.ScenePath, OpenSceneMode.Additive);
            try
            {
                var board = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<KeyboardBoard>(true)).FirstOrDefault(found => found);
                if (!board) return "The overworld scene has no keyboard board object.";
                Wire(board.gameObject, playerPrefab, magnifier, settings);
                SmokingDetectiveWiring.Ensure(scene, smoking);
                EditorSceneManager.SaveScene(scene);
                return null;
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        static void Wire(GameObject host, PlayerView playerPrefab, PlayerMovementSettings magnifier, OverworldSettings settings)
        {
            foreach (var name in GamePieces)
                foreach (var piece in host.GetComponents<Component>().Where(component => component && component.GetType().Name == name))
                    Object.DestroyImmediate(piece);
            var controller = host.GetComponent<OverworldController>();
            if (!controller) controller = host.AddComponent<OverworldController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            serialized.FindProperty("playerSettings").objectReferenceValue = magnifier;
            serialized.FindProperty("overworldSettings").objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var view = new SerializedObject(host.GetComponent<KeyboardGridView>());
            view.FindProperty("drawKeys").boolValue = false;
            view.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
