using System.Linq;
using DetectiveDave.GuardPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class GuardSpawnsTests
    {
        FakeBoard board;
        FakeLevel level;
        KeyPathfinder pathfinder;
        GuardPassability passability;

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard();
            level = new FakeLevel();
            board.Connect(Key.A, Key.S);
            board.Connect(Key.S, Key.D);
            passability = new GuardPassability(level);
            pathfinder = new KeyPathfinder(board, passability.CanStandOn);
        }

        GuardPatrolData Patrol(Key start, Key end, float patrolSeconds = 0f, float chaseSeconds = 0f) =>
            new GuardPatrolData(start, end, 1f, 2f, patrolSeconds, chaseSeconds);

        System.Collections.Generic.List<GuardSpawn> Build(params GuardPatrolData[] patrols) =>
            GuardSpawns.Build(patrols, pathfinder, passability, 0.6f, 0.5f);

        [Test]
        public void TheRouteRunsFromTheStartKeyToTheEndKey()
        {
            var spawns = Build(Patrol(Key.A, Key.D));
            CollectionAssert.AreEqual(new[] { Key.A, Key.S, Key.D }, spawns.Single().Route);
        }

        [Test]
        public void WaitsAreCopiedFromThePatrol()
        {
            var settings = Build(Patrol(Key.A, Key.D)).Single().Settings;
            Assert.AreEqual(1f, settings.WaitBeforeMoveSeconds);
            Assert.AreEqual(2f, settings.WaitAtEndSeconds);
        }

        [Test]
        public void ZeroSpeedsFallBackToTheDefaults()
        {
            var settings = Build(Patrol(Key.A, Key.D)).Single().Settings;
            Assert.AreEqual(0.6f, settings.PatrolSecondsPerKey);
            Assert.AreEqual(0.5f, settings.ChaseSecondsPerKey);
        }

        [Test]
        public void ASpeedOnThePatrolWins()
        {
            var settings = Build(Patrol(Key.A, Key.D, 0.9f, 0.3f)).Single().Settings;
            Assert.AreEqual(0.9f, settings.PatrolSecondsPerKey);
            Assert.AreEqual(0.3f, settings.ChaseSecondsPerKey);
        }

        [Test]
        public void AnEndKeyThatCannotBeReachedLeavesTheGuardOnItsStartKey()
        {
            level.Kinds[Key.S] = CellKind.Gap;
            CollectionAssert.AreEqual(new[] { Key.A }, Build(Patrol(Key.A, Key.D)).Single().Route);
        }

        [Test]
        public void AStartKeyThatCannotBeStoodOnSkipsTheGuard()
        {
            level.Kinds[Key.A] = CellKind.Wall;
            Assert.IsEmpty(Build(Patrol(Key.A, Key.D)));
        }

        [Test]
        public void EveryPatrolBecomesOneSpawn()
        {
            Assert.AreEqual(2, Build(Patrol(Key.A, Key.S), Patrol(Key.S, Key.D)).Count);
        }
    }
}
