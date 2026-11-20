using System;
using System.Collections.Generic;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class PlayerGameTests
    {
        const Key Start = Key.LeftShift;
        const Key Next = Key.S;
        const Key Goal = Key.RightShift;
        const Key LeftOfStart = Key.Q;
        const Key Far = Key.L;
        const Key Spare = Key.D;
        const Key Unplaced = Key.W;
        const float KeyWorldSize = 2f;
        const float SlipDistanceInKeys = 0.3f;
        const float DropMarginInKeys = 1f;
        const float BumpDistanceInKeys = 0.35f;

        static readonly Vector2 StartWorld = new Vector2(0f, 0f);
        static readonly Vector2 NextWorld = new Vector2(1f, 0f);
        static readonly Vector2 GoalWorld = new Vector2(2f, 0f);
        static readonly Vector2 SpareWorld = new Vector2(1f, 1f);
        static readonly Vector2 LeftWorld = new Vector2(-1f, 0f);

        FakeBoard board;
        FakeLevel level;
        FakeAnimations animations;
        FakePresentation presentation;
        PlayerGame game;
        List<Key> moved;
        int failed;
        int goalReached;

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard { KeyWorldSize = KeyWorldSize };
            board.Place(Start, StartWorld);
            board.Place(Next, NextWorld);
            board.Place(Goal, GoalWorld);
            board.Place(LeftOfStart, LeftWorld);
            board.Place(Spare, SpareWorld);
            board.Place(Far, new Vector2(9f, 9f));
            board.Connect(Start, Next);
            board.Connect(Next, Goal);
            board.Connect(Next, Spare);
            board.Connect(Spare, Start);
            board.Connect(Start, LeftOfStart);
            board.Connect(Start, Unplaced);
            level = new FakeLevel();
            animations = new FakeAnimations();
            presentation = new FakePresentation();
            game = new PlayerGame(board, level, animations, presentation, new PlayerGameSettings(3, 3, SlipDistanceInKeys, DropMarginInKeys, BumpDistanceInKeys));
            moved = new List<Key>();
            failed = 0;
            goalReached = 0;
            game.Moved += moved.Add;
            game.Failed += () => failed++;
            game.GoalReached += () => goalReached++;
        }

        [Test]
        public void KeyPressesDoNothingBeforeRestart()
        {
            game.OnKeyPressed(Start);
            game.OnKeyPressed(Next);
            Assert.AreEqual(PlayerState.NotStarted, game.State);
            Assert.IsEmpty(animations.Calls);
        }

        [Test]
        public void MissingEndsHideThePlayerAndIgnoreTheStartKey()
        {
            level.EndsAvailable = false;
            game.Restart();
            int calls = animations.Calls.Count;
            game.OnKeyPressed(Start);
            game.OnKeyPressed(Next);
            Assert.IsTrue(game.EndsMissing);
            Assert.IsFalse(game.IsActive);
            Assert.IsFalse(presentation.Visible);
            Assert.AreEqual(calls, animations.Calls.Count);
            Assert.AreEqual(PlayerState.NotStarted, game.State);
        }

        [Test]
        public void NoLevelHidesThePlayerSilently()
        {
            level.HasLevel = false;
            game.Restart();
            Assert.IsFalse(game.EndsMissing);
            Assert.IsFalse(game.IsActive);
            Assert.IsFalse(presentation.Visible);
        }

        [Test]
        public void GoalKeyWithoutPositionCountsAsEndsMissing()
        {
            level.Goal = Unplaced;
            game.Restart();
            Assert.IsTrue(game.EndsMissing);
            Assert.IsFalse(game.IsActive);
            Assert.IsFalse(presentation.Visible);
        }

        [Test]
        public void RestartShowsThePlayerAtTheStart()
        {
            game.Restart();
            Assert.IsFalse(game.EndsMissing);
            Assert.IsTrue(game.IsActive);
            Assert.IsTrue(presentation.Visible);
            Assert.AreEqual(StartWorld, animations.LastShownAt);
        }

        [Test]
        public void KeyOtherThanStartDoesNotBeginTheGame()
        {
            game.Restart();
            game.OnKeyPressed(Next);
            Assert.AreEqual(PlayerState.NotStarted, game.State);
            Assert.AreEqual(0, CountCalls(nameof(FakeAnimations.MoveTo)));
        }

        [Test]
        public void StartKeyBeginsTheGame()
        {
            Begin();
            Assert.AreEqual(PlayerState.Idle, game.State);
            Assert.AreEqual(0, CountCalls(nameof(FakeAnimations.MoveTo)));
        }

        [Test]
        public void StepMovesThenIdlesAfterTheAnimation()
        {
            Begin();
            game.OnKeyPressed(Next);
            Assert.AreEqual(PlayerState.Moving, game.State);
            Assert.AreEqual(NextWorld, animations.LastMoveTarget);
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Idle, game.State);
            CollectionAssert.AreEqual(new[] { Next }, moved);
        }

        [Test]
        public void StepAdvancesTheWalkFrame()
        {
            Begin();
            game.OnKeyPressed(Next);
            Assert.AreEqual(1, presentation.Frame);
        }

        [Test]
        public void InputIsIgnoredWhileMoving()
        {
            Begin();
            game.OnKeyPressed(Next);
            AssertPressesAreIgnored();
        }

        [Test]
        public void InputIsIgnoredWhileSlipping()
        {
            Begin();
            game.OnKeyPressed(Far);
            Assert.AreEqual(PlayerState.Slipping, game.State);
            AssertPressesAreIgnored();
        }

        [Test]
        public void InputIsIgnoredWhileBumpingWall()
        {
            level.Kinds[Next] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            Assert.AreEqual(PlayerState.BumpingWall, game.State);
            AssertPressesAreIgnored();
        }

        [Test]
        public void InputIsIgnoredWhileFallingInGap()
        {
            level.Kinds[Next] = CellKind.Gap;
            Begin();
            game.OnKeyPressed(Next);
            Assert.AreEqual(PlayerState.FallingInGap, game.State);
            AssertPressesAreIgnored();
        }

        [Test]
        public void InputIsIgnoredWhileRespawning()
        {
            Begin();
            game.OnKeyPressed(Far);
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Respawning, game.State);
            AssertPressesAreIgnored();
        }

        [Test]
        public void WallBumpLeansTowardsTheWallThenReturnsToIdle()
        {
            level.Kinds[Next] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            Assert.AreEqual(Vector2.right, animations.LastSquashDirection);
            Assert.AreEqual(StartWorld, animations.LastBumpHome);
            Assert.AreEqual(StartWorld + Vector2.right * BumpDistanceInKeys * KeyWorldSize, animations.LastBumpTarget);
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Idle, game.State);
        }

        [Test]
        public void WallBumpDoesNotRespawnOrFail()
        {
            level.Kinds[Next] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            Assert.AreEqual(0, failed);
            Assert.AreEqual(0, CountCalls(nameof(FakeAnimations.DropFromTop)));
            Assert.AreEqual(0, moved.Count);
        }

        [Test]
        public void WallBumpKeepsThePlayerOnTheCurrentKey()
        {
            level.Kinds[Spare] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Spare);
            Assert.AreEqual(NextWorld, animations.LastBumpHome);
            animations.CompleteLast();
            game.OnKeyPressed(Goal);
            Assert.AreEqual(GoalWorld, animations.LastMoveTarget);
        }

        [Test]
        public void WallBumpResetsTheWalkFrames()
        {
            level.Kinds[Spare] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            Assert.AreEqual(1, presentation.Frame);
            game.OnKeyPressed(Spare);
            Assert.AreEqual(0, presentation.Frame);
            animations.CompleteLast();
            game.OnKeyPressed(Start);
            Assert.AreEqual(1, presentation.Frame);
        }

        [Test]
        public void WallBumpOnAGutterKeepsTheClimbPoseAtFrameZero()
        {
            level.Kinds[Next] = CellKind.Gutter;
            level.Kinds[Spare] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Spare);
            Assert.IsTrue(presentation.Climbing);
            Assert.AreEqual(0, presentation.Frame);
        }

        [Test]
        public void MovesWorkAgainAfterAWallBump()
        {
            level.Kinds[Spare] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Spare);
            animations.CompleteLast();
            game.OnKeyPressed(Next);
            Assert.AreEqual(PlayerState.Moving, game.State);
        }

        [Test]
        public void BreathesWhileIdleAndNotWhileMoving()
        {
            game.Restart();
            Assert.IsTrue(animations.IsBreathing);
            game.OnKeyPressed(Start);
            Assert.IsTrue(animations.IsBreathing);
            game.OnKeyPressed(Next);
            Assert.IsFalse(animations.IsBreathing);
            animations.CompleteLast();
            Assert.IsTrue(animations.IsBreathing);
        }

        [Test]
        public void BreathingStopsDuringABumpAndResumesAfter()
        {
            level.Kinds[Next] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            Assert.IsFalse(animations.IsBreathing);
            animations.CompleteLast();
            Assert.IsTrue(animations.IsBreathing);
        }

        [Test]
        public void BreathingStopsAtTheGoalAndWhenStopped()
        {
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Goal);
            animations.CompleteLast();
            Assert.IsFalse(animations.IsBreathing);
            game.Restart();
            Assert.IsTrue(animations.IsBreathing);
            game.Stop();
            Assert.IsFalse(animations.IsBreathing);
        }

        [Test]
        public void SlipEndsInRespawningThenIdle()
        {
            Begin();
            game.OnKeyPressed(Far);
            animations.CompleteLast();
            AssertRespawning();
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Idle, game.State);
            Assert.AreEqual(1, failed);
        }

        [Test]
        public void GapFallWalksFadesThenRespawns()
        {
            level.Kinds[Next] = CellKind.Gap;
            Begin();
            game.OnKeyPressed(Next);
            Assert.AreEqual(NextWorld, animations.LastMoveTarget);
            Assert.AreEqual(1, presentation.Frame);
            animations.CompleteLast();
            Assert.AreEqual(1, CountCalls(nameof(FakeAnimations.FadeIntoGap)));
            Assert.AreEqual(PlayerState.FallingInGap, game.State);
            animations.CompleteLast();
            AssertRespawning();
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Idle, game.State);
            Assert.AreEqual(1, failed);
        }

        [Test]
        public void RespawnResetsFrameFacingClimbingAndPosition()
        {
            level.Kinds[Next] = CellKind.Gutter;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            Assert.IsTrue(presentation.Climbing);
            game.OnKeyPressed(Far);
            animations.CompleteLast();
            Assert.AreEqual(0, presentation.Frame);
            Assert.AreEqual(Facing.Right, presentation.Facing);
            Assert.IsFalse(presentation.Climbing);
            animations.CompleteLast();
            game.OnKeyPressed(Next);
            Assert.AreEqual(PlayerState.Moving, game.State);
        }

        [Test]
        public void RespawnRestoresTheDefaultFacing()
        {
            Begin();
            game.OnKeyPressed(LeftOfStart);
            animations.CompleteLast();
            Assert.AreEqual(Facing.Left, presentation.Facing);
            game.OnKeyPressed(Far);
            animations.CompleteLast();
            Assert.AreEqual(Facing.Right, presentation.Facing);
        }

        [Test]
        public void SlipOffsetPointsBackwardsFromFacingRight()
        {
            Begin();
            game.OnKeyPressed(Far);
            Assert.AreEqual(new Vector2(-SlipDistanceInKeys * KeyWorldSize, 0f), animations.LastSlipOffset);
        }

        [Test]
        public void SlipOffsetPointsBackwardsFromFacingLeft()
        {
            level.Goal = LeftOfStart;
            Begin();
            game.OnKeyPressed(Far);
            Assert.AreEqual(new Vector2(SlipDistanceInKeys * KeyWorldSize, 0f), animations.LastSlipOffset);
        }

        [Test]
        public void SlipOffsetScalesWithKeyWorldSize()
        {
            board.KeyWorldSize = 5f;
            Begin();
            game.OnKeyPressed(Far);
            Assert.AreEqual(-SlipDistanceInKeys * 5f, animations.LastSlipOffset.x, 0.0001f);
        }

        [Test]
        public void GutterArrivalShowsTheClimbPoseAndLeavingItShowsTheWalkPoseOnArrival()
        {
            level.Kinds[Next] = CellKind.Gutter;
            Begin();
            game.OnKeyPressed(Next);
            Assert.IsTrue(presentation.Climbing);
            animations.CompleteLast();
            Assert.IsTrue(presentation.Climbing);
            Assert.AreEqual(PlayerState.Idle, game.State);
            game.OnKeyPressed(Goal);
            Assert.IsTrue(presentation.Climbing);
            animations.CompleteLast();
            Assert.IsFalse(presentation.Climbing);
        }

        [Test]
        public void ClimbFramesFollowTheBackAndForthSequenceOnGutterMoves()
        {
            level.Kinds[Next] = CellKind.Gutter;
            Begin();
            var path = new[] { Next, Start, Next, Spare };
            var expected = new[] { 1, 2, 1, 0 };
            for (int i = 0; i < path.Length; i++)
            {
                game.OnKeyPressed(path[i]);
                Assert.IsTrue(presentation.Climbing, $"after step {i + 1}");
                Assert.AreEqual(expected[i], presentation.Frame, $"after step {i + 1}");
                animations.CompleteLast();
            }
        }

        [Test]
        public void WalkFramesResumeWhereTheyWereAfterLeavingTheGutter()
        {
            level.Kinds[Next] = CellKind.Gutter;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Start);
            animations.CompleteLast();
            Assert.IsFalse(presentation.Climbing);
            Assert.AreEqual(0, presentation.Frame);
            game.OnKeyPressed(Spare);
            Assert.IsFalse(presentation.Climbing);
            Assert.AreEqual(1, presentation.Frame);
        }

        [Test]
        public void ClimbFramesRestartFromZeroAfterAFailure()
        {
            level.Kinds[Next] = CellKind.Gutter;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Start);
            animations.CompleteLast();
            game.OnKeyPressed(Far);
            animations.CompleteLast();
            animations.CompleteLast();
            game.OnKeyPressed(Next);
            Assert.IsTrue(presentation.Climbing);
            Assert.AreEqual(1, presentation.Frame);
        }

        [Test]
        public void GoalIsTerminal()
        {
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Goal);
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Goal, game.State);
            Assert.AreEqual(1, goalReached);
            int calls = animations.Calls.Count;
            game.OnKeyPressed(Next);
            game.OnKeyPressed(Start);
            animations.CompleteAll();
            Assert.AreEqual(calls, animations.Calls.Count);
            Assert.AreEqual(PlayerState.Goal, game.State);
            Assert.AreEqual(1, goalReached);
        }

        [Test]
        public void RestartDuringAnimationIgnoresTheStaleCallback()
        {
            animations.KeepsPendingOnCancel = true;
            level.Kinds[Next] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            level.RaiseLevelChanged();
            game.Restart();
            animations.CompleteAll();
            Assert.AreEqual(0, failed);
            Assert.AreEqual(PlayerState.NotStarted, game.State);
            Assert.AreEqual(0, CountCalls(nameof(FakeAnimations.DropFromTop)));
        }

        [Test]
        public void RestartCancelsRunningAnimations()
        {
            Begin();
            game.OnKeyPressed(Next);
            int cancels = CountCalls(nameof(FakeAnimations.CancelAllWithoutCallbacks));
            game.Restart();
            Assert.AreEqual(cancels + 1, CountCalls(nameof(FakeAnimations.CancelAllWithoutCallbacks)));
        }

        [Test]
        public void StopCancelsAndIgnoresStaleCallbacks()
        {
            animations.KeepsPendingOnCancel = true;
            level.Kinds[Next] = CellKind.Wall;
            Begin();
            game.OnKeyPressed(Next);
            game.Stop();
            animations.CompleteAll();
            Assert.IsFalse(game.IsActive);
            Assert.AreEqual(0, failed);
            Assert.AreEqual(0, CountCalls(nameof(FakeAnimations.DropFromTop)));
            int calls = animations.Calls.Count;
            game.OnKeyPressed(Next);
            Assert.AreEqual(calls, animations.Calls.Count);
        }

        [Test]
        public void DefaultFacingIsTowardsTheGoal()
        {
            level.Goal = LeftOfStart;
            game.Restart();
            Assert.AreEqual(Facing.Left, presentation.Facing);
        }

        [Test]
        public void KeyWithoutPositionThrows()
        {
            Begin();
            Assert.Throws<InvalidOperationException>(() => game.OnKeyPressed(Unplaced));
        }

        [Test]
        public void WalkFramesFollowTheBackAndForthSequence()
        {
            Begin();
            var path = new[] { Next, Spare, Next, Start, Next };
            var expected = new[] { 1, 2, 1, 0, 1 };
            for (int i = 0; i < path.Length; i++)
            {
                game.OnKeyPressed(path[i]);
                Assert.AreEqual(expected[i], presentation.Frame, $"after step {i + 1}");
                animations.CompleteLast();
            }
        }

        [Test]
        public void FrameRestartsFromZeroAfterAFailureFollowingSeveralSteps()
        {
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Spare);
            animations.CompleteLast();
            Assert.AreEqual(2, presentation.Frame);
            game.OnKeyPressed(Far);
            animations.CompleteLast();
            Assert.AreEqual(0, presentation.Frame);
            animations.CompleteLast();
            game.OnKeyPressed(Next);
            Assert.AreEqual(1, presentation.Frame);
        }

        [Test]
        public void BumpingAWallBehindTurnsTowardsTheWall()
        {
            level.Kinds[LeftOfStart] = CellKind.Wall;
            Begin();
            Assert.AreEqual(Facing.Right, presentation.Facing);
            game.OnKeyPressed(LeftOfStart);
            Assert.AreEqual(Facing.Left, presentation.Facing);
            Assert.AreEqual(Vector2.left, animations.LastSquashDirection);
        }

        [Test]
        public void PlayerIsShownAtTheStartShiftBeforeBegin()
        {
            game.Restart();
            Assert.IsTrue(presentation.Visible);
            Assert.AreEqual(PlayerState.NotStarted, game.State);
            Assert.AreEqual(StartWorld, animations.LastShownAt);
        }

        [Test]
        public void PressingTheEndShiftBeforeStartIsANoOp()
        {
            game.Restart();
            game.OnKeyPressed(Goal);
            Assert.AreEqual(PlayerState.NotStarted, game.State);
            Assert.AreEqual(0, CountCalls(nameof(FakeAnimations.MoveTo)));
        }

        [Test]
        public void ReloadReturnsToNotStarted()
        {
            Begin();
            level.RaiseLevelChanged();
            game.Restart();
            Assert.AreEqual(PlayerState.NotStarted, game.State);
            Assert.IsTrue(presentation.Visible);
        }

        [Test]
        public void GapPaintedOnGoalShiftStillReachesGoal()
        {
            level.Kinds[Goal] = CellKind.Gap;
            Begin();
            game.OnKeyPressed(Next);
            animations.CompleteLast();
            game.OnKeyPressed(Goal);
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Goal, game.State);
            Assert.AreEqual(1, goalReached);
        }

        void Begin()
        {
            game.Restart();
            game.OnKeyPressed(Start);
        }

        void AssertRespawning()
        {
            Assert.AreEqual(PlayerState.Respawning, game.State);
            Assert.AreEqual(1, CountCalls(nameof(FakeAnimations.DropFromTop)));
            Assert.AreEqual(StartWorld, animations.LastDropTarget);
            Assert.AreEqual(DropMarginInKeys * KeyWorldSize, animations.LastDropMargin, 0.0001f);
        }

        void AssertPressesAreIgnored()
        {
            var state = game.State;
            int calls = animations.Calls.Count;
            game.OnKeyPressed(Next);
            game.OnKeyPressed(Goal);
            game.OnKeyPressed(Far);
            Assert.AreEqual(calls, animations.Calls.Count);
            Assert.AreEqual(state, game.State);
        }

        int CountCalls(string name) => animations.Calls.FindAll(call => call == name).Count;
    }
}
