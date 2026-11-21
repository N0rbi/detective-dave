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

        static readonly Color WashColor = new Color(1f, 0.55f, 0.1f, 0.12f);
        static readonly GuardEditMode Mode = new GuardEditMode();

        static GuardSceneTool()
        {
            SceneView.duringSceneGui += OnSceneGui;
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += _ => Mode.Exit();
            PrefabStage.prefabStageClosing += _ => Mode.Exit();
        }

        public static bool IsEditing(GuardPatrol patrol) => Mode.IsActive && patrol && Mode.GuardId == patrol.GetHashCode();

        public static void Begin(GuardPatrol patrol)
        {
            if (!patrol) return;
            Mode.Enter(patrol.GetHashCode(), patrol.StartKey != Key.None);
            Selection.activeGameObject = patrol.gameObject;
            SceneView.RepaintAll();
        }

        public static void End()
        {
            Mode.Exit();
            SceneView.RepaintAll();
        }

        // Selecting another guard switches to it; a child or parent of the level keeps the mode; anything else leaves it.
        static void OnSelectionChanged()
        {
            if (!Mode.IsActive) return;
            var selected = Selection.activeGameObject;
            var guard = selected ? selected.GetComponent<GuardPatrol>() : null;
            if (guard) Mode.SwitchGuard(guard.GetHashCode(), guard.StartKey != Key.None);
            else if (!IsLevelRelated(selected)) Mode.Exit();
            SceneView.RepaintAll();
        }

        static bool IsLevelRelated(GameObject selected)
        {
            var level = LevelOfOpenStage();
            if (!level || !selected) return false;
            return selected.GetComponentInParent<Level>() == level || level.transform.IsChildOf(selected.transform);
        }

        static void OnSceneGui(SceneView view)
        {
            if (Application.isPlaying) return;
            var level = LevelOfOpenStage();
            var layout = KeyboardLayouts.Preview;
            if (!level || layout == null) return;
            var selected = SelectedGuard(level);
            if (Event.current.type == EventType.Repaint) Draw(level, layout, selected);
            if (selected) DrawPanel(selected);
            if (selected && IsEditing(selected)) Interact(level, layout, selected);
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
            if (selected && IsEditing(selected)) DrawWash(selected);
            if (selected) DrawHint(level, layout, selected);
        }

        static void DrawWash(GuardPatrol patrol)
        {
            Handles.BeginGUI();
            EditorGUI.DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), WashColor);
            Handles.EndGUI();
        }

        static void DrawPanel(GuardPatrol patrol)
        {
            bool editing = IsEditing(patrol);
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10f, 150f, 230f, 130f), EditorStyles.helpBox);
            GUILayout.Label("Guard Patrol: " + patrol.name, EditorStyles.boldLabel);
            bool wantEdit = GUILayout.Toggle(editing, "Edit Patrol", EditorStyles.miniButton);
            if (wantEdit && !editing) Begin(patrol);
            if (!wantEdit && editing) End();
            if (editing)
            {
                int target = GUILayout.Toolbar(Mode.Target == PatrolTarget.Start ? 0 : 1, new[] { "Start", "End" });
                Mode.ChooseTarget(target == 0 ? PatrolTarget.Start : PatrolTarget.End);
                if (GUILayout.Button("Exit")) End();
            }
            GUILayout.EndArea();
            Handles.EndGUI();
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
            var lines = new List<string> { IsEditing(patrol)
                ? $"PATROL EDIT: {patrol.name}: click a key to set {Mode.Target.ToString().ToUpper()}  (Shift+click = END, right-click / Esc = leave)"
                : $"{patrol.name}: press Edit Patrol (panel below, or Inspector) to place its keys" };
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
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                current.Use();
                End();
                return;
            }
            if (current.type != EventType.MouseDown || current.alt || HandleUtility.nearestControl != id) return;
            if (current.button == 1)
            {
                current.Use();
                End();
                return;
            }
            if (current.button != 0) return;
            var key = KeyUnderMouse(level, layout);
            current.Use();
            if (key == null || !key.InputKey.HasValue) return;
            var placed = Mode.Click(current.shift || current.control || current.command);
            if (placed.HasValue) Place(patrol, placed.Value == PatrolTarget.End, key.InputKey.Value);
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
