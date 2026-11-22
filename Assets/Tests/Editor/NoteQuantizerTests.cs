using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class NoteQuantizerTests
    {
        const double Precision = 1e-4;

        static NoteQuantizer Build(QuantizeGrid grid, float maxDelay, float lead = 0f, bool dropExtra = false) =>
            new NoteQuantizer(ChordFixtures.Clock(), grid, maxDelay, lead, dropExtra);

        [Test]
        public void With_the_grid_off_every_note_plays_at_once()
        {
            var quantizer = Build(QuantizeGrid.Off, 1f);
            Assert.IsTrue(quantizer.TryPlan(0.1, out var first));
            Assert.IsTrue(quantizer.TryPlan(0.1, out var second));
            Assert.AreEqual(0.0, first);
            Assert.AreEqual(0.0, second);
        }

        [Test]
        public void A_note_is_held_back_to_the_next_grid_point()
        {
            Assert.IsTrue(Build(QuantizeGrid.Eighth, 0.5f).TryPlan(0.1, out var delay));
            Assert.AreEqual(0.3, delay, Precision);
        }

        [Test]
        public void A_note_whose_next_point_is_too_far_away_plays_at_once()
        {
            Assert.IsTrue(Build(QuantizeGrid.Eighth, 0.2f).TryPlan(0.1, out var delay));
            Assert.AreEqual(0.0, delay);
        }

        [Test]
        public void A_second_note_in_the_same_slot_takes_the_following_slot()
        {
            var quantizer = Build(QuantizeGrid.Eighth, 1f);
            quantizer.TryPlan(0.1, out _);
            Assert.IsTrue(quantizer.TryPlan(0.2, out var delay));
            Assert.AreEqual(0.6, delay, Precision);
        }

        [Test]
        public void A_second_note_in_the_same_slot_is_dropped_when_extra_notes_are_dropped()
        {
            var quantizer = Build(QuantizeGrid.Eighth, 1f, 0f, true);
            quantizer.TryPlan(0.1, out _);
            Assert.IsFalse(quantizer.TryPlan(0.2, out _));
        }

        [Test]
        public void A_second_note_is_dropped_when_the_following_slot_is_too_far_away()
        {
            var quantizer = Build(QuantizeGrid.Eighth, 0.35f);
            quantizer.TryPlan(0.1, out _);
            Assert.IsFalse(quantizer.TryPlan(0.2, out _));
        }

        [Test]
        public void A_note_in_a_later_slot_is_not_affected_by_an_earlier_one()
        {
            var quantizer = Build(QuantizeGrid.Eighth, 1f);
            quantizer.TryPlan(0.1, out _);
            Assert.IsTrue(quantizer.TryPlan(0.5, out var delay));
            Assert.AreEqual(0.3, delay, Precision);
        }

        [Test]
        public void The_lead_time_moves_a_note_that_would_start_too_soon_to_the_next_slot()
        {
            var quantizer = Build(QuantizeGrid.Eighth, 1f, 0.1f);
            Assert.IsTrue(quantizer.TryPlan(0.35, out var delay));
            Assert.AreEqual(0.45, delay, Precision);
        }

        [Test]
        public void A_note_played_at_once_does_not_claim_a_slot()
        {
            var quantizer = Build(QuantizeGrid.Eighth, 0.2f);
            quantizer.TryPlan(0.1, out var first);
            quantizer.TryPlan(0.1, out var second);
            Assert.AreEqual(0.0, first);
            Assert.AreEqual(0.0, second);
        }
    }
}
