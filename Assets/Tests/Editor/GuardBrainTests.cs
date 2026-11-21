using DetectiveDave.GuardPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public sealed class GuardBrainTests
    {
        static readonly Key[] Row = { Key.A, Key.S, Key.D, Key.F, Key.G };

        FakeBoard board;
        FakeLevel level;
        GuardBrain brain;

        [SetUp]
        public void CreateRowAndGuard()
        {
            board = new FakeBoard();
            level = new FakeLevel();
            for (int i = 0; i < Row.Length - 1; i++) board.Connect(Row[i], Row[i + 1]);
            brain = CreateBrain(Row, 1f, 2f);
        }

        GuardBrain CreateBrain(Key[] route, float waitBefore, float waitAtEnd)
        {
            var pathfinder = new KeyPathfinder(board, key => level.KindOf(key) == CellKind.Floor);
            var settings = new GuardBrainSettings(waitBefore, waitAtEnd, 1f, 0.5f);
            return new GuardBrain(board, pathfinder, route, settings);
        }

        static void AssertStep(GuardAction action, Key target, float seconds)
        {
            Assert.That(action.Kind, Is.EqualTo(GuardActionKind.StepTo));
            Assert.That(action.Target, Is.EqualTo(target));
            Assert.That(action.Seconds, Is.EqualTo(seconds).Within(0.0001f));
        }

        GuardAction Finish(GuardAction step, Key playerKey)
        {
            return brain.Tick(step.Seconds, playerKey);
        }

        [Test]
        public void StartsOnItsFirstKey_AndWaitsBeforeMoving()
        {
            Assert.That(brain.Current, Is.EqualTo(Key.A));
            Assert.That(brain.Tick(0.5f, Key.Z).Kind, Is.EqualTo(GuardActionKind.Stay));
            AssertStep(brain.Tick(0.5f, Key.Z), Key.S, 1f);
        }

        [Test]
        public void KeepsStepping_AlongTheRoute()
        {
            AssertStep(brain.Tick(1f, Key.Z), Key.S, 1f);
            AssertStep(Finish(GuardAction.StepTo(Key.S, 1f, false), Key.Z), Key.D, 1f);
            Assert.That(brain.Current, Is.EqualTo(Key.S));
        }

        [Test]
        public void WaitsAtTheEnd_ThenWalksBack()
        {
            var action = brain.Tick(1f, Key.Z);
            for (int i = 0; i < 4; i++) action = Finish(action, Key.Z);
            Assert.That(brain.Current, Is.EqualTo(Key.G));
            Assert.That(action.Kind, Is.EqualTo(GuardActionKind.Stay));
            Assert.That(brain.Mode, Is.EqualTo(GuardMode.WaitingAtEnd));
            Assert.That(brain.Tick(1.9f, Key.Z).Kind, Is.EqualTo(GuardActionKind.Stay));
            AssertStep(brain.Tick(0.2f, Key.Z), Key.F, 1f);
        }

        [Test]
        public void WaitsAtTheStart_BeforeGoingAgain()
        {
            var action = brain.Tick(1f, Key.Z);
            for (int i = 0; i < 4; i++) action = Finish(action, Key.Z);
            action = brain.Tick(2f, Key.Z);
            for (int i = 0; i < 4; i++) action = Finish(action, Key.Z);
            Assert.That(brain.Current, Is.EqualTo(Key.A));
            Assert.That(brain.Mode, Is.EqualTo(GuardMode.WaitingToStart));
            AssertStep(brain.Tick(1f, Key.Z), Key.S, 1f);
        }

        [Test]
        public void PlayerOnNeighbouringKey_StartsAChaseAtChaseSpeed()
        {
            var action = brain.Tick(1f, Key.Z);
            action = Finish(action, Key.D);
            action = brain.Tick(0f, Key.D);
            Assert.That(brain.Mode, Is.EqualTo(GuardMode.Chasing));
            Assert.That(action.IsChasing, Is.True);
            AssertStep(action, Key.D, 0.5f);
        }

        [Test]
        public void PlayerTwoKeysAway_DoesNotStartAChase()
        {
            brain.Tick(1f, Key.D);
            Assert.That(brain.Mode, Is.EqualTo(GuardMode.Patrolling));
        }

        [Test]
        public void ChaseRepathsEveryStep_AsThePlayerMoves()
        {
            var action = brain.Tick(1f, Key.Z);
            action = Finish(action, Key.D);
            AssertStep(action, Key.D, 0.5f);
            action = brain.Tick(0.5f, Key.F);
            AssertStep(action, Key.F, 0.5f);
        }

        [Test]
        public void ArrivingOnThePlayersKey_CatchesThem()
        {
            var action = brain.Tick(1f, Key.Z);
            action = Finish(action, Key.D);
            action = brain.Tick(0.5f, Key.D);
            Assert.That(action.Kind, Is.EqualTo(GuardActionKind.Catch));
            Assert.That(brain.Mode, Is.EqualTo(GuardMode.Catching));
            Assert.That(brain.Tick(1f, Key.D).Kind, Is.EqualTo(GuardActionKind.Stay));
        }

        [Test]
        public void PlayerSteppingOntoTheGuardsKey_IsCaught()
        {
            Assert.That(brain.Tick(0.1f, Key.A).Kind, Is.EqualTo(GuardActionKind.Catch));
        }

        [Test]
        public void PlayerOnAGutterNextToTheGuard_DoesNotStartAChase()
        {
            level.Kinds[Key.S] = CellKind.Gutter;
            brain.Tick(0.1f, Key.S);
            Assert.That(brain.Mode, Is.Not.EqualTo(GuardMode.Chasing));
        }

        [Test]
        public void PlayerEscapingOntoAGutter_SendsTheGuardBackToPatrolling()
        {
            var action = brain.Tick(1f, Key.Z);
            action = Finish(action, Key.D);
            action = brain.Tick(0f, Key.D);
            level.Kinds[Key.F] = CellKind.Gutter;
            action = Finish(action, Key.F);
            Assert.That(brain.Mode, Is.EqualTo(GuardMode.Patrolling));
            Assert.That(action.IsChasing, Is.False);
            Assert.That(action.Kind, Is.EqualTo(GuardActionKind.StepTo));
        }

        [Test]
        public void LosingThePath_WalksBackToTheNearestRouteKey()
        {
            var shortRoute = new[] { Key.A, Key.S };
            board.Connect(Key.S, Key.Q);
            board.Connect(Key.Q, Key.W);
            var guard = CreateBrain(shortRoute, 0f, 0f);
            var action = guard.Tick(0f, Key.Z);
            action = guard.Tick(1f, Key.Z);
            Assert.That(guard.Current, Is.EqualTo(Key.S));
            board.Connect(Key.S, Key.W);
            action = guard.Tick(0f, Key.W);
            Assert.That(guard.Mode, Is.EqualTo(GuardMode.Chasing));
            level.Kinds[Key.W] = CellKind.Wall;
            action = guard.Tick(0.5f, Key.W);
            Assert.That(guard.Mode, Is.EqualTo(GuardMode.Patrolling));
            Assert.That(action.Kind, Is.Not.EqualTo(GuardActionKind.Catch));
        }

        [Test]
        public void RouteWithOneKey_StaysPut()
        {
            var guard = CreateBrain(new[] { Key.A }, 0f, 0f);
            for (int i = 0; i < 5; i++) Assert.That(guard.Tick(1f, Key.Z).Kind, Is.EqualTo(GuardActionKind.Stay));
            Assert.That(guard.Current, Is.EqualTo(Key.A));
        }

        [Test]
        public void EmptyRoute_IsRejected()
        {
            Assert.Throws<System.ArgumentException>(() => CreateBrain(new Key[0], 0f, 0f));
        }

        [Test]
        public void PassabilityRejectsEverythingButFloor_AndTheStartShift()
        {
            level.Kinds[Key.S] = CellKind.Gap;
            level.Kinds[Key.D] = CellKind.Gutter;
            level.Kinds[Key.F] = CellKind.Wall;
            var passability = new GuardPassability(level);
            Assert.That(passability.CanStandOn(Key.A), Is.True);
            Assert.That(passability.CanStandOn(Key.S), Is.False);
            Assert.That(passability.CanStandOn(Key.D), Is.False);
            Assert.That(passability.CanStandOn(Key.F), Is.False);
            Assert.That(passability.CanStandOn(level.Start), Is.False);
            Assert.That(passability.CanStandOn(level.Goal), Is.True);
        }
    }
}
