using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(DeskLayout))]
    public sealed class DeskLayoutInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var desk = (DeskLayout)target;
            DrawKeyboardPicker();
            DrawSummary(desk);
            DrawProblems(desk);
        }

        static void DrawKeyboardPicker()
        {
            var layouts = KeyboardLayouts.All;
            if (layouts.Count == 0) return;
            var names = layouts.Select(layout => layout.Name).ToArray();
            int current = System.Math.Max(0, layouts.ToList().IndexOf(KeyboardLayouts.Preview));
            int picked = EditorGUILayout.Popup("Preview keyboard", current, names);
            if (picked == current) return;
            LevelPreferences.PreviewLayoutId = layouts[picked].Id;
            SceneView.RepaintAll();
        }

        static void DrawSummary(DeskLayout desk)
        {
            var markers = DeskDesignInputs.AllMarkers(desk);
            int withoutCase = markers.Count(marker => !marker.CanBeMapped);
            EditorGUILayout.LabelField($"{markers.Length} case files on the desk, {withoutCase} without a case.");
        }

        static void DrawProblems(DeskLayout desk)
        {
            var reports = DeskLayoutChecks.OnEveryLayout(DeskDesignInputs.MappedMarkers(desk), DeskDesignInputs.EveryLayout(), DeskDesignInputs.StartKey);
            if (reports.Count == 0)
            {
                EditorGUILayout.HelpBox("No problems on any keyboard.", MessageType.Info);
                return;
            }
            foreach (var report in reports)
                EditorGUILayout.HelpBox(report.LayoutName + ":\n" + string.Join("\n", report.Issues.Select(DeskLayoutChecks.Describe)), MessageType.Warning);
        }
    }
}
