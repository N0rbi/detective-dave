using System.Linq;
using DetectiveDave.GuardPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public sealed class KeyPathfinderTests
    {
        FakeBoard board;
        FakeLevel level;

        [SetUp]
        public void CreateRow()
        {
            board = new FakeBoard();
            level = new FakeLevel();
            board.Connect(Key.A, Key.S);
            board.Connect(Key.S, Key.D);
            board.Connect(Key.D, Key.F);
        }

        KeyPathfinder Pathfinder() => new KeyPathfinder(board, key => level.KindOf(key) == DetectiveDave.PlayerPlay.CellKind.Floor);

        [Test]
        public void StraightRow_ListsEveryKeyIncludingBothEnds()
        {
            Assert.That(Pathfinder().FindPath(Key.A, Key.F), Is.EqualTo(new[] { Key.A, Key.S, Key.D, Key.F }));
        }

        [Test]
        public void SameKey_IsAPathOfOneKey()
        {
            Assert.That(Pathfinder().FindPath(Key.S, Key.S), Is.EqualTo(new[] { Key.S }));
        }

        [TestCase(DetectiveDave.PlayerPlay.CellKind.Gap)]
        [TestCase(DetectiveDave.PlayerPlay.CellKind.Gutter)]
        [TestCase(DetectiveDave.PlayerPlay.CellKind.Wall)]
        public void BlockedKeyInTheMiddle_LeavesNoPath(DetectiveDave.PlayerPlay.CellKind blocked)
        {
            level.Kinds[Key.S] = blocked;
            Assert.That(Pathfinder().FindPath(Key.A, Key.F), Is.Empty);
        }

        [Test]
        public void BlockedGoal_LeavesNoPath()
        {
            level.Kinds[Key.F] = DetectiveDave.PlayerPlay.CellKind.Gutter;
            Assert.That(Pathfinder().HasPath(Key.A, Key.F), Is.False);
        }

        [Test]
        public void ShortcutExists_TakesTheShorterWay()
        {
            board.Connect(Key.A, Key.Q);
            board.Connect(Key.Q, Key.F);
            Assert.That(Pathfinder().FindPath(Key.A, Key.F).Count, Is.EqualTo(3));
        }

        [Test]
        public void EqualRoutes_PickTheSameOneEveryTime()
        {
            board.Connect(Key.A, Key.Q);
            board.Connect(Key.Q, Key.F);
            board.Connect(Key.A, Key.W);
            board.Connect(Key.W, Key.F);
            var first = Pathfinder().FindPath(Key.A, Key.F).ToArray();
            var second = Pathfinder().FindPath(Key.A, Key.F).ToArray();
            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void DisconnectedKeys_LeaveNoPath()
        {
            Assert.That(Pathfinder().HasPath(Key.A, Key.Z), Is.False);
        }
    }
}
