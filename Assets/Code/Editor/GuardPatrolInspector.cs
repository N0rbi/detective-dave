using DetectiveDave.GuardPlay;
using DetectiveDave.Levels;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(GuardPatrol))]
    public sealed class GuardPatrolInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var patrol = (GuardPatrol)target;
            DrawKeys(patrol);
            EditorGUILayout.HelpBox("Scene view: click a key = START. Shift+click a key = END.", MessageType.Info);
            DrawDefaultInspector();
            DrawProblems(patrol);
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
