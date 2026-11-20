using System;
using System.Collections.Generic;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class GoalSequenceTests
    {
        const float KeyWorldSize = 2f;
        const float HandRiseInKeys = 1.5f;
        const float HandShare = 0.5f;

        static readonly Vector2 StartWorld = new Vector2(0f, 0f);
        static readonly Vector2 GoalWorld = new Vector2(4f, 1f);
        static readonly Vector2 FocusOnGoal = GoalWorld + Vector2.up * (HandRiseInKeys * KeyWorldSize * HandShare);
        static readonly Vector2 FocusOnStart = StartWorld + Vector2.up * (HandRiseInKeys * KeyWorldSize * HandShare);

        FakeBoard board;
        FakeLevel level;
        FakeDecorations decorations;
        FakeEvidence evidence;
        FakeWipe wipe;
        FakeZoom zoom;
        FakeAdvance advance;
        GoalSequence sequence;

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard { KeyWorldSize = KeyWorldSize };
            board.Place(Key.LeftShift, StartWorld);
            board.Place(Key.RightShift, GoalWorld);
            level = new FakeLevel();
            decorations = new FakeDecorations();
            evidence = new FakeEvidence();
            wipe = new FakeWipe();
            zoom = new FakeZoom();
            advance = new FakeAdvance();
            var director = new EndDecorationsDirector(board, level, decorations, 0.5f);
            var focus = new ZoomFocus(board, HandRiseInKeys, HandShare);
            var intro = new IntroSequence(board, level, wipe, zoom, focus);
            sequence = new GoalSequence(board, level, director, evidence, zoom, focus, wipe, advance, intro, HandRiseInKeys);
        }

        [Test]
        public void BeginShowsTheBurstAndHoldsTheEvidenceAboveTheGoal()
        {
            sequence.Begin();
            Assert.IsTrue(sequence.IsRunning);
            Assert.IsNotNull(decorations.Burst);
            Assert.AreEqual(GoalWorld + Vector2.up * (HandRiseInKeys * KeyWorldSize), evidence.Hand);
        }

        [Test]
        public void TheCameraZoomsOnThePlayerHoldingTheEvidenceOnlyAfterTheEvidencePoppedUp()
        {
            sequence.Begin();
            Assert.AreEqual(0, zoom.Calls.Count);
            evidence.Finish();
            Assert.AreEqual(new[] { "ZoomIn " + FocusOnGoal }, zoom.Calls);
            Assert.AreEqual(1, evidence.EmphasiseCount);
        }

        [Test]
        public void TheScreenWipesOutFromTheGoalOnlyAfterTheCameraZoomed()
        {
            sequence.Begin();
            evidence.Finish();
            Assert.AreEqual(0, wipe.Calls.Count);
            zoom.FinishZoomIn();
            Assert.AreEqual(new[] { "Out " + GoalWorld }, wipe.Calls);
        }

        [Test]
        public void TheNextLevelLoadsOnlyAfterTheWipeCoveredTheScreen()
        {
            sequence.Begin();
            evidence.Finish();
            zoom.FinishZoomIn();
            Assert.AreEqual(0, advance.Count);
            wipe.Finish();
            Assert.AreEqual(1, advance.Count);
        }

        [Test]
        public void TheEvidenceIsHiddenBeforeTheNextLevelLoads()
        {
            sequence.Begin();
            evidence.Finish();
            zoom.FinishZoomIn();
            wipe.Finish();
            Assert.IsTrue(evidence.HiddenBeforeAdvance);
        }

        [Test]
        public void TheCameraStartsZoomedOnTheNewStartAndZoomsBackOutWhileTheWipeComesIn()
        {
            sequence.Begin();
            evidence.Finish();
            zoom.FinishZoomIn();
            wipe.Finish();
            CollectionAssert.AreEqual(new[] { "ZoomIn " + FocusOnGoal, "Snap " + FocusOnStart, "ZoomOut" }, zoom.Calls);
        }

        [Test]
        public void TheWipeComesBackInFromTheStartOfTheNextLevel()
        {
            sequence.Begin();
            evidence.Finish();
            zoom.FinishZoomIn();
            wipe.Finish();
            Assert.AreEqual(new[] { "Out " + GoalWorld, "In " + StartWorld }, wipe.Calls);
        }

        [Test]
        public void TheSequenceIsRunningUntilTheWipeIsBackIn()
        {
            sequence.Begin();
            evidence.Finish();
            zoom.FinishZoomIn();
            wipe.Finish();
            Assert.IsTrue(sequence.IsRunning);
            wipe.Finish();
            Assert.IsTrue(sequence.IsRunning);
            zoom.FinishZoomOut();
            Assert.IsFalse(sequence.IsRunning);
        }

        [Test]
        public void BeginDoesNothingWhileRunning()
        {
            sequence.Begin();
            sequence.Begin();
            Assert.AreEqual(1, evidence.PresentCount);
        }

        [Test]
        public void BeginDoesNothingWhenTheEndsAreMissing()
        {
            level.EndsAvailable = false;
            sequence.Begin();
            Assert.IsFalse(sequence.IsRunning);
            Assert.AreEqual(0, evidence.PresentCount);
        }

        [Test]
        public void ALevelChangeDuringTheAdvanceDoesNotCancelTheSequence()
        {
            advance.OnAdvance = sequence.HandleLevelChanged;
            sequence.Begin();
            evidence.Finish();
            zoom.FinishZoomIn();
            wipe.Finish();
            Assert.AreEqual(2, wipe.Calls.Count);
            Assert.AreEqual(0, wipe.CancelCount);
            Assert.AreEqual(0, zoom.RestoreCount);
        }

        [Test]
        public void ALevelChangeFromOutsideCancelsTheSequence()
        {
            sequence.Begin();
            sequence.HandleLevelChanged();
            Assert.IsFalse(sequence.IsRunning);
            Assert.AreEqual(1, wipe.CancelCount);
            Assert.AreEqual(1, zoom.RestoreCount);
            Assert.IsTrue(evidence.Hidden);
        }

        [Test]
        public void CallbacksAfterACancelDoNothing()
        {
            sequence.Begin();
            sequence.Cancel();
            evidence.Finish();
            Assert.AreEqual(0, zoom.Calls.Count);
        }

        [Test]
        public void AWipeCallbackAfterACancelDoesNotAdvanceTheLevel()
        {
            sequence.Begin();
            evidence.Finish();
            zoom.FinishZoomIn();
            sequence.Cancel();
            wipe.Finish();
            Assert.AreEqual(0, advance.Count);
        }

        [Test]
        public void ASequenceCanRunAgainAfterACancel()
        {
            sequence.Begin();
            sequence.Cancel();
            sequence.Begin();
            Assert.IsTrue(sequence.IsRunning);
            Assert.AreEqual(2, evidence.PresentCount);
        }

        sealed class FakeDecorations : IEndDecorations
        {
            public Vector2? Burst { get; private set; }

            public void ShowStart(Vector2 world) { }

            public void ShowGoal(Vector2 world) { }

            public void ShowBurst(Vector2 world) => Burst = world;

            public void HideAll() => Burst = null;
        }

        sealed class FakeEvidence : IEvidencePresenter
        {
            Action done;

            public Vector2 Hand { get; private set; }

            public int PresentCount { get; private set; }

            public int EmphasiseCount { get; private set; }

            public bool Hidden { get; private set; }

            public bool HiddenBeforeAdvance { get; private set; }

            public void Present(Vector2 hand, Action finished)
            {
                Hand = hand;
                PresentCount++;
                done = finished;
                Hidden = false;
            }

            public void Emphasise() => EmphasiseCount++;

            public void Hide()
            {
                Hidden = true;
                HiddenBeforeAdvance = true;
            }

            public void Finish() => done?.Invoke();
        }

        sealed class FakeAdvance : ILevelAdvance
        {
            public int Count { get; private set; }

            public Action OnAdvance { get; set; }

            public void Advance()
            {
                Count++;
                OnAdvance?.Invoke();
            }
        }
    }
}
