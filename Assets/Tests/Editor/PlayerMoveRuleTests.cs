using DetectiveDave.KeyboardGrid;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class PlayerMoveRuleTests
    {
        const Key Here = Key.A;
        const Key Next = Key.S;
        const Key Far = Key.L;
        const Key Goal = Key.RightShift;

        readonly PlayerMoveRule rule = new PlayerMoveRule();
        FakeBoard board;
        FakeLevel level;

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard();
            board.Connect(Here, Next);
            level = new FakeLevel();
        }

        [Test]
        public void SameKeyIsIgnored() => Assert.AreEqual(MoveOutcome.Ignored, rule.Decide(board, level, Goal, Here, Here));

        [Test]
        public void SameKeyIsIgnoredEvenWhenItIsANeighbourOfItself()
        {
            board.Connect(Here, Here);
            Assert.AreEqual(MoveOutcome.Ignored, rule.Decide(board, level, Goal, Here, Here));
        }

        [Test]
        public void KeyThatIsNotANeighbourStuns() => Assert.AreEqual(MoveOutcome.Stun, rule.Decide(board, level, Goal, Here, Far));

        [TestCase(CellKind.Floor, MoveOutcome.Step)]
        [TestCase(CellKind.Wall, MoveOutcome.BumpWall)]
        [TestCase(CellKind.Gap, MoveOutcome.FallIntoGap)]
        [TestCase(CellKind.Gutter, MoveOutcome.HangOnGutter)]
        public void NeighbourOutcomeFollowsItsKind(CellKind kind, MoveOutcome expected)
        {
            level.Kinds[Next] = kind;
            Assert.AreEqual(expected, rule.Decide(board, level, Goal, Here, Next));
        }

        [TestCase(CellKind.Floor)]
        [TestCase(CellKind.Gap)]
        [TestCase(CellKind.Wall)]
        public void PressingTheGoalNeighbourReachesGoalWhateverItsKind(CellKind kind)
        {
            board.Connect(Here, Goal);
            level.Kinds[Goal] = kind;
            Assert.AreEqual(MoveOutcome.ReachGoal, rule.Decide(board, level, Goal, Here, Goal));
        }
    }
}
