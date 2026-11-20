using System.Collections.Generic;
using DetectiveDave.LevelPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// While playing, a level saved in Prefab Mode (Auto Save, or Ctrl+S) is put on the keyboard again: <see cref="LevelLoader.Reload"/>
    /// rebuilds only the level, never the keyboard or its key source. Saves within the same editor update are coalesced into one reload.
    /// Key Art edits need nothing here: the views listen to <c>KeyArt.Changed</c>.
    /// </summary>
    [InitializeOnLoad]
    public static class LiveLevelReload
    {
        static readonly HashSet<string> savedPaths = new HashSet<string>();

        static LiveLevelReload() => PrefabStage.prefabSaved += OnPrefabSaved;

        static void OnPrefabSaved(GameObject savedRoot)
        {
            if (!EditorApplication.isPlaying) return;
            var stage = PrefabStageUtility.GetPrefabStage(savedRoot);
            var path = stage != null ? stage.assetPath : AssetDatabase.GetAssetPath(savedRoot);
            Schedule(path);
        }

        /// <summary>Reloads, on the next editor update, every playing loader whose level is the prefab at <paramref name="assetPath"/>.</summary>
        public static void Schedule(string assetPath)
        {
            if (!EditorApplication.isPlaying || string.IsNullOrEmpty(assetPath)) return;
            if (savedPaths.Count == 0) EditorApplication.delayCall += ReloadSaved;
            savedPaths.Add(assetPath);
        }

        static void ReloadSaved()
        {
            var paths = new List<string>(savedPaths);
            savedPaths.Clear();
            if (!EditorApplication.isPlaying) return;
            foreach (var loader in Object.FindObjectsByType<LevelLoader>())
            {
                if (!loader.Source || !paths.Contains(AssetDatabase.GetAssetPath(loader.Source))) continue;
                loader.Reload();
                Debug.Log($"[Live reload] '{loader.Source.name}' reloaded.", loader);
            }
        }
    }
}
