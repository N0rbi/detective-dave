using System.Linq;
using DetectiveDave.Levels;
using DetectiveDave.OverworldPlay;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(CaseFile))]
    public sealed class CaseFileInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var file = (CaseFile)target;
            if (CaseIds.StampIfEmpty(file)) serializedObject.Update();
            DrawDefaultInspector();
            DrawLevelPicker();
            DrawIdProblems(file);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Letter preview", EditorStyles.boldLabel);
            DrawLetter("Accept", file.ResolvedAcceptLetter);
            DrawLetter("Decline", file.ResolvedDeclineLetter);
            EditorGUILayout.LabelField("After the level", EditorStyles.boldLabel);
            DrawLetter("Left paper", new PaperReveal(file.ResolvedDescription, file.ResolvedFindings).Full);
            DrawLetter("Right paper (typed by the player)", file.ResolvedResponse);
            DrawProblems(file);
        }

        void DrawLevelPicker()
        {
            var levels = LevelTools.FindLevels();
            var levelProperty = serializedObject.FindProperty("level");
            var names = new[] { "None" }.Concat(levels.Select(level => level.name)).ToArray();
            int current = levels.IndexOf(levelProperty.objectReferenceValue as Level) + 1;
            int picked = EditorGUILayout.Popup("Pick a level", current, names);
            if (picked == current) return;
            levelProperty.objectReferenceValue = picked == 0 ? null : levels[picked - 1];
            serializedObject.ApplyModifiedProperties();
        }

        static void DrawIdProblems(CaseFile file)
        {
            var others = CaseIds.OtherPathsSharing(file);
            if (others.Count == 0) return;
            EditorGUILayout.HelpBox("Another case already uses the id " + file.Id + ": " + string.Join(", ", others) + ". Give one of them another id before players have progress.", MessageType.Warning);
        }

        static void DrawLetter(string title, string letter)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(letter, MessageType.None);
        }

        static void DrawProblems(CaseFile file)
        {
            if (DivergingTyper.OneStartsWithTheOther(file.ResolvedAcceptMessage, file.ResolvedDeclineMessage))
                EditorGUILayout.HelpBox("One answer is the start of the other. The player chooses the shorter one by pressing Enter or Space after it, and the longer one by typing on.", MessageType.Warning);
            else if (!DivergingTyper.Diverge(file.ResolvedAcceptLetter, file.ResolvedDeclineLetter))
                EditorGUILayout.HelpBox("The accept and decline messages must differ.", MessageType.Warning);
            var unresolved = TextTemplate.UnresolvedPlaceholders(file.ResolvedDescription + file.ResolvedFindings + file.ResolvedResponse + file.ResolvedAcceptLetter + file.ResolvedDeclineLetter);
            if (unresolved.Count > 0)
                EditorGUILayout.HelpBox("No value for " + string.Join(", ", unresolved) + ". Add a suspect with that key or fix the text.", MessageType.Warning);
            if (string.IsNullOrWhiteSpace(file.ResolvedFindings))
                EditorGUILayout.HelpBox("The findings are empty: nothing appears under the description after the level.", MessageType.Warning);
            if (string.IsNullOrWhiteSpace(file.ResolvedResponse))
                EditorGUILayout.HelpBox("The response is empty: the player has nothing to type to solve the case.", MessageType.Warning);
            if (!file.Level)
                EditorGUILayout.HelpBox("No level to play when the case is accepted.", MessageType.Warning);
        }
    }
}
