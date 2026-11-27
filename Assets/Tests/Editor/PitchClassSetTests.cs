using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PitchClassSetTests
    {
        [Test]
        public void The_octave_does_not_matter()
        {
            var set = PitchClassSet.Of(new[] { 60 });
            Assert.IsTrue(set.Contains(0));
            Assert.IsTrue(set.Contains(72));
            Assert.IsTrue(set.Contains(48));
            Assert.IsFalse(set.Contains(61));
        }

        [Test]
        public void Negative_notes_wrap_into_the_octave()
        {
            Assert.IsTrue(PitchClassSet.Of(new[] { -1 }).Contains(11));
        }

        [Test]
        public void A_set_contains_a_smaller_set()
        {
            var scale = PitchClassSet.Of(new[] { 9, 11, 0, 2, 4, 7 });
            Assert.IsTrue(scale.ContainsAll(PitchClassSet.Of(new[] { 0, 4, 7 })));
            Assert.IsFalse(scale.ContainsAll(PitchClassSet.Of(new[] { 0, 4, 8 })));
        }

        [Test]
        public void An_empty_set_is_empty_and_a_set_with_notes_is_not()
        {
            Assert.IsTrue(PitchClassSet.Of(null).IsEmpty);
            Assert.IsTrue(PitchClassSet.Of(new int[0]).IsEmpty);
            Assert.IsFalse(PitchClassSet.Of(new[] { 3 }).IsEmpty);
        }

        [Test]
        public void Note_names_follow_the_numbers()
        {
            Assert.AreEqual("A C E G", PitchClassNames.Of(new[] { 9, 0, 4, 7 }));
            Assert.AreEqual("C# G#", PitchClassNames.Of(new[] { 1, 8 }));
            Assert.AreEqual(string.Empty, PitchClassNames.Of(null));
        }
    }
}
