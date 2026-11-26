using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Bass Chart", fileName = "Bass Chart.config.asset")]
    public sealed class BassChart : ScriptableObject
    {
        [SerializeField] [Tooltip("Beats of the loop (0 is the very start of the loop) where the upright bass starts a note. Filled by Window > Detective Dave > Set Up Bass Chart from bass_onsets.json.")] float[] onsetBeats = new float[0];
        [SerializeField] [Tooltip("Bass Start Offset Beats. The exported bass file is assumed to start exactly at the start of the loop (bar 1). If it starts later or earlier, enter by how many beats its start is ahead of the loop start, then run Set Up Bass Chart again.")] float bassStartOffsetBeats;
        [SerializeField, Min(0.01f)] [Tooltip("Note starts are rounded to this many beats when the chart is filled. 0.25 is a sixteenth note.")] float quantizeBeats = 0.25f;
        [SerializeField] [Tooltip("The audio file the notes were found in. Filled automatically.")] string sourceFile;
        [SerializeField] [Tooltip("Length of that file in seconds. Filled automatically.")] float sourceDurationSeconds;
        [SerializeField] [Tooltip("Which tempo turned seconds into beats. Filled automatically.")] string tempoNote;

        public IReadOnlyList<float> OnsetBeats => onsetBeats;

        public float BassStartOffsetBeats => bassStartOffsetBeats;

        public float QuantizeBeats => quantizeBeats;

        public string SourceFile => sourceFile;

        public float SourceDurationSeconds => sourceDurationSeconds;

        public string TempoNote => tempoNote;

        public bool HasOnsets => onsetBeats.Length > 0;

        public void Replace(float[] newOnsetBeats, string newSourceFile, float newSourceDurationSeconds, string newTempoNote)
        {
            onsetBeats = newOnsetBeats;
            sourceFile = newSourceFile;
            sourceDurationSeconds = newSourceDurationSeconds;
            tempoNote = newTempoNote;
        }

        public BassPulseClock CreateClock(int loopLengthBeats, double loopSeconds) => new BassPulseClock(onsetBeats, loopLengthBeats, loopSeconds);
    }
}
