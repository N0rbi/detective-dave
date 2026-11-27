using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class ShiftEndsTests
    {
        [Test]
        public void LeftStartsOnLeftShiftAndEndsOnRightShift()
        {
            ShiftEnds.Of(StartSide.Left, out var start, out var goal);
            Assert.AreEqual(Key.LeftShift, start);
            Assert.AreEqual(Key.RightShift, goal);
        }

        [Test]
        public void RightStartsOnRightShiftAndEndsOnLeftShift()
        {
            ShiftEnds.Of(StartSide.Right, out var start, out var goal);
            Assert.AreEqual(Key.RightShift, start);
            Assert.AreEqual(Key.LeftShift, goal);
        }
    }
}
