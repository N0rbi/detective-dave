using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class LightFadeTests
    {
        [Test]
        public void Fading_in_goes_from_zero_to_the_configured_full_intensity()
        {
            var fade = new LightFade(1.5f, 0.8f, 0.6f);
            Assert.AreEqual(0f, fade.FadeInFrom);
            Assert.AreEqual(1.5f, fade.FadeInTo);
        }

        [Test]
        public void Fading_out_goes_down_to_zero()
        {
            var fade = new LightFade(1f, 0.8f, 0.6f);
            Assert.AreEqual(0f, fade.FadeOutTo);
            Assert.AreEqual(0.6f, fade.FadeOutSeconds);
        }
    }

    public class CompletionLatchTests
    {
        [Test]
        public void Fires_only_after_every_completion()
        {
            var fired = 0;
            var latch = new CompletionLatch(2, () => fired++);
            latch.Complete();
            Assert.AreEqual(0, fired);
            Assert.IsFalse(latch.IsOpen);
            latch.Complete();
            Assert.AreEqual(1, fired);
            Assert.IsTrue(latch.IsOpen);
        }

        [Test]
        public void Fires_once_even_when_completed_too_often()
        {
            var fired = 0;
            var latch = new CompletionLatch(1, () => fired++);
            latch.Complete();
            latch.Complete();
            Assert.AreEqual(1, fired);
        }

        [Test]
        public void Waiting_for_nothing_is_refused()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new CompletionLatch(0, () => { }));
        }
    }

    public class DetectiveDropTests
    {
        [Test]
        public void Dropping_targets_the_rest_height_minus_the_distance()
        {
            var drop = new DetectiveDrop(-12.06f, 3f);
            Assert.AreEqual(-15.06f, drop.BeginDrop(), 0.0001f);
            Assert.AreEqual(DropPhase.Dropping, drop.Phase);
        }

        [Test]
        public void After_the_drop_he_stays_down_until_the_close_cue()
        {
            var drop = new DetectiveDrop(2f, 3f);
            drop.BeginDrop();
            drop.FinishDrop();
            Assert.AreEqual(DropPhase.Down, drop.Phase);
            Assert.IsTrue(drop.TryStartReturn(out var target));
            Assert.AreEqual(2f, target);
            Assert.AreEqual(DropPhase.Returning, drop.Phase);
            drop.FinishReturn();
            Assert.AreEqual(DropPhase.AtRest, drop.Phase);
        }

        [Test]
        public void Finishing_the_drop_does_not_start_the_return()
        {
            var drop = new DetectiveDrop(2f, 3f);
            drop.BeginDrop();
            drop.FinishDrop();
            Assert.AreEqual(DropPhase.Down, drop.Phase);
        }

        [Test]
        public void Opening_again_while_down_stays_at_the_same_low_height()
        {
            var drop = new DetectiveDrop(2f, 3f);
            var first = drop.BeginDrop();
            drop.FinishDrop();
            Assert.AreEqual(first, drop.BeginDrop());
        }

        [Test]
        public void Closing_early_during_the_drop_returns_to_rest()
        {
            var drop = new DetectiveDrop(2f, 3f);
            drop.BeginDrop();
            Assert.IsTrue(drop.TryStartReturn(out var target));
            Assert.AreEqual(drop.RestY, target);
        }

        [Test]
        public void Closing_while_at_rest_or_already_returning_starts_nothing()
        {
            var drop = new DetectiveDrop(2f, 3f);
            Assert.IsFalse(drop.TryStartReturn(out _));
            drop.BeginDrop();
            drop.TryStartReturn(out _);
            Assert.IsFalse(drop.TryStartReturn(out _));
        }

        [Test]
        public void Opening_again_mid_return_drops_to_the_same_low_height()
        {
            var drop = new DetectiveDrop(2f, 3f);
            var first = drop.BeginDrop();
            drop.TryStartReturn(out _);
            Assert.AreEqual(first, drop.BeginDrop());
            Assert.AreEqual(-1f, drop.LowY);
        }

        [Test]
        public void Resetting_puts_the_phase_back_at_rest()
        {
            var drop = new DetectiveDrop(2f, 3f);
            drop.BeginDrop();
            drop.ResetToRest();
            Assert.AreEqual(DropPhase.AtRest, drop.Phase);
        }

        [Test]
        public void A_negative_distance_is_refused()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new DetectiveDrop(0f, -1f));
        }
    }
}
