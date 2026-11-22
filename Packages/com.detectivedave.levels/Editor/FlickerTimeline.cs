using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// The timeline widget of one flicker pattern. Time runs left to right, brightness bottom (dark) to top (full).
    /// Click empty space to add a point, drag a point to move it, right-click a point (or select it and press Delete) to remove it.
    /// </summary>
    internal static class FlickerTimeline
    {
        public const float Height = 120f;
        const float PointRadius = 5f;
        const float GrabRadius = 10f;
        static readonly Color Background = new Color(0.14f, 0.14f, 0.14f);
        static readonly Color GridLine = new Color(1f, 1f, 1f, 0.07f);
        static readonly Color CurveColor = new Color(1f, 0.85f, 0.35f);
        static readonly Color PointColor = new Color(1f, 0.95f, 0.7f);
        static readonly Color SelectedColor = new Color(0.35f, 0.75f, 1f);
        static readonly Color PlayheadColor = new Color(0.45f, 1f, 0.55f);

        sealed class State
        {
            public int Selected = -1;
            public int Dragging = -1;
        }

        public static void Draw(SerializedProperty pattern, TimelineSnap snap, float playheadSeconds)
        {
            var rect = GUILayoutUtility.GetRect(10f, Height, GUILayout.ExpandWidth(true));
            var duration = pattern.FindPropertyRelative("duration");
            var points = pattern.FindPropertyRelative("points");
            var mapper = new TimelineMapper(rect, duration.floatValue);
            int id = GUIUtility.GetControlID(FocusType.Passive);
            var state = (State)GUIUtility.GetStateObject(typeof(State), id);
            var screen = ScreenPoints(points, mapper);

            switch (Event.current.GetTypeForControl(id))
            {
                case EventType.Repaint:
                    Paint(rect, mapper, duration.floatValue, snap, screen, state.Selected, playheadSeconds);
                    break;
                case EventType.MouseDown:
                    if (rect.Contains(Event.current.mousePosition)) OnMouseDown(id, state, points, mapper, snap, screen);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id && state.Dragging >= 0) MovePoint(points, state.Dragging, mapper, snap);
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        state.Dragging = -1;
                        Event.current.Use();
                    }
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.Delete && state.Selected >= 0 && state.Selected < points.arraySize && GUIUtility.keyboardControl == id)
                    {
                        points.DeleteArrayElementAtIndex(state.Selected);
                        state.Selected = -1;
                        Event.current.Use();
                    }
                    break;
            }
        }

        static void OnMouseDown(int id, State state, SerializedProperty points, TimelineMapper mapper, TimelineSnap snap, Vector2[] screen)
        {
            var mouse = Event.current.mousePosition;
            int hit = TimelineMapper.NearestPoint(screen, mouse, GrabRadius);
            GUIUtility.keyboardControl = id;
            if (Event.current.button == 1)
            {
                if (hit >= 0) points.DeleteArrayElementAtIndex(hit);
                state.Selected = -1;
                Event.current.Use();
                return;
            }
            if (Event.current.button != 0) return;
            if (hit < 0)
            {
                hit = points.arraySize;
                points.arraySize = hit + 1;
                SetPoint(points.GetArrayElementAtIndex(hit), mapper.XToTime(mouse.x), mapper.YToBrightness(mouse.y), snap);
            }
            state.Selected = hit;
            state.Dragging = hit;
            GUIUtility.hotControl = id;
            Event.current.Use();
        }

        static void MovePoint(SerializedProperty points, int index, TimelineMapper mapper, TimelineSnap snap)
        {
            if (index >= points.arraySize) return;
            var mouse = Event.current.mousePosition;
            SetPoint(points.GetArrayElementAtIndex(index), mapper.XToTime(mouse.x), mapper.YToBrightness(mouse.y), snap);
            GUI.changed = true;
            Event.current.Use();
        }

        static void SetPoint(SerializedProperty point, float time, float brightness, TimelineSnap snap)
        {
            point.FindPropertyRelative("Time").floatValue = Mathf.Max(0f, TimelineMapper.Snap(time, snap.TimeStep, snap.Enabled));
            point.FindPropertyRelative("Brightness").floatValue = Mathf.Clamp01(TimelineMapper.Snap(brightness, snap.BrightnessStep, snap.Enabled));
        }

        static Vector2[] ScreenPoints(SerializedProperty points, TimelineMapper mapper)
        {
            var screen = new Vector2[points.arraySize];
            for (int index = 0; index < screen.Length; index++)
            {
                var point = points.GetArrayElementAtIndex(index);
                screen[index] = mapper.ToScreen(new FlickerPoint(point.FindPropertyRelative("Time").floatValue, point.FindPropertyRelative("Brightness").floatValue));
            }
            return screen;
        }

        static void Paint(Rect rect, TimelineMapper mapper, float duration, TimelineSnap snap, Vector2[] screen, int selected, float playheadSeconds)
        {
            EditorGUI.DrawRect(rect, Background);
            DrawGrid(rect, mapper, duration, snap);
            DrawCurve(rect, screen);
            for (int index = 0; index < screen.Length; index++)
                EditorGUI.DrawRect(new Rect(screen[index].x - PointRadius, screen[index].y - PointRadius, PointRadius * 2f, PointRadius * 2f), index == selected ? SelectedColor : PointColor);
            if (playheadSeconds >= 0f)
            {
                float x = mapper.TimeToX(playheadSeconds);
                EditorGUI.DrawRect(new Rect(x - 1f, rect.yMin, 2f, rect.height), PlayheadColor);
            }
        }

        static void DrawGrid(Rect rect, TimelineMapper mapper, float duration, TimelineSnap snap)
        {
            float timeStep = snap.TimeStep > 0f ? snap.TimeStep : 0.1f;
            int lines = Mathf.Min(200, Mathf.FloorToInt(duration / timeStep));
            for (int line = 1; line <= lines; line++)
                EditorGUI.DrawRect(new Rect(mapper.TimeToX(line * timeStep), rect.yMin, 1f, rect.height), GridLine);
            foreach (float level in new[] { 0.25f, 0.5f, 0.75f })
                EditorGUI.DrawRect(new Rect(rect.xMin, mapper.BrightnessToY(level), rect.width, 1f), GridLine);
        }

        static void DrawCurve(Rect rect, Vector2[] screen)
        {
            if (screen.Length == 0) return;
            var ordered = screen.OrderBy(point => point.x).ToList();
            var line = new List<Vector3> { new Vector3(rect.xMin, ordered[0].y) };
            line.AddRange(ordered.Select(point => (Vector3)point));
            line.Add(new Vector3(rect.xMax, ordered[ordered.Count - 1].y));
            Handles.color = CurveColor;
            Handles.DrawAAPolyLine(2f, line.ToArray());
        }
    }

    internal readonly struct TimelineSnap
    {
        public bool Enabled { get; }
        public float TimeStep { get; }
        public float BrightnessStep { get; }

        public TimelineSnap(bool enabled, float timeStep, float brightnessStep)
        {
            Enabled = enabled;
            TimeStep = timeStep;
            BrightnessStep = brightnessStep;
        }
    }
}
