using DetectiveDave.GuardPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class GuardFramesTests
    {
        [TestCase(0f, 0)]
        [TestCase(0.24f, 0)]
        [TestCase(0.26f, 1)]
        [TestCase(0.51f, 2)]
        [TestCase(0.76f, 1)]
        [TestCase(1f, 1)]
        public void ThreeFramesYoyoThroughOneStep(float progress, int expected)
        {
            Assert.AreEqual(expected, GuardFrames.At(progress, 3));
        }

        [Test]
        public void OneFrameAlwaysShowsThatFrame()
        {
            Assert.AreEqual(0, GuardFrames.At(0.7f, 1));
        }

        [Test]
        public void ProgressOutsideTheStepStaysInsideTheFrames()
        {
            Assert.AreEqual(0, GuardFrames.At(-1f, 3));
            Assert.AreEqual(1, GuardFrames.At(5f, 3));
        }
    }
}
