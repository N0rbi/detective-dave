using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public static class ChordChartValidation
    {
        const float BeatTolerance = 0.001f;
        const float LoopClipToleranceSeconds = 0.05f;

        public static IReadOnlyList<string> Problems(ChordChart chart, float loopClipSeconds)
        {
            var problems = new List<string>();
            problems.AddRange(TimingProblems(chart, loopClipSeconds));
            problems.AddRange(SegmentProblems(chart));
            problems.AddRange(CoverageProblems(chart));
            problems.AddRange(ClipProblems(chart));
            problems.AddRange(ChordWithoutClipProblems(chart));
            return problems;
        }

        static IEnumerable<string> TimingProblems(ChordChart chart, float loopClipSeconds)
        {
            if (chart.TempoBpm <= 0f) yield return "The tempo must be above 0.";
            if (chart.Segments.Count == 0) yield return "The chart has no chords.";
            if (loopClipSeconds > 0f && Math.Abs(chart.LoopSeconds - loopClipSeconds) > LoopClipToleranceSeconds)
                yield return $"The chart lasts {chart.LoopSeconds:0.00} s ({chart.LoopLengthBars} bars of {chart.BeatsPerBar} beats at {chart.TempoBpm:0.#} BPM) but the loop clip lasts {loopClipSeconds:0.00} s, so the chords will drift against the music.";
        }

        static IEnumerable<string> SegmentProblems(ChordChart chart) =>
            chart.Segments.SelectMany(segment => ProblemsOf(segment, chart.LoopLengthBeats));

        static IEnumerable<string> ProblemsOf(ChordSegment segment, int loopBeats)
        {
            if (segment.LengthBeats <= 0f) yield return $"{segment.ChordName}: the length must be above 0 beats.";
            if (segment.ScaleTones.IsEmpty) yield return $"{segment.ChordName}: the scale has no notes, so no piano clip fits it and When No Clip Fits decides what plays over it.";
            if (!segment.ScaleTones.ContainsAll(segment.ChordTones)) yield return $"{segment.ChordName}: some chord tones are not in its scale.";
            if (segment.EndBeat > loopBeats + BeatTolerance) yield return $"{segment.ChordName}: it ends at beat {segment.EndBeat:0.##}, after the end of the loop at beat {loopBeats}.";
        }

        static IEnumerable<string> CoverageProblems(ChordChart chart)
        {
            var ordered = chart.Segments.OrderBy(segment => segment.StartBeat).ToList();
            if (ordered.Count == 0) yield break;
            if (ordered[0].StartBeat > BeatTolerance) yield return $"No chord covers beats 0 to {ordered[0].StartBeat:0.##}.";
            for (var index = 1; index < ordered.Count; index++)
            {
                var gap = ordered[index].StartBeat - ordered[index - 1].EndBeat;
                if (gap > BeatTolerance) yield return $"No chord covers beats {ordered[index - 1].EndBeat:0.##} to {ordered[index].StartBeat:0.##}.";
                if (gap < -BeatTolerance) yield return $"{ordered[index - 1].ChordName} and {ordered[index].ChordName} overlap around beat {ordered[index].StartBeat:0.##}.";
            }
            if (ordered.Last().EndBeat < chart.LoopLengthBeats - BeatTolerance) yield return $"No chord covers beats {ordered.Last().EndBeat:0.##} to {chart.LoopLengthBeats}.";
        }

        static IEnumerable<string> ClipProblems(ChordChart chart)
        {
            var seen = new HashSet<UnityEngine.AudioClip>();
            foreach (var pitch in chart.ClipPitches)
            {
                if (!pitch.Clip) yield return "Clip Pitches has an entry without a clip.";
                else if (!seen.Add(pitch.Clip)) yield return $"{pitch.Clip.name} is listed twice in Clip Pitches.";
                else if (pitch.PitchClasses.IsEmpty) yield return $"{pitch.Clip.name} has no notes in Clip Pitches, so it never plays.";
            }
        }

        static IEnumerable<string> ChordWithoutClipProblems(ChordChart chart)
        {
            var clipPitches = chart.ClipPitches.Select(pitch => pitch.PitchClasses).ToList();
            var palette = new ChordPalette(chart.Segments, clipPitches);
            for (var index = 0; index < chart.Segments.Count; index++)
                if (palette.Allowed(index).Count == 0) yield return $"{chart.Segments[index].ChordName} (chord number {index + 1}): no piano clip fits its scale, so When No Clip Fits decides what plays over it.";
        }
    }
}
