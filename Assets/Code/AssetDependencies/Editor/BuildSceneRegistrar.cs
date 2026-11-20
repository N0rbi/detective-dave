using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// Before Play Mode starts, makes sure every <see cref="GameScene"/> is in the build list (and enabled).
    /// A scene that does not exist in the project at all cancels Play Mode with an error.
    /// </summary>
    [InitializeOnLoad]
    static class BuildSceneRegistrar
    {
        static BuildSceneRegistrar() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode) EnsureScenesInBuild();
        }

        static void EnsureScenesInBuild()
        {
            var buildScenes = EditorBuildSettings.scenes.ToList();
            var changed = false;

            foreach (var scene in Enum.GetValues(typeof(GameScene)).Cast<GameScene>())
            {
                var path = FindScenePath(scene.ToString());
                if (path == null)
                {
                    Debug.LogError($"GameScene.{scene} has no scene file named '{scene}.unity' in the project.");
                    EditorApplication.isPlaying = false;
                    return;
                }

                var existing = buildScenes.FirstOrDefault(entry => entry.path == path);
                if (existing == null)
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                    changed = true;
                    Debug.Log($"Added '{path}' to the build list.");
                }
                else if (!existing.enabled)
                {
                    existing.enabled = true;
                    changed = true;
                    Debug.Log($"Enabled '{path}' in the build list.");
                }
            }

            if (changed) EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        static string FindScenePath(string sceneName) =>
            AssetDatabase.FindAssets($"{sceneName} t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == sceneName);
    }
}
