using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class PendingMoveTests
    {
        float now;
        PendingMove pending;

        [SetUp]
        public void SetUp()
        {
            now = 1f;
            pending = new PendingMove(0.15f, () => now);
        }

        [Test]
        public void GivesBackAFreshKeyOnce()
        {
            pending.Hold(Key.S);
            now = 1.1f;
            Assert.IsTrue(pending.TryTake(out var key));
            Assert.AreEqual(Key.S, key);
            Assert.IsFalse(pending.TryTake(out _));
        }

        [Test]
        public void DropsAKeyThatIsTooOld()
        {
            pending.Hold(Key.S);
            now = 1.2f;
            Assert.IsFalse(pending.TryTake(out _));
        }

        [Test]
        public void KeepsOnlyTheLatestKey()
        {
            pending.Hold(Key.S);
            pending.Hold(Key.D);
            Assert.IsTrue(pending.TryTake(out var key));
            Assert.AreEqual(Key.D, key);
        }

        [Test]
        public void ClearForgetsTheKey()
        {
            pending.Hold(Key.S);
            pending.Clear();
            Assert.IsFalse(pending.TryTake(out _));
        }

        [Test]
        public void ZeroSecondsHoldsNothing()
        {
            var never = new PendingMove(0f, () => now);
            never.Hold(Key.S);
            Assert.IsFalse(never.TryTake(out _));
        }
    }
}
