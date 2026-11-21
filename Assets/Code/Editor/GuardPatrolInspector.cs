using DetectiveDave.GuardPlay;
using DetectiveDave.Levels;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(GuardPatrol))]
    public sealed class GuardPatrolInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var patrol = (GuardPatrol)target;
            DrawEditButton(patrol);
            DrawKeys(patrol);
            EditorGUILayout.HelpBox("Press Edit Patrol, then click keys in the Scene view: first click = START, next clicks = END. Right-click or Esc leaves.", MessageType.Info);
            DrawDefaultInspector();
            DrawProblems(patrol);
        }

        static void DrawEditButton(GuardPatrol patrol)
        {
            bool editing = GuardSceneTool.IsEditing(patrol);
            var color = GUI.backgroundColor;
            GUI.backgroundColor = editing ? new Color(1f, 0.6f, 0.1f) : color;
            if (GUILayout.Button(editing ? "Stop Editing Patrol" : "Edit Patrol", GUILayout.Height(30f)))
            {
                if (editing) GuardSceneTool.End();
                else GuardSceneTool.Begin(patrol);
            }
            GUI.backgroundColor = color;
        }

        static void DrawKeys(GuardPatrol patrol)
        {
            var layout = KeyboardLayouts.Preview;
            if (layout == null) return;
            EditorGUILayout.LabelField("Start", GuardPlacements.LabelOf(layout, patrol.StartKey));
            EditorGUILayout.LabelField("End", GuardPlacements.LabelOf(layout, patrol.EndKey));
        }

        static void DrawProblems(GuardPatrol patrol)
        {
            var level = patrol.GetComponentInParent<Level>();
            if (!level) return;
            foreach (var message in GuardLayoutChecks.Describe(level, patrol))
                EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
    }
}
