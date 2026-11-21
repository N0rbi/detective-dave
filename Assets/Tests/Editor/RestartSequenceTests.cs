using System.Collections.Generic;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class RestartSequenceTests
    {
        static readonly Vector2 StartWorld = new Vector2(1f, 2f);
        static readonly Vector2 PlayerWorld = new Vector2(4f, 3f);

        FakeWipe wipe;
        FakeZoom zoom;
        List<string> events;
        RestartSequence restart;

        [SetUp]
        public void SetUp()
        {
            var board = new FakeBoard { KeyWorldSize = 2f };
            board.Place(Key.LeftShift, StartWorld);
            board.Place(Key.RightShift, new Vector2(5f, 2f));
            wipe = new FakeWipe();
            zoom = new FakeZoom();
            events = new List<string>();
            var intro = new IntroSequence(board, new FakeLevel(), wipe, zoom, new ZoomFocus(board, 1.5f, 0.5f));
            restart = new RestartSequence(wipe, intro, () => events.Add("reset"));
        }

        [Test]
        public void BeginSpiralsOutFromThePlayer()
        {
            restart.Begin(PlayerWorld);
            Assert.AreEqual(new[] { "Out " + PlayerWorld }, wipe.Calls);
            Assert.IsTrue(restart.IsRunning);
            Assert.IsEmpty(events);
        }

        [Test]
        public void TheWorldResetsOnlyOnceTheScreenIsCovered()
        {
            restart.Begin(PlayerWorld);
            wipe.Finish();
            CollectionAssert.AreEqual(new[] { "reset" }, events);
            Assert.AreEqual("In " + StartWorld, wipe.Calls[1]);
        }

        [Test]
        public void TheRestartEndsWhenTheSpiralInAndTheZoomOutAreDone()
        {
            restart.Begin(PlayerWorld);
            wipe.Finish();
            wipe.Finish();
            Assert.IsTrue(restart.IsRunning);
            zoom.FinishZoomOut();
            Assert.IsFalse(restart.IsRunning);
        }

        [Test]
        public void BeginWhileRunningDoesNothing()
        {
            restart.Begin(PlayerWorld);
            restart.Begin(PlayerWorld);
            Assert.AreEqual(1, wipe.Calls.Count);
        }

        [Test]
        public void CancelStopsTheSequenceAndIgnoresTheLateWipe()
        {
            restart.Begin(PlayerWorld);
            restart.Cancel();
            wipe.Finish();
            Assert.IsFalse(restart.IsRunning);
            Assert.IsEmpty(events);
            Assert.AreEqual(1, wipe.CancelCount);
        }

        [Test]
        public void CancelWhileNotRunningLeavesTheScreenAlone()
        {
            restart.Cancel();
            Assert.AreEqual(0, wipe.CancelCount);
            Assert.AreEqual(0, zoom.RestoreCount);
        }

        [Test]
        public void ARestartCanRunAgainAfterItFinished()
        {
            restart.Begin(PlayerWorld);
            wipe.Finish();
            wipe.Finish();
            zoom.FinishZoomOut();
            restart.Begin(PlayerWorld);
            Assert.IsTrue(restart.IsRunning);
            Assert.AreEqual("Out " + PlayerWorld, wipe.Calls[2]);
        }
    }
}
