using DetectiveDave.OverworldPlay;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(CaseFile))]
    public sealed class CaseFileInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var file = (CaseFile)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Letter preview", EditorStyles.boldLabel);
            DrawLetter("Accept", file.ResolvedAcceptLetter);
            DrawLetter("Decline", file.ResolvedDeclineLetter);
            DrawProblems(file);
        }

        static void DrawLetter(string title, string letter)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(letter, MessageType.None);
        }

        static void DrawProblems(CaseFile file)
        {
            if (!DivergingTyper.Diverge(file.ResolvedAcceptLetter, file.ResolvedDeclineLetter))
                EditorGUILayout.HelpBox("The accept and decline messages must differ, and neither may be the start of the other.", MessageType.Warning);
            var unresolved = TextTemplate.UnresolvedPlaceholders(file.ResolvedDescription + file.ResolvedFindings + file.ResolvedAcceptLetter + file.ResolvedDeclineLetter);
            if (unresolved.Count > 0)
                EditorGUILayout.HelpBox("No value for " + string.Join(", ", unresolved) + ". Add a suspect with that key or fix the text.", MessageType.Warning);
            if (!file.Level)
                EditorGUILayout.HelpBox("No level to play when the case is accepted.", MessageType.Warning);
        }
    }
}
