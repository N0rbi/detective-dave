using DetectiveDave.EditorTools;
using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class ChordChartSummaryTests
    {
        [Test]
        public void A_chord_is_described_with_note_names()
        {
            Assert.AreEqual("Am7: chord A C E G, scale A B C D E G", ChordChartSummary.SegmentLine(ChordFixtures.Segments()[0]));
        }

        [Test]
        public void A_clip_lists_the_chords_it_plays_over_once()
        {
            var line = ChordChartSummary.ClipLine(new PianoClipPitch(null, new[] { 5, 9, 0 }, "F4 A4 C5"), ChordFixtures.Segments());
            Assert.AreEqual("(no clip): F A C, plays over Dm7, Bm7b5, E7", line);
        }

        [Test]
        public void A_clip_that_fits_no_chord_says_so()
        {
            var line = ChordChartSummary.ClipLine(new PianoClipPitch(null, new[] { 1 }, "x"), ChordFixtures.Segments());
            StringAssert.EndsWith("plays over no chord", line);
        }

        [Test]
        public void A_clip_without_notes_fits_no_chord()
        {
            var line = ChordChartSummary.ClipLine(new PianoClipPitch(null, new int[0], "x"), ChordFixtures.Segments());
            StringAssert.EndsWith("plays over no chord", line);
        }
    }
}
