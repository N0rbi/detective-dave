using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class ChordTimelineLayoutTests
    {
        [Test]
        public void A_chord_takes_its_share_of_the_loop()
        {
            var span = ChordTimelineLayout.SpanOf(4f, 8f, 32f);
            Assert.AreEqual(0.125f, span.Start, 1e-6f);
            Assert.AreEqual(0.125f, span.Length, 1e-6f);
        }

        [Test]
        public void A_chord_past_the_end_of_the_loop_is_cut_off()
        {
            var span = ChordTimelineLayout.SpanOf(28f, 36f, 32f);
            Assert.AreEqual(0.875f, span.Start, 1e-6f);
            Assert.AreEqual(0.125f, span.Length, 1e-6f);
        }

        [Test]
        public void A_chord_before_the_start_of_the_loop_is_cut_off()
        {
            var span = ChordTimelineLayout.SpanOf(-4f, 4f, 32f);
            Assert.AreEqual(0f, span.Start);
            Assert.AreEqual(0.125f, span.Length, 1e-6f);
        }

        [Test]
        public void Bar_lines_include_both_ends_of_the_loop()
        {
            var lines = ChordTimelineLayout.BarLines(8);
            Assert.AreEqual(9, lines.Count);
            Assert.AreEqual(0f, lines[0]);
            Assert.AreEqual(1f, lines[8]);
            Assert.AreEqual(0.5f, lines[4], 1e-6f);
        }
    }
}
