using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class SinglePitchTests
    {
        static int Parsed(string name)
        {
            Assert.IsTrue(SinglePitch.TryParse(name, out var midiPitch, out var problem), problem);
            return midiPitch;
        }

        static void AssertRejected(string name)
        {
            Assert.IsFalse(SinglePitch.TryParse(name, out _, out var problem));
            Assert.IsNotEmpty(problem);
        }

        [TestCase("Piano Single - A3", 57)]
        [TestCase("Piano Single - C4", 60)]
        [TestCase("Piano Single - Gs3", 56)]
        [TestCase("Piano Single - Cs4", 61)]
        [TestCase("Piano Single - E7", 100)]
        public void A_name_gives_its_midi_pitch(string name, int expected)
        {
            Assert.AreEqual(expected, Parsed(name));
        }

        [Test]
        public void Case_does_not_matter()
        {
            Assert.AreEqual(56, Parsed("piano single - gs3"));
            Assert.AreEqual(56, Parsed("PIANO SINGLE - GS3"));
        }

        [Test]
        public void Extra_spaces_are_allowed()
        {
            Assert.AreEqual(57, Parsed("  Piano   Single  -   A3  "));
            Assert.AreEqual(57, Parsed("Piano Single-A3"));
        }

        [Test]
        public void The_lowest_note_is_octave_minus_one()
        {
            Assert.AreEqual(0, Parsed("Piano Single - C-1"));
            Assert.AreEqual(9, Parsed("Piano Single - A-1"));
        }

        [Test]
        public void The_highest_note_is_g_in_octave_nine()
        {
            Assert.AreEqual(127, Parsed("Piano Single - G9"));
        }

        [TestCase("Piano Single - Gs9")]
        [TestCase("Piano Single - A9")]
        [TestCase("Piano Single - A-2")]
        public void A_note_outside_the_midi_range_is_rejected(string name)
        {
            AssertRejected(name);
        }

        [TestCase("Piano Single - Bb3")]
        [TestCase("Piano Single - Ab3")]
        [TestCase("Piano Single - H3")]
        [TestCase("Piano Single - A")]
        [TestCase("Piano Single - 3")]
        [TestCase("Piano Single - A33")]
        [TestCase("Piano Single A3")]
        [TestCase("Jazz - Piano - 1")]
        [TestCase("")]
        public void A_name_that_does_not_follow_the_pattern_is_rejected(string name)
        {
            AssertRejected(name);
        }

        [Test]
        public void A_missing_name_is_rejected()
        {
            AssertRejected(null);
        }

        [Test]
        public void A_rejected_name_is_named_in_the_problem()
        {
            SinglePitch.TryParse("Piano Single - Bb3", out _, out var problem);
            StringAssert.Contains("Piano Single - Bb3", problem);
            StringAssert.Contains("Gs3", problem);
        }

        [Test]
        public void A_sharp_of_e_is_f_and_a_sharp_of_b_is_the_next_c()
        {
            Assert.AreEqual(53, Parsed("Piano Single - Es3"));
            Assert.AreEqual(60, Parsed("Piano Single - Bs3"));
        }

        [TestCase(56, "Gs3", "G#3")]
        [TestCase(60, "C4", "C4")]
        [TestCase(0, "C-1", "C-1")]
        [TestCase(127, "G9", "G9")]
        [TestCase(69, "A4", "A4")]
        public void A_midi_pitch_has_a_file_name_and_a_display_name(int midiPitch, string fileName, string display)
        {
            Assert.AreEqual(fileName, SinglePitch.FileNameOf(midiPitch));
            Assert.AreEqual(display, SinglePitch.DisplayName(midiPitch));
        }

        [Test]
        public void The_file_name_of_a_pitch_parses_back_to_the_pitch()
        {
            for (var midiPitch = 0; midiPitch <= 127; midiPitch++)
                Assert.AreEqual(midiPitch, Parsed("Piano Single - " + SinglePitch.FileNameOf(midiPitch)));
        }

        [Test]
        public void The_pitch_class_ignores_the_octave()
        {
            Assert.AreEqual(8, SinglePitch.PitchClassOf(56));
            Assert.AreEqual(8, SinglePitch.PitchClassOf(68));
            Assert.AreEqual(0, SinglePitch.PitchClassOf(60));
        }
    }
}
