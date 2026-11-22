using System.Linq;
using DetectiveDave.MusicPlay;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    [CustomEditor(typeof(MusicSettings))]
    sealed class MusicSettingsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (MusicSettings)target;
            DrawDefaultInspector();
            DrawDebounce(settings);
        }

        static void DrawDebounce(MusicSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debounce", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(DebounceSummary.For(settings.ChordDebounceSeconds), MessageType.Info);
            if (!settings.SingleNoteClips.Any(single => single != null && single.Clip))
            {
                EditorGUILayout.HelpBox("No single note clips yet, so every note is a chord. Export \"Piano Single - <Note><Octave>.wav\" files into Assets/Staging and run Window > Detective Dave > Set Up Music.", MessageType.Warning);
                return;
            }
            foreach (var single in SingleNoteOrder.Sorted(settings.SingleNoteClips))
                EditorGUILayout.LabelField($"{SinglePitch.FileNameOf(single.MidiPitch)} -> {single.NoteName}", single.Clip.name);
        }
    }
}
