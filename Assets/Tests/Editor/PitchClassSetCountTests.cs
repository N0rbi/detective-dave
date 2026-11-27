using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PitchClassSetCountTests
    {
        [Test]
        public void Count_in_is_the_number_of_notes_that_are_also_in_the_scale()
        {
            var clip = PitchClassSet.Of(new[] { 0, 4, 9 });
            var scale = PitchClassSet.Of(new[] { 0, 4, 7 });
            Assert.AreEqual(2, clip.CountIn(scale));
        }

        [Test]
        public void Octaves_count_as_the_same_note()
        {
            Assert.AreEqual(1, PitchClassSet.Of(new[] { 60 }).CountIn(PitchClassSet.Of(new[] { 0 })));
        }

        [Test]
        public void An_empty_set_has_no_notes_in_any_scale()
        {
            Assert.AreEqual(0, PitchClassSet.Of(new int[0]).CountIn(PitchClassSet.Of(new[] { 0, 4, 7 })));
        }

        [Test]
        public void A_note_that_is_in_the_set_twice_counts_once()
        {
            Assert.AreEqual(1, PitchClassSet.Of(new[] { 0, 12 }).CountIn(PitchClassSet.Of(new[] { 0 })));
        }
    }
}
