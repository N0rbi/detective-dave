using DetectiveDave.KeyboardGrid;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public sealed class CapsLockLitTimerTests
    {
        const float MaxLit = 0.6f;

        float now;
        CapsLockLitTimer timer;

        [SetUp]
        public void BuildTimer()
        {
            now = 10f;
            timer = new CapsLockLitTimer(() => now, MaxLit);
        }

        [Test]
        public void NotLitBeforeAnyPress() => Assert.IsFalse(timer.IsLit);

        [Test]
        public void LitRightAfterThePress()
        {
            timer.OnPressed();
            Assert.IsTrue(timer.IsLit);
        }

        [Test]
        public void LitWhileWithinTheLimit()
        {
            timer.OnPressed();
            now += MaxLit - 0.01f;
            Assert.IsTrue(timer.IsLit);
        }

        [Test]
        public void UnlitOnceTheLimitIsReachedEvenWhileHeld()
        {
            timer.OnPressed();
            timer.EndFrame();
            now += MaxLit;
            Assert.IsFalse(timer.IsLit);
        }

        [Test]
        public void StaysUnlitForTheRestOfTheHold()
        {
            timer.OnPressed();
            timer.EndFrame();
            now += MaxLit + 5f;
            Assert.IsFalse(timer.IsLit);
        }

        [Test]
        public void ANewPressRestartsTheTimer()
        {
            timer.OnPressed();
            timer.EndFrame();
            now += MaxLit + 1f;
            timer.OnReleased();
            timer.OnPressed();
            timer.EndFrame();
            now += MaxLit - 0.01f;
            Assert.IsTrue(timer.IsLit);
            now += 0.02f;
            Assert.IsFalse(timer.IsLit);
        }

        [Test]
        public void ReleaseUnlightsAtOnce()
        {
            timer.OnPressed();
            timer.OnReleased();
            Assert.IsFalse(timer.IsLit);
        }

        [Test]
        public void ALimitOfZeroLightsOnlyThePressFrame()
        {
            timer = new CapsLockLitTimer(() => now, 0f);
            timer.OnPressed();
            Assert.IsTrue(timer.IsLit);
            timer.EndFrame();
            Assert.IsFalse(timer.IsLit);
        }

        [Test]
        public void ALimitOfZeroLightsAgainOnANewPress()
        {
            timer = new CapsLockLitTimer(() => now, 0f);
            timer.OnPressed();
            timer.EndFrame();
            timer.OnReleased();
            timer.OnPressed();
            Assert.IsTrue(timer.IsLit);
        }

        [Test]
        public void AReleaseBeforeTheFrameEndsLightsNothing()
        {
            timer = new CapsLockLitTimer(() => now, 0f);
            timer.OnPressed();
            timer.OnReleased();
            timer.EndFrame();
            Assert.IsFalse(timer.IsLit);
        }
    }
}
