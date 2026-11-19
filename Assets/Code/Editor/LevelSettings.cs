using DetectiveDave.Levels;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// Editor-only defaults for the level tools, shared by the team (saved in ProjectSettings, never shipped):
    /// where things live, the Key Art new levels start with, the Start / Goal kinds the checks look for, and the board size the keyboard is drawn at.
    /// Edit in Project Settings > Detective Dave > Levels. The game never reads this: what it plays comes from the scene.
    /// </summary>
    [FilePath("ProjectSettings/DetectiveDaveLevelSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class LevelSettings : ScriptableSingleton<LevelSettings>
    {
        // Where the migrated project keeps things. Only used to fill an empty setting the first time it is read.
        const string DefaultLevelsFolder = "Assets/Levels";
        const string DefaultLevelTemplate = "Assets/Levels/Templates/Level Template.prefab";
        const string DefaultPalette = "Assets/Tiles/Level Tiles.prefab";
        const string DefaultLayoutsFolder = "Assets/Game/Keyboard/Data";
        const string DefaultGameScene = "Assets/Scenes/Game.unity";
        const string DefaultStartType = "Assets/Tiles/Start.asset";
        const string DefaultGoalType = "Assets/Tiles/Goal.asset";

        // Object references, not paths: moving or renaming any of these keeps the setting (GUIDs).
        [Tooltip("New levels are created in this folder.")]
        [SerializeField] DefaultAsset levelsFolder;

        [Tooltip("New Level copies this prefab.")]
        [SerializeField] GameObject levelTemplate;

        [Tooltip("The Tile Palette opened for painting levels.")]
        [SerializeField] GameObject palette;

        [Tooltip("The folder with the keyboard layout files (*.json).")]
        [SerializeField] DefaultAsset layoutsFolder;

        [Tooltip("The scene Play This Level starts.")]
        [SerializeField] SceneAsset gameScene;

        [Tooltip("Key Art given to new levels. Empty: keep the template's.")]
        [SerializeField] KeyArt defaultKeyArt;

        [Tooltip("The kind of key the player starts on: the checks want exactly one on every keyboard.")]
        [SerializeField] TileType startType;

        [Tooltip("The kind of key that ends the level: the checks want exactly one on every keyboard.")]
        [SerializeField] TileType goalType;

        [Tooltip("The rectangle the keyboard is fitted into, in world units. Must match Size on the Keyboard Grid View in the game scene (15 x 4: one key is one unit).")]
        [SerializeField] Vector2 boardSize = new Vector2(15f, 4f);

        /// <summary>The folder new levels go to (path), or the default when unset.</summary>
        public string LevelsFolder => PathOf(Resolve(ref levelsFolder, DefaultLevelsFolder), DefaultLevelsFolder);

        public GameObject LevelTemplate => Resolve(ref levelTemplate, DefaultLevelTemplate);
        public GameObject Palette => Resolve(ref palette, DefaultPalette);
        public string LayoutsFolder => PathOf(Resolve(ref layoutsFolder, DefaultLayoutsFolder), DefaultLayoutsFolder);
        public SceneAsset GameScene => Resolve(ref gameScene, DefaultGameScene);
        public KeyArt DefaultKeyArt => defaultKeyArt;
        public TileType StartType => Resolve(ref startType, DefaultStartType);
        public TileType GoalType => Resolve(ref goalType, DefaultGoalType);
        public Vector2 BoardSize => boardSize;

        internal void SaveSettings() => Save(true);

        // An empty setting is filled once from where the project keeps the asset, and saved; after that the reference follows moves.
        T Resolve<T>(ref T field, string defaultPath) where T : Object
        {
            if (field) return field;
            field = AssetDatabase.LoadAssetAtPath<T>(defaultPath);
            if (field) SaveSettings();
            return field;
        }

        static string PathOf(Object asset, string fallback) => asset ? AssetDatabase.GetAssetPath(asset) : fallback;

        [SettingsProvider]
        static SettingsProvider CreateProvider()
        {
            SerializedObject serialized = null;
            return new SettingsProvider("Project/Detective Dave/Levels", SettingsScope.Project)
            {
                keywords = new[] { "level", "key art", "palette", "layout", "template", "start", "goal" },
                activateHandler = (_, __) =>
                {
                    instance.hideFlags &= ~HideFlags.NotEditable;
                    serialized = new SerializedObject(instance);
                },
                deactivateHandler = () => serialized = null,
                guiHandler = _ =>
                {
                    if (serialized == null) return;
                    serialized.Update();
                    EditorGUI.BeginChangeCheck();
                    var property = serialized.GetIterator();
                    property.NextVisible(true);                       // skip m_Script
                    while (property.NextVisible(false)) EditorGUILayout.PropertyField(property, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        instance.SaveSettings();
                        KeyboardLayouts.Clear();
                    }
                },
            };
        }
    }

    /// <summary>Per-computer choices of the level tools (EditorPrefs): not shared through version control.</summary>
    public static class LevelPreferences
    {
        const string PreviewLayoutKey = "DetectiveDave.Levels.PreviewLayout";

        /// <summary>The layout id used for key outlines, the Key brush and Play This Level. Defaults to UK (ISO).</summary>
        public static string PreviewLayoutId
        {
            get => EditorPrefs.GetString(PreviewLayoutKey, KeyboardLayouts.DefaultPreviewId);
            set => EditorPrefs.SetString(PreviewLayoutKey, value);
        }
    }
}
