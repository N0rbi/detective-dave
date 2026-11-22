using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Chord Chart", fileName = "Chord Chart.config.asset")]
    public sealed class ChordChart : ScriptableObject
    {
        [Header("Loop timing")]
        [SerializeField, Min(1f)] [Tooltip("Tempo of the jazz loop in beats per minute. The loop in the project is 75.")] float tempoBpm = 75f;
        [SerializeField, Min(1)] [Tooltip("Beats in one bar. 4 for 4/4.")] int beatsPerBar = 4;
        [SerializeField, Min(1)] [Tooltip("How many bars the loop lasts. Bars times beats per bar times 60 divided by the tempo must equal the length of the loop clip in seconds.")] int loopLengthBars = 8;
        [SerializeField, Min(0f)] [Tooltip("Which beat of the chart sounds at the very start of the loop clip. 0 when the clip starts on the first chord. If the clip starts on another bar, enter that bar's first beat, for example 12 when it starts on the fourth chord of the 4 beat chords.")] float chartBeatAtLoopStart;

        [Header("Chords")]
        [SerializeField] [Tooltip("The chords of the loop in order. Together they must cover every beat of the loop without gaps. Select the asset to see them drawn on a timeline.")] ChordSegment[] segments = new ChordSegment[0];

        [Header("Piano clips")]
        [SerializeField] [Tooltip("The notes inside every piano clip. The game plays a clip only over chords whose scale contains all of its notes.")] PianoClipPitch[] clipPitches = new PianoClipPitch[0];

        public float TempoBpm => tempoBpm;

        public int BeatsPerBar => beatsPerBar;

        public int LoopLengthBars => loopLengthBars;

        public float ChartBeatAtLoopStart => chartBeatAtLoopStart;

        public IReadOnlyList<ChordSegment> Segments => segments;

        public IReadOnlyList<PianoClipPitch> ClipPitches => clipPitches;

        public int LoopLengthBeats => beatsPerBar * loopLengthBars;

        public double LoopSeconds => LoopLengthBeats * 60.0 / tempoBpm;

        public bool HasData => segments.Length > 0 || clipPitches.Length > 0;

        public static ChordChart Build(float tempoBpm, int beatsPerBar, int loopLengthBars, float chartBeatAtLoopStart, IEnumerable<ChordSegment> segments, IEnumerable<PianoClipPitch> clipPitches)
        {
            var chart = CreateInstance<ChordChart>();
            chart.Replace(tempoBpm, beatsPerBar, loopLengthBars, chartBeatAtLoopStart, segments, clipPitches);
            return chart;
        }

        public void Replace(float newTempoBpm, int newBeatsPerBar, int newLoopLengthBars, float newChartBeatAtLoopStart, IEnumerable<ChordSegment> newSegments, IEnumerable<PianoClipPitch> newClipPitches)
        {
            tempoBpm = newTempoBpm;
            beatsPerBar = newBeatsPerBar;
            loopLengthBars = newLoopLengthBars;
            chartBeatAtLoopStart = newChartBeatAtLoopStart;
            segments = newSegments.ToArray();
            clipPitches = newClipPitches.ToArray();
        }

        public ChordClock CreateClock() => new ChordClock(tempoBpm, beatsPerBar, loopLengthBars, chartBeatAtLoopStart, segments);
    }
}
