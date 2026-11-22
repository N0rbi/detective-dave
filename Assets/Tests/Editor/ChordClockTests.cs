using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class ChordClockTests
    {
        const double Precision = 1e-4;

        [Test]
        public void At_75_bpm_a_beat_lasts_0_8_seconds_and_the_loop_25_6()
        {
            var clock = ChordFixtures.Clock();
            Assert.AreEqual(0.8, clock.SecondsPerBeat, Precision);
            Assert.AreEqual(25.6, clock.LoopSeconds, Precision);
        }

        [Test]
        public void The_start_of_the_loop_is_the_first_beat_of_the_first_chord()
        {
            var moment = ChordFixtures.Clock().MomentAt(0.0);
            Assert.AreEqual(0, moment.SegmentIndex);
            Assert.AreEqual(0, moment.BeatInBar);
            Assert.IsTrue(moment.IsStrongBeat);
            Assert.IsTrue(moment.IsChordStart);
        }

        [Test]
        public void The_chord_changes_exactly_at_the_chord_boundary()
        {
            var clock = ChordFixtures.Clock();
            Assert.AreEqual(0, clock.MomentAt(3.1999).SegmentIndex);
            Assert.AreEqual(1, clock.MomentAt(3.2001).SegmentIndex);
            Assert.AreEqual(1, clock.MomentAt(3.2).SegmentIndex);
        }

        [Test]
        public void The_second_and_fourth_beat_of_a_bar_are_weak()
        {
            var clock = ChordFixtures.Clock();
            Assert.IsFalse(clock.MomentAt(0.8 * 5 + 0.01).IsStrongBeat);
            Assert.IsTrue(clock.MomentAt(0.8 * 6 + 0.01).IsStrongBeat);
            Assert.IsFalse(clock.MomentAt(0.8 * 7 + 0.01).IsStrongBeat);
            Assert.AreEqual(1, clock.MomentAt(0.8 * 5 + 0.01).BeatInBar);
        }

        [Test]
        public void Only_the_first_beat_of_a_chord_is_a_chord_start()
        {
            var clock = ChordFixtures.Clock();
            Assert.IsTrue(clock.MomentAt(3.2 + 0.1).IsChordStart);
            Assert.IsFalse(clock.MomentAt(3.2 + 0.9).IsChordStart);
        }

        [Test]
        public void The_clock_wraps_at_the_end_of_the_loop()
        {
            var clock = ChordFixtures.Clock();
            Assert.AreEqual(0, clock.MomentAt(25.6 + 0.1).SegmentIndex);
            Assert.AreEqual(1, clock.MomentAt(25.6 + 3.3).SegmentIndex);
            Assert.AreEqual(7, clock.MomentAt(25.5).SegmentIndex);
        }

        [Test]
        public void Many_loops_later_it_still_agrees()
        {
            var clock = ChordFixtures.Clock();
            Assert.AreEqual(clock.MomentAt(5.0).SegmentIndex, clock.MomentAt(5.0 + 25.6 * 40).SegmentIndex);
        }

        [Test]
        public void The_chart_beat_at_loop_start_shifts_the_chords()
        {
            var clock = ChordFixtures.Clock(12f);
            Assert.AreEqual(3, clock.MomentAt(0.0).SegmentIndex);
            Assert.AreEqual(4, clock.MomentAt(3.3).SegmentIndex);
        }

        [Test]
        public void A_position_outside_every_chord_is_outside_the_chart()
        {
            var clock = new ChordClock(75f, 4, 8, 0f, new[] { new ChordSegment("Am7", 0f, 4f, new[] { 9 }, new[] { 9 }) });
            Assert.IsFalse(clock.MomentAt(5.0).IsInsideChart);
            Assert.AreEqual(ChordMoment.NoSegment, clock.MomentAt(5.0).SegmentIndex);
        }

        [Test]
        public void The_next_eighth_note_slot_is_found_with_its_delay()
        {
            var slot = ChordFixtures.Clock().NextSlot(0.1, 0.5f);
            Assert.AreEqual(1, slot.Index);
            Assert.AreEqual(0.3, slot.DelaySeconds, Precision);
        }

        [Test]
        public void A_position_exactly_on_a_slot_has_no_delay()
        {
            var slot = ChordFixtures.Clock().NextSlot(0.4, 0.5f);
            Assert.AreEqual(1, slot.Index);
            Assert.Less(slot.DelaySeconds, 1e-5);
        }

        [Test]
        public void Triplet_eighths_are_a_third_of_a_beat_apart()
        {
            var slot = ChordFixtures.Clock().NextSlot(0.1, 1f / 3f);
            Assert.AreEqual(1, slot.Index);
            Assert.AreEqual(0.16667, slot.DelaySeconds, Precision);
        }
    }
}
