using System.Collections.Generic;
using System.Linq;
using DetectiveDave.GuardPlay;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    [InitializeOnLoad]
    static class GuardSceneTool
    {
        static readonly Color StartColor = new Color(0.2f, 0.85f, 0.3f, 1f);
        static readonly Color EndColor = new Color(1f, 0.6f, 0.1f, 1f);
        static readonly Color PathColor = new Color(1f, 1f, 1f, 0.9f);
        static readonly Color ProblemColor = new Color(1f, 0.2f, 0.2f, 1f);
        static readonly Color OtherGuardColor = new Color(0.2f, 0.85f, 0.3f, 0.7f);
        const float MarkerRadius = 0.18f;

        static GuardSceneTool() => SceneView.duringSceneGui += OnSceneGui;

        static void OnSceneGui(SceneView view)
        {
            if (Application.isPlaying) return;
            var level = LevelOfOpenStage();
            var layout = KeyboardLayouts.Preview;
            if (!level || layout == null) return;
            var selected = SelectedGuard(level);
            if (Event.current.type == EventType.Repaint) Draw(level, layout, selected);
            if (selected) Interact(level, layout, selected);
        }

        static Level LevelOfOpenStage()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            return stage != null && stage.prefabContentsRoot ? stage.prefabContentsRoot.GetComponent<Level>() : null;
        }

        static GuardPatrol SelectedGuard(Level level)
        {
            var selected = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<GuardPatrol>() : null;
            return selected && selected.GetComponentInParent<Level>() == level ? selected : null;
        }

        static void Draw(Level level, KeyboardLayout layout, GuardPatrol selected)
        {
            using (new Handles.DrawingScope(level.BoardSpace.localToWorldMatrix))
            {
                foreach (var patrol in level.GetComponentsInChildren<GuardPatrol>(true))
                    if (patrol != selected) DrawMarker(layout, patrol);
                if (selected) DrawSelected(level, layout, selected);
            }
            if (selected) DrawHint(level, layout, selected);
        }

        static void DrawMarker(KeyboardLayout layout, GuardPatrol patrol)
        {
            var key = KeyOf(layout, patrol.StartKey);
            if (key == null) return;
            Handles.color = OtherGuardColor;
            Handles.DrawSolidDisc(KeyGeometry.Center(layout, key), Vector3.forward, MarkerRadius * KeyGeometry.Scale(layout));
        }

        static void DrawSelected(Level level, KeyboardLayout layout, GuardPatrol patrol)
        {
            var placement = GuardPlacements.Judge(level, patrol, layout);
            DrawRoute(layout, placement.Route);
            DrawEnd(layout, patrol.StartKey, StartColor, $"START\nwait {patrol.WaitBeforeMoveSeconds:0.0}s");
            DrawEnd(layout, patrol.EndKey, EndColor, $"END\nwait {patrol.WaitAtEndSeconds:0.0}s");
            if (placement.Problems.Contains(GuardProblemKind.NoPathBetweenStartAndEnd)) DrawBrokenLine(layout, patrol);
        }

        static void DrawEnd(KeyboardLayout layout, Key key, Color color, string text)
        {
            var def = KeyOf(layout, key);
            if (def == null) return;
            foreach (var rect in KeyGeometry.Rects(layout, def))
                Handles.DrawSolidRectangleWithOutline(rect, new Color(color.r, color.g, color.b, 0.35f), color);
            Handles.Label(KeyGeometry.Center(layout, def), text, LabelStyle(color));
        }

        static void DrawRoute(KeyboardLayout layout, IReadOnlyList<Key> route)
        {
            if (route.Count < 2) return;
            var points = route.Select(key => (Vector3)KeyGeometry.Center(layout, KeyOf(layout, key))).ToArray();
            Handles.color = PathColor;
            Handles.DrawAAPolyLine(4f, points);
            float size = 0.15f * KeyGeometry.Scale(layout);
            for (int i = 0; i + 1 < points.Length; i++)
            {
                var direction = (points[i + 1] - points[i]).normalized;
                Handles.ConeHandleCap(0, Vector3.Lerp(points[i], points[i + 1], 0.5f), Quaternion.LookRotation(direction, Vector3.forward), size, EventType.Repaint);
            }
        }

        static void DrawBrokenLine(KeyboardLayout layout, GuardPatrol patrol)
        {
            var start = KeyOf(layout, patrol.StartKey);
            var end = KeyOf(layout, patrol.EndKey);
            if (start == null || end == null) return;
            Handles.color = ProblemColor;
            Handles.DrawDottedLine(KeyGeometry.Center(layout, start), KeyGeometry.Center(layout, end), 6f);
        }

        static void DrawHint(Level level, KeyboardLayout layout, GuardPatrol patrol)
        {
            var lines = new List<string> { "Guard: click a key = START, Shift+click = END" };
            lines.AddRange(GuardLayoutChecks.Describe(level, patrol));
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10f, 10f, 520f, 200f));
            foreach (var line in lines) GUILayout.Label(line, line == lines[0] ? EditorStyles.boldLabel : WarningStyle());
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        static void Interact(Level level, KeyboardLayout layout, GuardPatrol patrol)
        {
            var current = Event.current;
            int id = GUIUtility.GetControlID(FocusType.Passive);
            if (current.type == EventType.Layout) HandleUtility.AddDefaultControl(id);
            if (current.type != EventType.MouseDown || current.button != 0 || current.alt || HandleUtility.nearestControl != id) return;
            var key = KeyUnderMouse(level, layout);
            current.Use();
            if (key == null || !key.InputKey.HasValue) return;
            bool isEnd = current.shift || current.control || current.command;
            Place(patrol, isEnd, key.InputKey.Value);
        }

        static KeyDef KeyUnderMouse(Level level, KeyboardLayout layout)
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var plane = new Plane(level.BoardSpace.forward, level.BoardSpace.position);
            if (!plane.Raycast(ray, out float distance)) return null;
            Vector3 local = level.BoardSpace.InverseTransformPoint(ray.GetPoint(distance));
            return KeyGeometry.KeyAt(layout, local);
        }

        static void Place(GuardPatrol patrol, bool isEnd, Key key)
        {
            Undo.RecordObject(patrol, isEnd ? "Place Guard End" : "Place Guard Start");
            if (isEnd) patrol.PlaceEnd(key);
            else patrol.PlaceStart(key);
            SceneView.RepaintAll();
        }

        static KeyDef KeyOf(KeyboardLayout layout, Key key) =>
            key == Key.None ? null : layout.UniqueKeys().FirstOrDefault(def => def.InputKey == key);

        static GUIStyle LabelStyle(Color color)
        {
            var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = color;
            return style;
        }

        static GUIStyle WarningStyle()
        {
            var style = new GUIStyle(EditorStyles.label) { wordWrap = true };
            style.normal.textColor = ProblemColor;
            return style;
        }
    }
}
