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
            DrawMisclick(settings);
            DrawDeathHook(settings);
        }

        static void DrawDeathHook(MusicSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Death hook", EditorStyles.boldLabel);
            if (!settings.DeathHook.HookClip)
                EditorGUILayout.HelpBox("No Hook Clip: dying changes nothing in the music. Put the muted trumpet in Assets/Staging or Assets/Audio/Music/Stingers and run Window > Detective Dave > Set Up Music.", MessageType.Warning);
            else
                EditorGUILayout.HelpBox("A death (caught by a guard, bitten by a dog) fades the hook up and the music down; the hook runs silently all the time in step with the loop. When the player can move again it fades back to silence and the music up. The wrong-key stumble and the goal never play it.", MessageType.Info);
        }

        static void DrawMisclick(MusicSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Most dissonant clip per chord (wrong key)", EditorStyles.boldLabel);
            if (!settings.DissonantMisclick)
            {
                EditorGUILayout.HelpBox("Dissonant Misclick is off: a wrong key plays a normal piano note.", MessageType.Info);
                return;
            }
            var rows = MisclickTable.For(settings);
            if (rows.Count == 0)
            {
                EditorGUILayout.HelpBox("No Chord Chart with chords, so a wrong key plays a normal piano note. Run Window > Detective Dave > Set Up Chord Chart.", MessageType.Warning);
                return;
            }
            foreach (var row in rows)
            {
                var tied = row.TiedCount > 1 ? $" (+{row.TiedCount - 1} tied, they take turns)" : string.Empty;
                EditorGUILayout.LabelField(row.Chord, $"{row.Clip}  score {row.Score:0.00}{tied}");
            }
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
