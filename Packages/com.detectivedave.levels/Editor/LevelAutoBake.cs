using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// Keeps baked level shapes in step with the tilemaps without anyone pressing Bake: before entering Play, when a scene is saved
    /// and when a prefab is saved, every Tile Map Group involved is baked into its level. Only levels whose shape actually changed
    /// are written. Off in batch mode. Toggle with Tools > Detective Dave > Levels > Auto-Bake.
    /// Also holds the other Tools menu items.
    /// </summary>
    [InitializeOnLoad]
    internal static class LevelAutoBake
    {
        const string ToolsMenu = "Tools/Detective Dave/Levels/";
        const string AutoBakeMenu = ToolsMenu + "Auto-Bake Before Play And On Save";
        const string AutoBakePref = "DetectiveDave.Levels.AutoBake";

        static LevelAutoBake()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            PrefabStage.prefabSaving += OnPrefabSaving;
        }

        static bool Enabled
        {
            get => EditorPrefs.GetBool(AutoBakePref, true) && !Application.isBatchMode;
            set => EditorPrefs.SetBool(AutoBakePref, value);
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode || !Enabled) return;
            int changed = LevelBaker.BakeOpenScenes(recordUndo: false);
            if (changed > 0) Debug.Log($"Auto-baked {changed} level shape(s) before Play.");
        }

        static void OnSceneSaving(Scene scene, string path)
        {
            if (!Enabled || !scene.isLoaded) return;
            BakeAll(group => group.gameObject.scene == scene);
        }

        static void OnPrefabSaving(GameObject root)
        {
            if (!Enabled || !root) return;
            BakeAll(group => group.transform.IsChildOf(root.transform));
        }

        // Shared targets are checked across every open group, so a group elsewhere that bakes into the same level is not silently overwritten.
        static void BakeAll(System.Func<TileMapGroup, bool> include)
        {
            foreach (var group in LevelBaker.GroupsSafeToBake(LevelBaker.FindOpenGroups()).Where(include))
                if (LevelBaker.BakeInto(group, recordUndo: false))
                    AssetDatabase.SaveAssetIfDirty(group.Level);
        }

        [MenuItem(AutoBakeMenu, false, 100)]
        static void ToggleAutoBake() => Enabled = !EditorPrefs.GetBool(AutoBakePref, true);

        [MenuItem(AutoBakeMenu, true)]
        static bool ToggleAutoBakeCheck()
        {
            Menu.SetChecked(AutoBakeMenu, EditorPrefs.GetBool(AutoBakePref, true));
            return true;
        }

        [MenuItem(ToolsMenu + "Bake All Groups In Open Scenes", false, 0)]
        static void BakeOpenScenes()
        {
            int changed = LevelBaker.BakeOpenScenes();
            Debug.Log(changed > 0 ? $"Baked {changed} level shape(s)." : "Every level shape in the open scenes is up to date.");
        }

        [MenuItem(ToolsMenu + "Validate All Levels", false, 1)]
        static void ValidateAllLevels()
        {
            int problems = 0;
            var templates = LevelValidator.FindTemplates();
            foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(LevelDefinition)))
            {
                var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var issue in LevelValidator.Validate(level, templates.Contains(level)).Where(issue => issue.Severity != MessageType.Info))
                {
                    problems++;
                    var text = $"Level '{level.name}': {issue.Message}";
                    if (issue.Severity == MessageType.Error) Debug.LogError(text, level);
                    else Debug.LogWarning(text, level);
                }
            }
            var openGroups = LevelBaker.FindOpenGroups();
            foreach (var group in openGroups)
                foreach (var issue in LevelValidator.Validate(group, openGroups).Where(issue => issue.Severity != MessageType.Info))
                {
                    problems++;
                    var text = $"Tile Map Group '{LevelBaker.DescribeSource(group)}': {issue.Message}";
                    if (issue.Severity == MessageType.Error) Debug.LogError(text, group);
                    else Debug.LogWarning(text, group);
                }
            Debug.Log(problems == 0 ? "All levels passed their checks." : $"Level checks found {problems} problem(s); click a message to select the asset.");
        }
    }
}
