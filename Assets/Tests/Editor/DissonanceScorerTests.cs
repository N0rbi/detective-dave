using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DissonanceScorerTests
    {
        static readonly DissonanceWeights Weights = DissonanceWeights.Defaults;

        static ChordSegment Chord(int[] chord, int[] scale) => new ChordSegment("X", 0f, 4f, chord, scale);

        static PitchClassSet Notes(params int[] notes) => PitchClassSet.Of(notes);

        static readonly ChordSegment CMajor = Chord(new[] { 0, 4, 7 }, new[] { 0, 2, 4, 5, 7, 9, 11 });

        [Test]
        public void A_chord_tone_scores_zero() => Assert.AreEqual(0f, DissonanceScorer.Score(Notes(0, 4, 7), CMajor, Weights, false), 0.0001f);

        [Test]
        public void A_scale_note_that_is_not_a_chord_tone_scores_its_small_weight() =>
            Assert.AreEqual(Weights.ScaleNonChordTone, DissonanceScorer.Score(Notes(2), CMajor, Weights, false), 0.0001f);

        [Test]
        public void A_semitone_above_the_chord_tone_is_a_minor_second()
        {
            var chord = Chord(new[] { 0 }, new[] { 0 });
            Assert.AreEqual(1.0f, DissonanceScorer.Score(Notes(1), chord, Weights, false), 0.0001f);
        }

        [Test]
        public void A_semitone_below_the_chord_tone_is_a_major_seventh()
        {
            var chord = Chord(new[] { 0 }, new[] { 0 });
            Assert.AreEqual(0.95f, DissonanceScorer.Score(Notes(11), chord, Weights, false), 0.0001f);
        }

        [Test]
        public void Six_semitones_away_is_a_tritone()
        {
            var chord = Chord(new[] { 0 }, new[] { 0 });
            Assert.AreEqual(0.8f, DissonanceScorer.Score(Notes(6), chord, Weights, false), 0.0001f);
        }

        [Test]
        public void Two_semitones_above_is_a_major_second_and_below_a_minor_seventh()
        {
            var chord = Chord(new[] { 0 }, new[] { 0 });
            Assert.AreEqual(0.5f, DissonanceScorer.Score(Notes(2), chord, Weights, false), 0.0001f);
            Assert.AreEqual(0.45f, DissonanceScorer.Score(Notes(10), chord, Weights, false), 0.0001f);
        }

        [Test]
        public void Other_intervals_use_the_other_weight()
        {
            var chord = Chord(new[] { 0 }, new[] { 0 });
            foreach (var note in new[] { 3, 4, 5, 7, 8, 9 })
                Assert.AreEqual(0.3f, DissonanceScorer.Score(Notes(note), chord, Weights, false), 0.0001f, $"note {note}");
        }

        [Test]
        public void A_note_between_two_chord_tones_uses_the_nearest_and_the_harsher_on_a_tie()
        {
            var chord = Chord(new[] { 0, 2 }, new[] { 0, 2 });
            Assert.AreEqual(1.0f, DissonanceScorer.Score(Notes(1), chord, Weights, false), 0.0001f);
        }

        [Test]
        public void A_clip_scores_the_sum_of_its_notes() =>
            Assert.AreEqual(1.0f + 0.1f, DissonanceScorer.Score(Notes(1, 2), Chord(new[] { 0 }, new[] { 0, 2 }), Weights, false), 0.0001f);

        [Test]
        public void Normalising_averages_over_the_notes() =>
            Assert.AreEqual((1.0f + 0.1f) / 2f, DissonanceScorer.Score(Notes(1, 2), Chord(new[] { 0 }, new[] { 0, 2 }), Weights, true), 0.0001f);

        [Test]
        public void Custom_weights_are_used()
        {
            var weights = new DissonanceWeights();
            var chord = Chord(new[] { 0 }, new[] { 0 });
            Assert.AreEqual(weights.Tritone, DissonanceScorer.Score(Notes(6), chord, weights, false), 0.0001f);
        }
    }
}
