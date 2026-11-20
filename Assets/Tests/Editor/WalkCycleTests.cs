using System;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class WalkCycleTests
    {
        [Test]
        public void StartsOnIdleFrame() => Assert.AreEqual(0, new WalkCycle(3).Frame);

        [Test]
        public void YoyosOverThreeFrames()
        {
            var cycle = new WalkCycle(3);
            var seen = new int[7];
            for (int i = 0; i < seen.Length; i++)
            {
                cycle.Advance();
                seen[i] = cycle.Frame;
            }
            CollectionAssert.AreEqual(new[] { 1, 2, 1, 0, 1, 2, 1 }, seen);
        }

        [Test]
        public void ResetReturnsToIdleFrame()
        {
            var cycle = new WalkCycle(3);
            cycle.Advance();
            cycle.Advance();
            cycle.Reset();
            Assert.AreEqual(0, cycle.Frame);
            cycle.Advance();
            Assert.AreEqual(1, cycle.Frame);
        }

        [Test]
        public void FewerThanTwoFramesIsRejected() => Assert.Throws<ArgumentOutOfRangeException>(() => new WalkCycle(1));
    }
}
