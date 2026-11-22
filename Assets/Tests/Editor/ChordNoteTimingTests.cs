using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class ChordNoteTimingTests
    {
        static ChordNoteTiming Build(double loopSeconds, QuantizeGrid grid, float maxDelay)
        {
            var clock = ChordFixtures.Clock();
            return new ChordNoteTiming(clock, new NoteQuantizer(clock, grid, maxDelay, 0f, false), () => loopSeconds);
        }

        [Test]
        public void A_note_pressed_before_a_chord_change_is_judged_by_the_chord_it_lands_in()
        {
            var timing = Build(3.0, QuantizeGrid.Quarter, 0.5f);
            Assert.IsTrue(timing.TryPlan(out var delay, out var moment));
            Assert.AreEqual(0.2, delay, 1e-4);
            Assert.AreEqual(1, moment.SegmentIndex);
            Assert.IsTrue(moment.IsChordStart);
        }

        [Test]
        public void A_note_played_at_once_is_judged_by_the_chord_of_the_press()
        {
            var timing = Build(3.0, QuantizeGrid.Off, 0.5f);
            Assert.IsTrue(timing.TryPlan(out var delay, out var moment));
            Assert.AreEqual(0.0, delay);
            Assert.AreEqual(0, moment.SegmentIndex);
        }

        [Test]
        public void A_note_too_far_from_the_grid_is_judged_by_the_chord_of_the_press()
        {
            var timing = Build(2.5, QuantizeGrid.Quarter, 0.1f);
            Assert.IsTrue(timing.TryPlan(out var delay, out var moment));
            Assert.AreEqual(0.0, delay);
            Assert.AreEqual(0, moment.SegmentIndex);
        }

        [Test]
        public void A_dropped_note_has_no_moment()
        {
            var clock = ChordFixtures.Clock();
            var timing = new ChordNoteTiming(clock, new NoteQuantizer(clock, QuantizeGrid.Quarter, 0.5f, 0f, true), () => 3.0);
            timing.TryPlan(out _, out _);
            Assert.IsFalse(timing.TryPlan(out _, out var moment));
            Assert.IsFalse(moment.IsInsideChart);
        }

        [Test]
        public void The_loop_position_is_read_at_every_press()
        {
            var seconds = 0.0;
            var clock = ChordFixtures.Clock();
            var timing = new ChordNoteTiming(clock, new NoteQuantizer(clock, QuantizeGrid.Off, 0f, 0f, false), () => seconds);
            timing.TryPlan(out _, out var first);
            seconds = 10.0;
            timing.TryPlan(out _, out var second);
            Assert.AreEqual(0, first.SegmentIndex);
            Assert.AreEqual(3, second.SegmentIndex);
        }
    }
}
