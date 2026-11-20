using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class IntroSequenceTests
    {
        const float KeyWorldSize = 2f;
        const float HandRiseInKeys = 1.5f;
        const float HandShare = 0.5f;

        static readonly Vector2 StartWorld = new Vector2(1f, 2f);

        FakeBoard board;
        FakeLevel level;
        FakeWipe wipe;
        FakeZoom zoom;
        IntroSequence intro;

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard { KeyWorldSize = KeyWorldSize };
            board.Place(Key.LeftShift, StartWorld);
            board.Place(Key.RightShift, new Vector2(5f, 2f));
            level = new FakeLevel();
            wipe = new FakeWipe();
            zoom = new FakeZoom();
            intro = new IntroSequence(board, level, wipe, zoom, new ZoomFocus(board, HandRiseInKeys, HandShare));
        }

        [Test]
        public void BeginStartsCoveredAndZoomedOnThePlayerAtTheStart()
        {
            intro.Begin();
            var focus = StartWorld + Vector2.up * (HandRiseInKeys * KeyWorldSize * HandShare);
            Assert.AreEqual("Snap " + focus, zoom.Calls[0]);
        }

        [Test]
        public void BeginUnwindsTheSpiralFromThePlayerWhileTheCameraZoomsOut()
        {
            intro.Begin();
            Assert.AreEqual(new[] { "In " + StartWorld }, wipe.Calls);
            Assert.AreEqual("ZoomOut", zoom.Calls[1]);
            Assert.IsTrue(intro.IsRunning);
        }

        [Test]
        public void TheIntroIsDoneOnlyWhenBothTheWipeAndTheZoomAreDone()
        {
            bool done = false;
            intro.Begin(() => done = true);
            wipe.Finish();
            Assert.IsFalse(done);
            Assert.IsTrue(intro.IsRunning);
            zoom.FinishZoomOut();
            Assert.IsTrue(done);
            Assert.IsFalse(intro.IsRunning);
        }

        [Test]
        public void WithoutAStartNothingStaysCoveredAndTheIntroIsDoneAtOnce()
        {
            level.EndsAvailable = false;
            bool done = false;
            intro.Begin(() => done = true);
            Assert.IsTrue(done);
            Assert.IsFalse(intro.IsRunning);
            Assert.AreEqual(0, wipe.Calls.Count);
            Assert.AreEqual(1, wipe.CancelCount);
            Assert.AreEqual(1, zoom.RestoreCount);
        }

        [Test]
        public void CancelClearsTheWipeAndRestoresTheCamera()
        {
            intro.Begin();
            intro.Cancel();
            Assert.IsFalse(intro.IsRunning);
            Assert.AreEqual(1, wipe.CancelCount);
            Assert.AreEqual(1, zoom.RestoreCount);
        }

        [Test]
        public void CallbacksAfterACancelDoNothing()
        {
            bool done = false;
            intro.Begin(() => done = true);
            intro.Cancel();
            wipe.Finish();
            zoom.FinishZoomOut();
            Assert.IsFalse(done);
        }

        [Test]
        public void BeginAgainInvalidatesTheEarlierRun()
        {
            bool firstDone = false;
            intro.Begin(() => firstDone = true);
            intro.Begin();
            wipe.Finish();
            zoom.FinishZoomOut();
            Assert.IsFalse(firstDone);
        }
    }
}
