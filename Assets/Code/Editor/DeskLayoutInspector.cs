using System.Collections.Generic;
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
            DrawPreviewModePicker();
            DrawSummary(desk);
            DrawCases(desk);
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

        static void DrawPreviewModePicker()
        {
            int current = (int)DeskPreview.Mode;
            int picked = EditorGUILayout.Popup("Preview as", current, DeskPreview.ModeNames);
            if (picked == current) return;
            DeskPreview.Mode = (DeskPreviewMode)picked;
            SceneView.RepaintAll();
        }

        static void DrawSummary(DeskLayout desk)
        {
            var markers = DeskDesignInputs.AllMarkers(desk);
            int withoutCase = markers.Count(marker => !marker.CanBeMapped);
            EditorGUILayout.LabelField($"{markers.Length} case files on the desk, {withoutCase} without a case.");
        }

        static void DrawCases(DeskLayout desk)
        {
            EditorGUILayout.LabelField("Cases", EditorStyles.boldLabel);
            bool editable = !EditorUtility.IsPersistent(desk);
            if (!editable) EditorGUILayout.HelpBox("Open the desk (double-click it in the Project window) to add cases to it.", MessageType.Info);
            var placed = new HashSet<CaseFile>(DeskDesignInputs.AllMarkers(desk).Where(marker => marker.CaseFile).Select(marker => marker.CaseFile));
            var files = AssetDatabase.FindAssets("t:CaseFile").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<CaseFile>).Where(file => file).ToArray();
            if (files.Length == 0) EditorGUILayout.HelpBox("No cases yet. Make one with Assets > Create > Detective Dave > Case.", MessageType.None);
            foreach (var file in files) DrawCaseRow(desk, file, placed.Contains(file), editable);
            using (new EditorGUI.DisabledScope(!editable))
                if (GUILayout.Button("Add empty marker")) EditorApplication.delayCall += () => OverworldDeskBuilder.AddToDesk(desk, null);
        }

        static void DrawCaseRow(DeskLayout desk, CaseFile file, bool onDesk, bool editable)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(14f, 14f, GUILayout.Width(14f)), DeskCaseColors.Outline(file.Id));
                string client = string.IsNullOrWhiteSpace(file.ClientName) ? string.Empty : " (" + file.ClientName + ")";
                EditorGUILayout.LabelField(file.name + client + (onDesk ? " - on the desk" : " - not on the desk"));
                using (new EditorGUI.DisabledScope(onDesk || !editable))
                    if (GUILayout.Button("Add to desk", GUILayout.Width(90f))) EditorApplication.delayCall += () => OverworldDeskBuilder.AddToDesk(desk, file);
            }
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
