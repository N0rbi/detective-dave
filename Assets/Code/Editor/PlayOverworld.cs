using DetectiveDave.KeyboardGrid;
using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    /// <summary>Plays the overworld scene with the preview keyboard layout and a chosen desk, the same way Play This Level plays a level.</summary>
    [InitializeOnLoad]
    public static class PlayOverworld
    {
        const string DeskKey = "DetectiveDave.PlayOverworld.Desk";
        const string LayoutKey = "DetectiveDave.PlayOverworld.Layout";
        const string LogPrefix = "[Play Overworld]";

        static PlayOverworld() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        [Shortcut("Detective Dave/Play Overworld", KeyCode.F5, ShortcutModifiers.Shift)]
        [MenuItem("Window/Detective Dave/Play Overworld")]
        static void PlayMenu() => Play(DeskToPlay() ?? OverworldSetupPaths.SampleDeskPath);

        /// <summary>For F5: plays the overworld when a desk is open in Prefab Mode or selected. False when there is no desk (F5 then plays a level).</summary>
        public static bool TryPlayDeskInContext()
        {
            var path = DeskToPlay();
            if (path == null) return false;
            Play(path);
            return true;
        }

        static string DeskToPlay()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage && stage.prefabContentsRoot.GetComponent<DeskLayout>()) return stage.assetPath;
            var selected = Selection.activeObject as GameObject;
            if (selected && selected.GetComponent<DeskLayout>() && PrefabUtility.IsPartOfPrefabAsset(selected)) return AssetDatabase.GetAssetPath(selected);
            return null;
        }

        static void Play(string deskPath)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log($"{LogPrefix} Already playing: stop first.");
                return;
            }
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError($"{LogPrefix} Scripts have compile errors: fix them first.");
                return;
            }
            var layout = KeyboardLayouts.Preview;
            if (layout == null)
            {
                Debug.LogError($"{LogPrefix} No keyboard layouts found.");
                return;
            }
            if (PrefabStageUtility.GetCurrentPrefabStage()) AssetDatabase.SaveAssets();
            var error = OverworldSetup.ApplyOrError();
            if (error != null)
            {
                PlayerSetup.ShowProblem("Play Overworld", error);
                return;
            }
            SessionState.SetString(DeskKey, AssetDatabase.AssetPathToGUID(deskPath));
            SessionState.SetString(LayoutKey, layout.Id);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(OverworldSetupPaths.ScenePath);
            EditorApplication.EnterPlaymode();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad()
        {
            if (string.IsNullOrEmpty(SessionState.GetString(DeskKey, string.Empty))) return;
            var layout = KeyboardLayouts.Find(SessionState.GetString(LayoutKey, string.Empty)) ?? KeyboardLayouts.Preview;
            if (layout != null) KeyboardSession.Lock(layout);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            var path = AssetDatabase.GUIDToAssetPath(SessionState.GetString(DeskKey, string.Empty));
            var deskObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var desk = deskObject ? deskObject.GetComponent<DeskLayout>() : null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var controller = root.GetComponentInChildren<OverworldController>(true);
                if (!controller) continue;
                if (desk) controller.UseDesk(desk);
                return;
            }
            Debug.LogError($"{LogPrefix} Scene '{scene.name}' has no Overworld Controller: run Window > Detective Dave > Set Up Overworld.");
        }

        static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode && change != PlayModeStateChange.ExitingPlayMode) return;
            SessionState.EraseString(DeskKey);
            SessionState.EraseString(LayoutKey);
            var start = EditorSceneManager.playModeStartScene;
            if (start && AssetDatabase.GetAssetPath(start) == OverworldSetupPaths.ScenePath) EditorSceneManager.playModeStartScene = null;
        }
    }
}
