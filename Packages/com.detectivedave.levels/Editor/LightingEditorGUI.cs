using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>The Lighting section of the Level inspector: brightness, colour, flicker settings, the patterns with their timelines, and the preview.</summary>
    internal static class LightingEditorGUI
    {
        const float LampHeight = 34f;
        const float SnapTimeStep = 0.05f;
        const float SnapBrightnessStep = 0.1f;
        static readonly Color LampBackground = new Color(0.08f, 0.08f, 0.1f);
        static bool snapToGrid = true;

        public static void Draw(SerializedProperty lighting, LevelLightingSettings settings)
        {
            LevelEditorGUI.Header("Lighting");
            EditorGUILayout.PropertyField(lighting.FindPropertyRelative("intensity"));
            EditorGUILayout.PropertyField(lighting.FindPropertyRelative("colorTint"));
            DrawLamp(settings);
            var flicker = lighting.FindPropertyRelative("flicker");
            EditorGUILayout.PropertyField(flicker);
            if (!flicker.boolValue) return;
            EditorGUILayout.PropertyField(lighting.FindPropertyRelative("secondsBetweenFlickersMin"), new GUIContent("Wait At Least (s)"));
            EditorGUILayout.PropertyField(lighting.FindPropertyRelative("secondsBetweenFlickersMax"), new GUIContent("Wait At Most (s)"));
            DrawPatterns(lighting.FindPropertyRelative("patterns"), settings);
        }

        static void DrawLamp(LevelLightingSettings settings)
        {
            var rect = EditorGUILayout.GetControlRect(false, LampHeight);
            var label = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
            EditorGUI.LabelField(label, new GUIContent("Final Look", "The light with this level's brightness and colour. It dims while a flicker is previewed."));
            var lamp = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, rect.height);
            EditorGUI.DrawRect(lamp, LampBackground);
            float brightness = settings.Intensity * (LightingPreview.IsPlaying ? LightingPreview.Multiplier : 1f);
            var color = settings.ColorTint * Mathf.Clamp01(brightness);
            color.a = 1f;
            EditorGUI.DrawRect(new Rect(lamp.x + 4f, lamp.y + 4f, lamp.width - 8f, lamp.height - 8f), color);
        }

        static void DrawPatterns(SerializedProperty patterns, LevelLightingSettings settings)
        {
            LevelEditorGUI.Header("Flicker Patterns");
            using (new EditorGUILayout.HorizontalScope())
            {
                snapToGrid = EditorGUILayout.ToggleLeft(new GUIContent("Snap To Grid", "Points jump to the grid lines while you drag them."), snapToGrid, GUILayout.Width(110f));
                GUILayout.FlexibleSpace();
                DrawPlayLikeTheGame(settings);
            }
            int removeIndex = -1;
            for (int index = 0; index < patterns.arraySize; index++)
                if (DrawPattern(patterns.GetArrayElementAtIndex(index), settings, index)) removeIndex = index;
            if (removeIndex >= 0) patterns.DeleteArrayElementAtIndex(removeIndex);
            DrawAddPattern(patterns);
        }

        static void DrawPlayLikeTheGame(LevelLightingSettings settings)
        {
            bool playingAll = LightingPreview.IsPlaying && !LightingPreview.IsSinglePattern;
            var label = new GUIContent(playingAll ? "Stop Preview" : "Preview Like In Game", "Plays the flicker as the game does: random waits, random patterns.");
            if (!GUILayout.Button(label, GUILayout.Width(140f))) return;
            if (playingAll) LightingPreview.Stop();
            else LightingPreview.PlayLikeTheGame(settings);
        }

        static bool DrawPattern(SerializedProperty pattern, LevelLightingSettings settings, int index)
        {
            bool remove = false;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(pattern.FindPropertyRelative("name"), GUIContent.none);
                    DrawPreviewButton(settings, index);
                    if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60f))) remove = true;
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(pattern.FindPropertyRelative("duration"), new GUIContent("Duration (s)"));
                    EditorGUILayout.PropertyField(pattern.FindPropertyRelative("weight"), new GUIContent("Weight", "How likely this pattern is picked compared with the others."));
                }
                var snap = new TimelineSnap(snapToGrid, SnapTimeStep, SnapBrightnessStep);
                bool previewing = LightingPreview.IsPlayingPattern(index);
                FlickerTimeline.Draw(pattern, snap, previewing ? LightingPreview.Elapsed : -1f);
                EditorGUILayout.LabelField("Left = start, right = end. Top = full light, bottom = dark. Click to add a point, drag to move, right-click to remove.", EditorStyles.miniLabel);
                DrawPresetMenu(pattern);
            }
            return remove;
        }

        static void DrawPreviewButton(LevelLightingSettings settings, int index)
        {
            bool previewing = LightingPreview.IsPlayingPattern(index);
            if (!GUILayout.Button(previewing ? "Stop" : "Preview", EditorStyles.miniButton, GUILayout.Width(60f))) return;
            if (previewing) LightingPreview.Stop();
            else LightingPreview.PlayPattern(settings, index);
        }

        static void AppendPreset(SerializedObject owner, string path, FlickerPattern preset)
        {
            owner.Update();
            FlickerPresets.Append(owner.FindProperty(path), preset);
            owner.ApplyModifiedProperties();
        }

        static void DrawPresetMenu(SerializedProperty pattern)
        {
            var points = pattern.FindPropertyRelative("points");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Start From", EditorStyles.miniLabel, GUILayout.Width(60f));
                foreach (var preset in FlickerPresets.All)
                    if (GUILayout.Button(preset.Name, EditorStyles.miniButton))
                    {
                        FlickerPresets.ReplacePoints(points, preset);
                        pattern.FindPropertyRelative("duration").floatValue = preset.Duration;
                    }
            }
        }

        static void DrawAddPattern(SerializedProperty patterns)
        {
            var owner = patterns.serializedObject;
            var path = patterns.propertyPath;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (!GUILayout.Button("Add Pattern", GUILayout.Width(110f))) return;
                var menu = new GenericMenu();
                foreach (var preset in FlickerPresets.All)
                {
                    var chosen = preset;
                    menu.AddItem(new GUIContent(preset.Name), false, () => AppendPreset(owner, path, chosen));
                }
                menu.ShowAsContext();
            }
        }
    }
}
