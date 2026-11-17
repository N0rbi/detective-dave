using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// The Level Definition inspector: checks on top, then the level's name, background (with preview), key art per tile type
    /// (with a sprite strip of every standard key shape, template fallbacks included) and the baked shape (with a map).
    /// </summary>
    [CustomEditor(typeof(LevelDefinition))]
    internal sealed class LevelDefinitionEditor : UnityEditor.Editor
    {
        const float ThumbnailSize = 40f;
        const float BackgroundPreviewHeight = 72f;

        SerializedProperty displayName;
        SerializedProperty background;
        SerializedProperty backgroundTint;
        SerializedProperty styleTemplate;
        SerializedProperty tileStyles;
        bool usedAsTemplate;

        // Validating and checking the source group are not free, so they run only when something changed, in the Layout pass.
        List<LevelIssue> issues;
        List<TileMapGroup> sourceGroups;
        bool sourceUpToDate;
        bool dirty = true;

        void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnTilesChanged;
            Undo.undoRedoPerformed -= MarkDirty;
            EditorApplication.hierarchyChanged -= MarkDirty;
            EditorApplication.projectChanged -= MarkDirty;
        }

        void OnTilesChanged(Tilemap tilemap, Tilemap.SyncTile[] tiles) => MarkDirty();

        void MarkDirty()
        {
            dirty = true;
            Repaint();
        }

        void Recompute(LevelDefinition level)
        {
            issues = LevelValidator.Validate(level, usedAsTemplate);
            sourceGroups = LevelBaker.FindOpenGroups().Where(group => group.Level == level).ToList();
            sourceUpToDate = sourceGroups.Count != 1 || LevelBaker.IsUpToDate(sourceGroups[0]);
            dirty = false;
        }

        void OnEnable()
        {
            Tilemap.tilemapTileChanged += OnTilesChanged;
            Undo.undoRedoPerformed += MarkDirty;
            EditorApplication.hierarchyChanged += MarkDirty;
            EditorApplication.projectChanged += MarkDirty;
            usedAsTemplate = LevelValidator.FindTemplates().Contains((LevelDefinition)target);
            displayName = serializedObject.FindProperty("displayName");
            background = serializedObject.FindProperty("background");
            backgroundTint = serializedObject.FindProperty("backgroundTint");
            styleTemplate = serializedObject.FindProperty("styleTemplate");
            tileStyles = serializedObject.FindProperty("tileStyles");
        }

        public override void OnInspectorGUI()
        {
            var level = (LevelDefinition)target;
            if (issues == null || (dirty && Event.current.type == EventType.Layout)) Recompute(level);
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            LevelEditorGUI.Header("Checks");
            LevelEditorGUI.DrawIssues(issues);

            LevelEditorGUI.Header("Level");
            EditorGUILayout.PropertyField(displayName);

            DrawBackground(level);
            DrawKeyArt(level);
            DrawShape(level);

            if (EditorGUI.EndChangeCheck()) dirty = true;
            serializedObject.ApplyModifiedProperties();
        }

        void DrawBackground(LevelDefinition level)
        {
            LevelEditorGUI.Header("Background");
            EditorGUILayout.PropertyField(background);
            EditorGUILayout.PropertyField(backgroundTint);
            var shown = level.Background;
            if (!shown) return;
            if (!background.objectReferenceValue)
                EditorGUILayout.LabelField("Inherited from the style template.", EditorStyles.miniLabel);
            var rect = GUILayoutUtility.GetRect(BackgroundPreviewHeight * 2f, BackgroundPreviewHeight, GUILayout.ExpandWidth(true));
            LevelEditorGUI.DrawSprite(rect, shown, level.BackgroundTint);
        }

        void DrawKeyArt(LevelDefinition level)
        {
            LevelEditorGUI.Header("Key Art");
            EditorGUILayout.PropertyField(styleTemplate);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Add Styles For All Tile Types", "Adds a style, with the standard key shapes, for every Tile Type asset in the project that has none here.")))
                    AddStylesForAllTypes(level);
                if (GUILayout.Button(new GUIContent("Add Standard Key Shapes", "Adds the key shapes every shipped keyboard layout uses to each style that lacks them.")))
                    for (int i = 0; i < tileStyles.arraySize; i++) AddStandardShapes(tileStyles.GetArrayElementAtIndex(i).FindPropertyRelative("shapes"));
            }

            for (int i = 0; i < tileStyles.arraySize; i++)
            {
                if (DrawStyle(level, i)) break;   // the array changed under us: draw again next frame
            }

            if (GUILayout.Button("Add Style"))
            {
                var element = AppendElement(tileStyles);
                ResetStyle(element, null);
            }
        }

        // Returns true when the style was removed.
        bool DrawStyle(LevelDefinition level, int index)
        {
            var element = tileStyles.GetArrayElementAtIndex(index);
            var type = element.FindPropertyRelative("type").objectReferenceValue as TileType;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, type ? type.name : "(no tile type)", true, EditorStyles.foldoutHeader);
                DrawSpriteStrip(level, type);
                if (!element.isExpanded) return false;

                EditorGUILayout.PropertyField(element.FindPropertyRelative("type"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("tint"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("shapes"), true);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("Fill From Folder…", "Pick a folder of sprites named like 'wall_1_25x1' or 'wall_enter_1_5x2' ('_' is the decimal point). Each sprite goes to the shape its name gives.")))
                        FillFromFolder(element.FindPropertyRelative("shapes"), type);
                    if (GUILayout.Button("Remove Style"))
                    {
                        tileStyles.DeleteArrayElementAtIndex(index);
                        return true;
                    }
                }
            }
            return false;
        }

        // Every standard shape as this level draws it (own sprite, or the template's), red when missing.
        static void DrawSpriteStrip(LevelDefinition level, TileType type)
        {
            if (!type) return;
            var tint = level.FindStyle(type)?.Tint ?? Color.white;
            var shapes = KeyShapePresets.Standard;
            var row = GUILayoutUtility.GetRect(ThumbnailSize * shapes.Count, ThumbnailSize, GUILayout.ExpandWidth(false));
            float width = Mathf.Min(ThumbnailSize, (EditorGUIUtility.currentViewWidth - 40f) / shapes.Count);
            for (int i = 0; i < shapes.Count; i++)
            {
                var cell = new Rect(row.x + i * width, row.y, width - 2f, row.height - 2f);
                var sprite = level.FindSprite(type, shapes[i].Size, shapes[i].WholeKey);
                LevelEditorGUI.DrawSprite(cell, sprite, tint);
                GUI.Label(cell, new GUIContent(string.Empty, shapes[i].Label + (sprite ? string.Empty : " (missing)")));
            }
        }

        void DrawShape(LevelDefinition level)
        {
            LevelEditorGUI.Header("Shape");
            var terrain = level.Terrain;
            if (terrain.IsEmpty)
                EditorGUILayout.LabelField("Not baked yet.", EditorStyles.miniLabel);
            else
            {
                var keys = Vector2.Scale(terrain.CellBounds.size, terrain.CellSize);
                EditorGUILayout.LabelField($"{keys.x:0.##} x {keys.y:0.##} keys, baked from {level.BakedFrom}", EditorStyles.wordWrappedMiniLabel);
                LevelEditorGUI.DrawTerrainPreview(terrain);
            }

            var groups = sourceGroups;
            if (groups.Count == 0)
            {
                EditorGUILayout.HelpBox("To change the shape, open the scene or prefab with this level's Tile Map Group, paint, and press Bake there.", MessageType.None);
                return;
            }
            if (groups.Count > 1)
            {
                EditorGUILayout.HelpBox($"{groups.Count} open Tile Map Groups bake into this level. Give each its own level.", MessageType.Error);
                return;
            }
            var source = groups[0];
            if (!sourceUpToDate)
                EditorGUILayout.HelpBox("Out of date: the group's tilemaps changed since the last bake.", MessageType.Warning);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Bake Now"))
                {
                    LevelBaker.BakeInto(source);
                    AssetDatabase.SaveAssetIfDirty(level);
                    dirty = true;
                }
                if (GUILayout.Button("Select Group")) Selection.activeObject = source.gameObject;
            }
        }

        void AddStylesForAllTypes(LevelDefinition level)
        {
            var styled = level.TileStyles.Where(style => style != null && style.Type).Select(style => style.Type).ToList();
            foreach (var type in LevelAssets.FindAllTileTypes().Where(type => !styled.Contains(type)))
                ResetStyle(AppendElement(tileStyles), type);
        }

        internal static void ResetStyle(SerializedProperty element, TileType type)
        {
            element.FindPropertyRelative("type").objectReferenceValue = type;
            element.FindPropertyRelative("tint").colorValue = Color.white;
            var shapes = element.FindPropertyRelative("shapes");
            shapes.arraySize = 0;
            AddStandardShapes(shapes);
            element.isExpanded = true;
        }

        static void AddStandardShapes(SerializedProperty shapes)
        {
            foreach (var shape in KeyShapePresets.Standard)
                if (FindShape(shapes, shape.Size, shape.WholeKey) == null)
                    SetShape(AppendElement(shapes), shape.Label, shape.Size, shape.WholeKey, null);
        }

        static void FillFromFolder(SerializedProperty shapes, TileType type)
        {
            var absolute = EditorUtility.OpenFolderPanel($"Sprites for {(type ? type.name : "this style")}", "Assets", string.Empty);
            if (string.IsNullOrEmpty(absolute)) return;
            var folder = FileUtil.GetProjectRelativePath(absolute.Replace('\\', '/'));
            if (string.IsNullOrEmpty(folder))
            {
                EditorUtility.DisplayDialog("Fill From Folder", "Pick a folder inside this project's Assets folder.", "OK");
                return;
            }

            int assigned = 0;
            foreach (var path in AssetDatabase.FindAssets("t:Sprite", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).Distinct())
                foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                {
                    if (!KeyShapePresets.TryParseSpriteName(sprite.name, out var size, out var wholeKey)) continue;
                    var entry = FindShape(shapes, size, wholeKey) ?? AppendElement(shapes);
                    SetShape(entry, KeyShapePresets.LabelFor(size, wholeKey), size, wholeKey, sprite);
                    assigned++;
                }
            if (assigned == 0)
                EditorUtility.DisplayDialog("Fill From Folder", $"No sprite in {folder} is named like 'name_1_25x1' or 'name_enter_1_5x2'.", "OK");
        }

        static SerializedProperty FindShape(SerializedProperty shapes, Vector2 size, bool wholeKey)
        {
            for (int i = 0; i < shapes.arraySize; i++)
            {
                var entry = shapes.GetArrayElementAtIndex(i);
                var probe = new KeyShapeSprite(null, entry.FindPropertyRelative("size").vector2Value, entry.FindPropertyRelative("wholeKey").boolValue, null);
                if (probe.Matches(size, wholeKey)) return entry;
            }
            return null;
        }

        static void SetShape(SerializedProperty entry, string label, Vector2 size, bool wholeKey, Sprite sprite)
        {
            entry.FindPropertyRelative("label").stringValue = label;
            entry.FindPropertyRelative("size").vector2Value = size;
            entry.FindPropertyRelative("wholeKey").boolValue = wholeKey;
            entry.FindPropertyRelative("sprite").objectReferenceValue = sprite;
        }

        internal static SerializedProperty AppendElement(SerializedProperty array)
        {
            array.arraySize++;
            return array.GetArrayElementAtIndex(array.arraySize - 1);
        }
    }
}
