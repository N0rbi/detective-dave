using DetectiveDave.KeyboardGrid;
using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// Play This Level: starts the game scene on one level and the preview keyboard layout, with no layout picker and no scene edit.
    /// <para>
    /// How the level gets there, with public APIs only and no runtime code that knows about the editor:
    /// the launcher saves the open level, puts the level's GUID and the layout id in <see cref="SessionState"/> (survives the domain reload),
    /// sets <see cref="EditorSceneManager.playModeStartScene"/> to the game scene and enters Play Mode.
    /// Before the first scene loads, <see cref="BeforeSceneLoad"/> (this editor assembly) locks the layout with the game's own
    /// <see cref="KeyboardSession.Lock"/>, so the keyboard builds with it in Awake. When the scene has loaded (after Awake, before Start)
    /// it hands the level to the scene's <see cref="LevelLoader.Load"/>, so the loader skips its sequence.
    /// Leaving Play Mode clears the start scene and the session values, so a normal Play is untouched.
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    public static class PlayThisLevel
    {
        const string LevelKey = "DetectiveDave.PlayThisLevel.Level";
        const string LayoutKey = "DetectiveDave.PlayThisLevel.Layout";
        const string LogPrefix = "[Play This Level]";

        static PlayThisLevel() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        /// <summary>Default F5, rebindable in Edit > Shortcuts. Plays the level open in Prefab Mode, else the selected level, else the first of the sequence.</summary>
        [Shortcut("Detective Dave/Play This Level", KeyCode.F5)]
        static void PlayShortcut() => Play(LevelTools.LevelToPlay());

        /// <summary>Starts the game scene on <paramref name="level"/> (a level prefab asset, or the root of an open level in Prefab Mode).</summary>
        public static void Play(Level level)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log($"{LogPrefix} Already playing: stop first.");
                return;
            }
            var path = LevelTools.AssetPath(level);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"{LogPrefix} No level to play: open or select a level in Assets/Levels.");
                return;
            }
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError($"{LogPrefix} Scripts have compile errors: fix them first.");
                return;
            }
            var scene = LevelSettings.instance.GameScene;
            if (!scene)
            {
                Debug.LogError($"{LogPrefix} No game scene set (Project Settings > Detective Dave > Levels > Game Scene).");
                return;
            }
            var layout = KeyboardLayouts.Preview;
            if (layout == null)
            {
                Debug.LogError($"{LogPrefix} No keyboard layouts found in '{LevelSettings.instance.LayoutsFolder}'.");
                return;
            }
            LevelTools.SaveOpenLevel();
            SessionState.SetString(LevelKey, AssetDatabase.AssetPathToGUID(path));
            SessionState.SetString(LayoutKey, layout.Id);
            EditorSceneManager.playModeStartScene = scene;
            EditorApplication.EnterPlaymode();
        }

        // Runs in Play Mode before the first scene's Awake. Does nothing on a normal Play (no session value).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad()
        {
            var guid = SessionState.GetString(LevelKey, string.Empty);
            if (string.IsNullOrEmpty(guid)) return;
            var layout = KeyboardLayouts.Find(SessionState.GetString(LayoutKey, string.Empty)) ?? KeyboardLayouts.Preview;
            bool locked = layout != null && KeyboardSession.Lock(layout);
            Debug.Log($"{LogPrefix} Before scene load: layout '{layout?.Id}' {(locked ? "locked" : "NOT locked")}, level {AssetDatabase.GUIDToAssetPath(guid)}.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // After the scene's Awake / OnEnable, before any Start: the loader has not picked its first level yet.
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            var guid = SessionState.GetString(LevelKey, string.Empty);
            var level = AssetDatabase.LoadAssetAtPath<Level>(AssetDatabase.GUIDToAssetPath(guid));
            var loader = FindLoader(scene);
            if (!loader)
            {
                Debug.LogError($"{LogPrefix} Scene '{scene.name}' has no Level Loader: the level cannot be played.");
                return;
            }
            Debug.Log($"{LogPrefix} Scene '{scene.name}' loaded: handing '{(level ? level.name : "nothing")}' to the Level Loader.", loader);
            loader.Load(level);
        }

        static LevelLoader FindLoader(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var loader = root.GetComponentInChildren<LevelLoader>(true);
                if (loader) return loader;
            }
            return null;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode && change != PlayModeStateChange.ExitingPlayMode) return;
            Clear();
        }

        // On every return to Edit Mode, not only after a Play This Level, so nothing set here can leak into the next normal Play.
        static void Clear()
        {
            SessionState.EraseString(LevelKey);
            SessionState.EraseString(LayoutKey);
            if (EditorSceneManager.playModeStartScene == LevelSettings.instance.GameScene) EditorSceneManager.playModeStartScene = null;
        }
    }
}
