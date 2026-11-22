using System.Collections.Generic;
using System.Linq;
using DetectiveDave.MusicPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class ChordChartValidationTests
    {
        readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var item in created) Object.DestroyImmediate(item);
            created.Clear();
        }

        AudioClip[] Clips(int count) => Enumerable.Range(1, count).Select(number => Track(AudioClip.Create($"Clip {number}", 100, 1, 44100, false))).ToArray();

        AudioClip Track(AudioClip clip)
        {
            created.Add(clip);
            return clip;
        }

        ChordChart Chart(IEnumerable<ChordSegment> segments, IEnumerable<PianoClipPitch> pitches)
        {
            var chart = ChordChart.Build(ChordFixtures.TempoBpm, ChordFixtures.BeatsPerBar, ChordFixtures.LoopLengthBars, 0f, segments, pitches);
            created.Add(chart);
            return chart;
        }

        IEnumerable<PianoClipPitch> Pitches(AudioClip[] clips) =>
            clips.Select((clip, index) => new PianoClipPitch(clip, ChordFixtures.ClipNotes()[index], "label"));

        static void AssertSome(IReadOnlyList<string> problems, string part) =>
            Assert.IsTrue(problems.Any(problem => problem.Contains(part)), $"Expected a problem containing '{part}' but got:\n{string.Join("\n", problems)}");

        ChordChart ValidChart() => Chart(ChordFixtures.Segments(), Pitches(Clips(12)));

        [Test]
        public void The_chart_of_the_project_loop_has_no_problems()
        {
            Assert.IsEmpty(ChordChartValidation.Problems(ValidChart(), ChordFixtures.LoopSeconds));
        }

        [Test]
        public void A_chart_without_chords_is_reported()
        {
            var chart = Chart(new ChordSegment[0], Pitches(Clips(12)));
            AssertSome(ChordChartValidation.Problems(chart, 0f), "no chords");
        }

        [Test]
        public void A_loop_clip_of_another_length_is_reported()
        {
            AssertSome(ChordChartValidation.Problems(ValidChart(), 24f), "drift");
        }

        [Test]
        public void A_missing_loop_clip_length_is_not_compared()
        {
            Assert.IsEmpty(ChordChartValidation.Problems(ValidChart(), 0f));
        }

        [Test]
        public void A_gap_between_chords_is_reported()
        {
            var segments = ChordFixtures.Segments().Where((segment, index) => index != 3);
            AssertSome(ChordChartValidation.Problems(Chart(segments, Pitches(Clips(12))), 0f), "beats 12 to 16");
        }

        [Test]
        public void A_gap_at_the_start_and_at_the_end_is_reported()
        {
            var segments = ChordFixtures.Segments().Skip(1).Take(6);
            var problems = ChordChartValidation.Problems(Chart(segments, Pitches(Clips(12))), 0f);
            AssertSome(problems, "beats 0 to 4");
            AssertSome(problems, "beats 28 to 32");
        }

        [Test]
        public void Overlapping_chords_are_reported()
        {
            var segments = ChordFixtures.Segments();
            segments[1] = new ChordSegment("Dm7", 3f, 4f, segments[1].ChordToneNumbers, segments[1].ScaleToneNumbers);
            AssertSome(ChordChartValidation.Problems(Chart(segments, Pitches(Clips(12))), 0f), "overlap");
        }

        [Test]
        public void A_chord_that_ends_after_the_loop_is_reported()
        {
            var segments = ChordFixtures.Segments();
            segments[7] = new ChordSegment("E7", 28f, 6f, segments[7].ChordToneNumbers, segments[7].ScaleToneNumbers);
            AssertSome(ChordChartValidation.Problems(Chart(segments, Pitches(Clips(12))), 0f), "after the end");
        }

        [Test]
        public void An_empty_scale_is_reported()
        {
            var segments = ChordFixtures.Segments();
            segments[0] = new ChordSegment("Am7", 0f, 4f, new[] { 9 }, new int[0]);
            AssertSome(ChordChartValidation.Problems(Chart(segments, Pitches(Clips(12))), 0f), "scale has no notes");
        }

        [Test]
        public void A_chord_tone_outside_the_scale_is_reported()
        {
            var segments = ChordFixtures.Segments();
            segments[0] = new ChordSegment("Am7", 0f, 4f, new[] { 9, 1 }, new[] { 9, 0, 4, 7 });
            AssertSome(ChordChartValidation.Problems(Chart(segments, Pitches(Clips(12))), 0f), "chord tones are not in its scale");
        }

        [Test]
        public void A_clip_without_notes_is_reported()
        {
            var clips = Clips(1);
            var chart = Chart(ChordFixtures.Segments(), new[] { new PianoClipPitch(clips[0], new int[0], "none") });
            AssertSome(ChordChartValidation.Problems(chart, 0f), "Clip 1 has no notes");
        }

        [Test]
        public void An_entry_without_a_clip_is_reported()
        {
            var chart = Chart(ChordFixtures.Segments(), new[] { new PianoClipPitch(null, new[] { 0 }, "none") });
            AssertSome(ChordChartValidation.Problems(chart, 0f), "without a clip");
        }

        [Test]
        public void A_clip_listed_twice_is_reported()
        {
            var clips = Clips(1);
            var chart = Chart(ChordFixtures.Segments(), new[] { new PianoClipPitch(clips[0], new[] { 0, 4, 7 }, "a"), new PianoClipPitch(clips[0], new[] { 0, 4, 7 }, "b") });
            AssertSome(ChordChartValidation.Problems(chart, 0f), "listed twice");
        }

        [Test]
        public void A_chord_with_no_fitting_clip_is_reported()
        {
            var clips = Clips(1);
            var chart = Chart(ChordFixtures.Segments(), new[] { new PianoClipPitch(clips[0], new[] { 0, 4, 7 }, "C E G") });
            var problems = ChordChartValidation.Problems(chart, 0f);
            AssertSome(problems, "E7 (chord number 4): no piano clip fits");
            Assert.IsFalse(problems.Any(problem => problem.Contains("Am7 (chord number 1)")));
        }
    }
}
