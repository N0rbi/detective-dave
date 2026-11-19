using System.Collections.Generic;
using System.Linq;
using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// Window > Detective Dave > Levels: everything a designer needs to make and play a level.
    /// The game order (drag to reorder), New Level, the current level's Key Art and Background, the live checks with fix buttons,
    /// the keyboard used for outlines and play-testing, and Play This Level (F5).
    /// The current level is the one open in Prefab Mode, else the one selected in the list.
    /// <para>
    /// IMGUI rules kept by construction: everything that decides which controls are drawn (current level, checks, levels missing from the
    /// order) is taken once per Layout event and reused by the events that follow, so Layout and Repaint always draw the same controls;
    /// and every button that opens windows, prefab stages, dialogs or edits assets runs after the GUI pass (<see cref="Defer"/>),
    /// never inside it, so no other window's GUI can run in the middle of this one.
    /// </para>
    /// </summary>
    public sealed class LevelsWindow : EditorWindow
    {
        LevelSequence sequence;
        SerializedObject serializedSequence;
        ReorderableList list;
        Level selected;
        Vector2 scroll;
        bool dirty = true;
        bool rebind = true;

        // Taken in the Layout event, used until the next one.
        Level shown;
        List<LevelIssue> issues = new List<LevelIssue>();
        List<Level> notInOrder = new List<Level>();

        [MenuItem("Window/Detective Dave/Levels")]
        static void ShowWindow() => GetWindow<LevelsWindow>("Levels");

        void OnEnable()
        {
            Tilemap.tilemapTileChanged += OnTilesChanged;
            Undo.undoRedoPerformed += MarkDirty;
            PrefabStage.prefabStageOpened += OnStageChanged;
            PrefabStage.prefabStageClosing += OnStageChanged;
            PrefabStage.prefabSaved += OnPrefabSaved;
            EditorApplication.projectChanged += OnProjectChanged;
            OnProjectChanged();
        }

        void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnTilesChanged;
            Undo.undoRedoPerformed -= MarkDirty;
            PrefabStage.prefabStageOpened -= OnStageChanged;
            PrefabStage.prefabStageClosing -= OnStageChanged;
            PrefabStage.prefabSaved -= OnPrefabSaved;
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        void OnFocus() => MarkDirty();

        void OnTilesChanged(Tilemap tilemap, Tilemap.SyncTile[] changes) => MarkDirty();
        void OnStageChanged(PrefabStage stage) => MarkDirty();
        void OnPrefabSaved(GameObject root) => MarkDirty();

        // The sequence is looked up again in the next Layout event, like everything else the window shows.
        void OnProjectChanged()
        {
            rebind = true;
            MarkDirty();
        }

        void MarkDirty()
        {
            dirty = true;
            Repaint();
        }

        // Runs an action after the current GUI pass, then refreshes the window.
        void Defer(System.Action action)
        {
            EditorApplication.delayCall += () =>
            {
                action();
                MarkDirty();
            };
        }

        void TakeSnapshot()
        {
            if (rebind)
            {
                var found = LevelTools.FindSequence();
                if (found != sequence || serializedSequence == null) BindSequence(found);
                rebind = false;
            }
            var current = Current;
            if (!dirty && current == shown) return;
            shown = current;
            issues = shown ? LevelValidator.Validate(shown) : new List<LevelIssue>();
            notInOrder = sequence ? LevelTools.FindLevels().Where(level => sequence.IndexOf(level) < 0).ToList() : new List<Level>();
            dirty = false;
        }

        Level Current => LevelTools.OpenLevel ? LevelTools.OpenLevel : selected;

        void BindSequence(LevelSequence found)
        {
            sequence = found;
            serializedSequence = sequence ? new SerializedObject(sequence) : null;
            list = null;
            if (serializedSequence == null) return;
            list = new ReorderableList(serializedSequence, serializedSequence.FindProperty("levels"), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Game order"),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.y += 1f;
                    var label = new Rect(rect.x, rect.y, 28f, rect.height);
                    EditorGUI.LabelField(label, (index + 1).ToString());
                    EditorGUI.PropertyField(new Rect(label.xMax, rect.y, rect.width - label.width, rect.height), element, GUIContent.none);
                },
                onSelectCallback = reorderable =>
                {
                    var element = reorderable.serializedProperty.GetArrayElementAtIndex(reorderable.index);
                    selected = element.objectReferenceValue as Level;
                    MarkDirty();      // the next Layout event shows it
                },
            };
        }

        void OnGUI()
        {
            if (Event.current.type == EventType.Layout) TakeSnapshot();
            DrawToolbar();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSequence();
            EditorGUILayout.Space();
            DrawCurrent();
            EditorGUILayout.Space();
            DrawChecks();
            EditorGUILayout.EndScrollView();
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var target = sequence;
                if (GUILayout.Button("New Level", EditorStyles.toolbarButton)) Defer(() => LevelTools.CreateLevel(target));
                var level = shown;
                using (new EditorGUI.DisabledScope(!level))
                {
                    if (GUILayout.Button("Open", EditorStyles.toolbarButton)) Defer(() => LevelTools.Open(level));
                    if (GUILayout.Button(new GUIContent("Play This Level", "Plays the current level on the preview keyboard. Shortcut: F5 (Edit > Shortcuts > Detective Dave)."), EditorStyles.toolbarButton))
                        Defer(() => PlayThisLevel.Play(level));
                }
                GUILayout.FlexibleSpace();
                DrawLayoutPopup();
            }
        }

        void DrawLayoutPopup()
        {
            var layouts = KeyboardLayouts.All;
            if (layouts.Count == 0)
            {
                GUILayout.Label("No keyboard layouts", EditorStyles.miniLabel);
                return;
            }
            var preview = KeyboardLayouts.Preview;
            int index = Mathf.Max(0, layouts.ToList().IndexOf(preview));
            var names = layouts.Select(layout => new GUIContent(layout.Name)).ToArray();
            GUILayout.Label(new GUIContent("Keyboard", "Used for key outlines, the Key brush and Play This Level. Remembered on this computer. Checks cover every keyboard."), EditorStyles.miniLabel);
            int picked = EditorGUILayout.Popup(index, names, EditorStyles.toolbarPopup, GUILayout.Width(110f));
            if (picked == index) return;
            LevelPreferences.PreviewLayoutId = layouts[picked].Id;
            SceneView.RepaintAll();
            MarkDirty();
        }

        void DrawSequence()
        {
            if (list == null)
            {
                EditorGUILayout.HelpBox("There is no Level Sequence: the game does not know which levels to play.", MessageType.Warning);
                if (GUILayout.Button("Create Level Sequence")) Defer(CreateSequence);
                return;
            }
            serializedSequence.Update();
            list.DoLayoutList();
            if (serializedSequence.ApplyModifiedProperties()) AssetDatabase.SaveAssetIfDirty(sequence);
            foreach (var level in notInOrder)
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox($"'{(level ? level.name : "(deleted)")}' is not in the game order.", MessageType.Info);
                    var target = sequence;
                    if (GUILayout.Button("Add", GUILayout.Width(50f), GUILayout.Height(38f))) Defer(() => LevelTools.AddToSequence(target, level));
                }
        }

        void CreateSequence()
        {
            var folder = LevelSettings.instance.LevelsFolder;
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogError($"Levels folder '{folder}' does not exist.");
                return;
            }
            var created = CreateInstance<LevelSequence>();
            AssetDatabase.CreateAsset(created, AssetDatabase.GenerateUniqueAssetPath($"{folder}/Level Sequence.asset"));
            foreach (var level in LevelTools.FindLevels()) LevelTools.AddToSequence(created, level);
            BindSequence(created);
            Debug.Log("Created the Level Sequence. Assign it to the Level Loader in the game scene once.", created);
        }

        void DrawCurrent()
        {
            var level = shown;
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
            if (!level)
            {
                EditorGUILayout.HelpBox("Select a level in the list, or press New Level.", MessageType.None);
                return;
            }
            bool open = LevelTools.OpenLevel == level;
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField(open ? "Editing" : "Selected", LevelTools.Asset(level), typeof(Level), false);

            EditorGUI.BeginChangeCheck();
            var art = (KeyArt)EditorGUILayout.ObjectField(new GUIContent("Key Art", "How every kind of key looks. Shared by levels: duplicate it to vary one level."), level.KeyArt, typeof(KeyArt), false);
            if (EditorGUI.EndChangeCheck()) Defer(() => LevelTools.SetKeyArt(level, art));

            var current = level.BackgroundRenderer ? level.BackgroundRenderer.sprite : null;
            EditorGUI.BeginChangeCheck();
            var sprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Background", "Drawn behind the keyboard. Drag a sprite from Assets/Backgrounds."), current, typeof(Sprite), false);
            if (EditorGUI.EndChangeCheck()) Defer(() => LevelTools.SetBackground(level, sprite));
        }

        void DrawChecks()
        {
            EditorGUILayout.LabelField("Checks", EditorStyles.boldLabel);
            if (!shown) return;
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("All good.", MessageType.Info);
                return;
            }
            foreach (var issue in issues)
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(issue.Message, issue.Severity);
                    if (issue.Fix != null && GUILayout.Button(issue.FixLabel ?? "Fix", GUILayout.MaxWidth(150f), GUILayout.Height(38f)))
                        Defer(issue.Fix);
                }
        }
    }
}
