using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class TimedRevealTests
    {
        [Test]
        public void Reveals_at_a_constant_speed_and_stops_at_the_length()
        {
            var reveal = new TimedReveal(10, 0.25f, 3f);
            Assert.AreEqual(0, reveal.VisibleCount);
            Assert.AreEqual(4, reveal.Advance(1f));
            Assert.IsFalse(reveal.IsDone);
            reveal.Advance(10f);
            Assert.AreEqual(10, reveal.VisibleCount);
            Assert.IsTrue(reveal.IsDone);
        }

        [Test]
        public void Skipping_multiplies_the_speed_from_then_on()
        {
            var reveal = new TimedReveal(100, 0.25f, 3f);
            reveal.Advance(1f);
            reveal.Skip();
            reveal.Advance(1f);
            Assert.AreEqual(4 + 12, reveal.VisibleCount);
            Assert.IsTrue(reveal.IsSkipping);
        }

        [Test]
        public void An_empty_text_is_done_at_once()
        {
            Assert.IsTrue(new TimedReveal(0, 0.1f, 3f).IsDone);
        }
    }

    public class BriefingGateTests
    {
        static BriefingGate NewGate(int length = 10) => new BriefingGate(new TimedReveal(length, 0.25f, 3f));

        [Test]
        public void The_letter_is_not_writable_until_the_description_is_finished()
        {
            var gate = NewGate();
            gate.Tick(1f);
            Assert.IsFalse(gate.IsWritable);
            gate.Tick(1f);
            Assert.IsFalse(gate.IsWritable);
            gate.Tick(0.6f);
            Assert.IsTrue(gate.IsWritable);
        }

        [Test]
        public void Any_key_before_the_gate_opens_speeds_the_description_up_for_good()
        {
            var normal = NewGate(100);
            var skipped = NewGate(100);
            skipped.OnAnyKey();
            normal.Tick(2f);
            skipped.Tick(2f);
            Assert.AreEqual(8, normal.VisibleCount);
            Assert.AreEqual(24, skipped.VisibleCount);
            skipped.Tick(2f);
            Assert.AreEqual(48, skipped.VisibleCount);
        }

        [Test]
        public void Keys_after_the_gate_opens_change_nothing()
        {
            var gate = NewGate(4);
            gate.Tick(5f);
            Assert.IsTrue(gate.IsWritable);
            gate.OnAnyKey();
            Assert.AreEqual(4, gate.VisibleCount);
            Assert.AreEqual(0, gate.Tick(5f));
        }

        [Test]
        public void An_empty_description_opens_the_gate_on_the_first_tick()
        {
            var gate = NewGate(0);
            Assert.IsFalse(gate.IsWritable);
            gate.Tick(0f);
            Assert.IsTrue(gate.IsWritable);
        }
    }

    public class BriefingLayoutTests
    {
        [Test]
        public void Places_the_paper_relative_to_the_view()
        {
            var spot = BriefingLayout.Place(new PaperPlacement(-0.25f, 0.1f, 0.4f, -2f), new Vector2(1f, 2f), new Vector2(20f, 10f));
            Assert.AreEqual(-4f, spot.Centre.x, 0.001f);
            Assert.AreEqual(3f, spot.Centre.y, 0.001f);
            Assert.AreEqual(8f, spot.Width, 0.001f);
            Assert.AreEqual(-2f, spot.RotationDegrees);
        }

        [Test]
        public void Counts_the_letters_that_fit_inside_the_padding()
        {
            Assert.AreEqual(20, BriefingLayout.LettersPerLine(10f, 0.1f, 0.5f, 0.8f));
            Assert.AreEqual(1, BriefingLayout.LettersPerLine(0.1f, 0.4f, 5f, 1f));
        }
    }

    public class FindingsPaperModelTests
    {
        [Test]
        public void Tints_the_revealed_part_and_leaves_the_rest_pending()
        {
            var model = new FindingsPaperModel("abc def", 100);
            Assert.AreEqual("<color=#111>abc </color><color=#999>def</color>", model.Render(4, "111", "999"));
        }

        [Test]
        public void Shows_everything_pending_at_the_start_and_everything_typed_at_the_end()
        {
            var model = new FindingsPaperModel("abc", 100);
            Assert.AreEqual("<color=#999>abc</color>", model.Render(0, "111", "999"));
            Assert.AreEqual("<color=#111>abc</color>", model.Render(99, "111", "999"));
        }

        [Test]
        public void Wrapping_keeps_the_length_so_typed_counts_still_line_up()
        {
            var model = new FindingsPaperModel("aaa bbb ccc", 7);
            Assert.AreEqual(11, model.Plain.Length);
        }
    }
}
