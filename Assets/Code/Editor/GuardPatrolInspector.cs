using DetectiveDave.GuardPlay;
using UnityEditor;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(GuardPatrol))]
    public sealed class GuardPatrolInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var patrol = (GuardPatrol)target;
            if (patrol.StartKey == patrol.EndKey)
                EditorGUILayout.HelpBox("Start and end are the same key, so this guard will stand still.", MessageType.Info);
            if (IsShift(patrol.StartKey) || IsShift(patrol.EndKey))
                EditorGUILayout.HelpBox("Guards cannot stand on the Shift keys the player starts from. Pick letter keys.", MessageType.Warning);
        }

        static bool IsShift(Key key) => key == Key.LeftShift || key == Key.RightShift;
    }
}
