using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class ChordPaletteTests
    {
        [Test]
        public void Over_Am7_only_the_clips_made_of_C_E_G_fit()
        {
            CollectionAssert.AreEqual(new[] { 0, 6 }, ChordFixtures.Palette().Allowed(0));
        }

        [Test]
        public void Over_Dm7_and_Bm7b5_the_chords_without_G_sharp_fit()
        {
            var expected = new[] { 0, 1, 2, 3, 6, 7, 8, 9 };
            CollectionAssert.AreEqual(expected, ChordFixtures.Palette().Allowed(1));
            CollectionAssert.AreEqual(expected, ChordFixtures.Palette().Allowed(2));
        }

        [Test]
        public void Over_E7_every_clip_without_G_fits()
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 7, 8, 9, 10, 11 }, ChordFixtures.Palette().Allowed(3));
        }

        [Test]
        public void A_repeated_chord_gives_the_same_clips()
        {
            var palette = ChordFixtures.Palette();
            CollectionAssert.AreEqual(palette.Allowed(0), palette.Allowed(4));
            CollectionAssert.AreEqual(palette.Allowed(3), palette.Allowed(7));
        }

        [Test]
        public void A_clip_made_of_chord_tones_is_a_chord_tone_clip()
        {
            var palette = ChordFixtures.Palette();
            Assert.IsTrue(palette.IsChordTone(0, 0));
            Assert.IsTrue(palette.IsChordTone(1, 1));
            Assert.IsTrue(palette.IsChordTone(1, 3));
            Assert.IsTrue(palette.IsChordTone(3, 4));
        }

        [Test]
        public void A_clip_that_only_fits_the_scale_is_not_a_chord_tone_clip()
        {
            var palette = ChordFixtures.Palette();
            Assert.IsFalse(palette.IsChordTone(1, 0));
            Assert.IsFalse(palette.IsChordTone(2, 1));
            Assert.IsFalse(palette.IsChordTone(3, 1));
        }

        [Test]
        public void A_clip_without_notes_never_fits()
        {
            var palette = new ChordPalette(ChordFixtures.Segments(), new[] { PitchClassSet.Of(new int[0]), PitchClassSet.Of(new[] { 0, 4, 7 }) });
            CollectionAssert.AreEqual(new[] { 1 }, palette.Allowed(0));
            Assert.IsFalse(palette.IsChordTone(0, 0));
        }

        [Test]
        public void Unknown_chords_allow_no_clips_and_have_no_name()
        {
            var palette = ChordFixtures.Palette();
            Assert.IsEmpty(palette.Allowed(-1));
            Assert.IsEmpty(palette.Allowed(99));
            Assert.AreEqual(string.Empty, palette.ChordName(99));
            Assert.AreEqual("E7", palette.ChordName(3));
        }

        [Test]
        public void The_palette_knows_how_many_clips_there_are()
        {
            Assert.AreEqual(12, ChordFixtures.Palette().ClipCount);
        }
    }
}
