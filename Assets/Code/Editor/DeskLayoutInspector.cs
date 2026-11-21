using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(DeskLayout))]
    public sealed class DeskLayoutInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var desk = (DeskLayout)target;
            DrawRootWarning(desk);
            DrawKeyboardPicker();
            DrawSummary(desk);
            DrawProblems(desk);
            DrawChainProblems(desk);
        }

        static void DrawRootWarning(DeskLayout desk)
        {
            if (!DeskRoot.NeedsReset(desk)) return;
            EditorGUILayout.HelpBox(DeskRootCheck.Warning, MessageType.Warning);
            if (GUILayout.Button("Reset root transform")) DeskRoot.Reset(desk);
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

        static void DrawChainProblems(DeskLayout desk)
        {
            var issues = CaseChainChecks.Run(CaseChainInputs.NodesOn(desk));
            if (issues.Count == 0) return;
            EditorGUILayout.HelpBox("Case chain:\n" + string.Join("\n", issues.Select(CaseChainChecks.Describe)), MessageType.Warning);
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
