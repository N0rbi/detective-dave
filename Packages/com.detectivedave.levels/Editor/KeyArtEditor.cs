using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// The Key Art inspector: checks on top, the default background (with preview), then one box per kind of key with its two
    /// sprites (the 9-sliced Box and the Enter), a strip of every standard key shape as it will look in game, and the optional shape overrides.
    /// </summary>
    [CustomEditor(typeof(KeyArt))]
    internal sealed class KeyArtEditor : UnityEditor.Editor
    {
        const float ThumbnailSize = 40f;
        const float BackgroundPreviewHeight = 72f;

        SerializedProperty background;
        SerializedProperty backgroundTint;
        SerializedProperty tileStyles;
        SerializedProperty borderThickness;
        readonly Dictionary<int, int> borderPixels = new Dictionary<int, int>();
        LevelInspectorCache cache;
        List<LevelIssue> issues;

        void OnEnable()
        {
            background = serializedObject.FindProperty("background");
            backgroundTint = serializedObject.FindProperty("backgroundTint");
            tileStyles = serializedObject.FindProperty("tileStyles");
            borderThickness = serializedObject.FindProperty("borderThicknessInKeyUnits");
            cache = new LevelInspectorCache(Repaint);
        }

        void OnDisable() => cache?.Dispose();

        public override void OnInspectorGUI()
        {
            var keyArt = (KeyArt)target;
            if (issues == null || (cache.Dirty && Event.current.type == EventType.Layout))
            {
                issues = LevelValidator.Validate(keyArt);
                cache.MarkClean();
            }
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            LevelEditorGUI.Header("Checks");
            if (LevelEditorGUI.DrawIssues(issues)) cache.MarkDirty();

            LevelEditorGUI.Header("Default Background");
            EditorGUILayout.PropertyField(background);
            EditorGUILayout.PropertyField(backgroundTint);
            if (keyArt.DefaultBackground)
            {
                var rect = GUILayoutUtility.GetRect(BackgroundPreviewHeight * 2f, BackgroundPreviewHeight, GUILayout.ExpandWidth(true));
                LevelEditorGUI.DrawSprite(rect, keyArt.DefaultBackground, keyArt.DefaultBackgroundTint);
            }

            DrawStyles(keyArt);

            if (EditorGUI.EndChangeCheck()) cache.MarkDirty();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawStyles(KeyArt keyArt)
        {
            LevelEditorGUI.Header("Keys");
            EditorGUILayout.PropertyField(borderThickness);

            if (GUILayout.Button(new GUIContent("Add Styles For All Tile Types", "Adds a style for every Tile Type asset in the project that has none here.")))
                AddStylesForAllTypes(keyArt);

            for (int i = 0; i < tileStyles.arraySize; i++)
            {
                if (DrawStyle(keyArt, i)) break;   // the array changed under us: draw again next frame
            }

            if (GUILayout.Button("Add Style"))
            {
                var element = AppendElement(tileStyles);
                ResetStyle(element, null);
            }
        }

        // Returns true when the style was removed.
        bool DrawStyle(KeyArt keyArt, int index)
        {
            var element = tileStyles.GetArrayElementAtIndex(index);
            var type = element.FindPropertyRelative("type").objectReferenceValue as TileType;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, type ? type.name : "(no tile type)", true, EditorStyles.foldoutHeader);
                DrawSpriteStrip(keyArt, type);
                if (!element.isExpanded) return false;

                EditorGUILayout.PropertyField(element.FindPropertyRelative("type"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("tint"));
                var box = element.FindPropertyRelative("box");
                EditorGUILayout.PropertyField(box);
                DrawBoxHelper(box, index);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("enter"));
                DrawMigrationHint(keyArt, type);
                DrawShapeOverrides(element, type);

                if (GUILayout.Button("Remove Style"))
                {
                    tileStyles.DeleteArrayElementAtIndex(index);
                    return true;
                }
            }
            return false;
        }

        void DrawBoxHelper(SerializedProperty box, int index)
        {
            var sprite = box.objectReferenceValue as Sprite;
            if (!sprite) return;
            var problem = BoxSpriteCheck.ProblemOf(sprite);
            if (problem == BoxSpriteProblem.None)
            {
                EditorGUILayout.HelpBox("The Box is 9-sliced. Its border is " + sprite.border.x + " px.", MessageType.None);
                return;
            }
            EditorGUILayout.HelpBox($"The Box '{sprite.name}': {BoxSpriteCheck.Describe(problem)}. It is drawn stretched until this is fixed.", MessageType.Warning);
            if (!BoxSpriteCheck.CanSetBorder(sprite, out var reason))
            {
                EditorGUILayout.LabelField(reason, EditorStyles.wordWrappedMiniLabel);
                return;
            }
            if (!borderPixels.TryGetValue(index, out var pixels) || pixels <= 0)
                pixels = BoxSpriteCheck.SuggestedBorderPixels((int)sprite.rect.width, (int)sprite.rect.height);
            using (new EditorGUILayout.HorizontalScope())
            {
                pixels = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("Border (pixels)", "The same on all four sides of the sprite file."), pixels));
                borderPixels[index] = pixels;
                if (GUILayout.Button(new GUIContent("Set 9-Slice Border", "Sets the border on the sprite's import settings and Mesh Type to Full Rect."), GUILayout.Width(140f)))
                {
                    BoxSpriteCheck.TrySetBorder(sprite, pixels, out var message);
                    Debug.Log($"Key Art: {message}", sprite);
                    cache.MarkDirty();
                    GUIUtility.ExitGUI();
                }
            }
        }

        static void DrawMigrationHint(KeyArt keyArt, TileType type)
        {
            var style = type ? keyArt.FindStyle(type) : null;
            if (style != null && style.CanMigrateToBox)
                EditorGUILayout.HelpBox("This style still draws keys from one sprite per size (Shape Overrides). It works as before. To use the two-sprite design, set a Box and an Enter above.", MessageType.Info);
        }

        void DrawShapeOverrides(SerializedProperty element, TileType type)
        {
            var shapes = element.FindPropertyRelative("shapes");
            EditorGUILayout.PropertyField(shapes, new GUIContent("Shape Overrides (optional)", "A picture for one exact key size, used instead of the Box for that size. Leave empty when the Box is enough."), true);
            if (!shapes.isExpanded) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Add Standard Key Shapes", "Adds the key shapes every shipped keyboard layout uses, without a sprite.")))
                    AddStandardShapes(shapes);
                if (GUILayout.Button(new GUIContent("Fill From Folder…", "Pick a folder of sprites named like 'wall_1_25x1' or 'wall_enter_1_5x2' ('_' is the decimal point). Each sprite goes to the shape its name gives.")))
                {
                    FillFromFolder(shapes, type);
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();      // a folder dialog ran in the middle of this GUI pass: end the pass here
                }
            }
        }

        // Every standard shape as it is drawn in game, red when missing.
        static void DrawSpriteStrip(KeyArt keyArt, TileType type)
        {
            if (!type) return;
            var tint = keyArt.FindStyle(type)?.Tint ?? Color.white;
            var shapes = KeyShapePresets.Standard;
            var row = GUILayoutUtility.GetRect(ThumbnailSize * shapes.Count, ThumbnailSize, GUILayout.ExpandWidth(false));
            float width = Mathf.Min(ThumbnailSize, (EditorGUIUtility.currentViewWidth - 40f) / shapes.Count);
            for (int i = 0; i < shapes.Count; i++)
            {
                var cell = new Rect(row.x + i * width, row.y, width - 2f, row.height - 2f);
                var choice = keyArt.Resolve(type, shapes[i].Size, shapes[i].WholeKey);
                LevelEditorGUI.DrawSprite(cell, choice.Sprite, tint);
                GUI.Label(cell, new GUIContent(string.Empty, shapes[i].Label + Describe(choice)));
            }
        }

        static string Describe(KeySpriteChoice choice) =>
            choice.Kind == KeySpriteKind.None ? " (missing)" : choice.Kind == KeySpriteKind.Sliced ? " (Box, 9-sliced)" : string.Empty;

        void AddStylesForAllTypes(KeyArt keyArt)
        {
            var styled = keyArt.TileStyles.Where(style => style != null && style.Type).Select(style => style.Type).ToList();
            foreach (var type in LevelAssets.FindAllTileTypes().Where(type => !styled.Contains(type)))
                ResetStyle(AppendElement(tileStyles), type);
        }

        internal static void ResetStyle(SerializedProperty element, TileType type)
        {
            element.FindPropertyRelative("type").objectReferenceValue = type;
            element.FindPropertyRelative("tint").colorValue = Color.white;
            element.FindPropertyRelative("box").objectReferenceValue = null;
            element.FindPropertyRelative("enter").objectReferenceValue = null;
            element.FindPropertyRelative("shapes").arraySize = 0;
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
