using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class DeskLevelTests
    {
        readonly DeskLevel level = new DeskLevel();

        [Test]
        public void DeskStartsOnTheLeftShiftWithoutAGoal()
        {
            Assert.IsTrue(level.TryGetEnds(out var start, out var goal));
            Assert.AreEqual(Key.LeftShift, start);
            Assert.AreEqual(Key.None, goal);
        }

        [Test]
        public void EveryKeyOnTheDeskIsFloor()
        {
            Assert.AreEqual(CellKind.Floor, level.KindOf(Key.A));
            Assert.AreEqual(CellKind.Floor, level.KindOf(Key.RightShift));
        }

        [Test]
        public void RefreshRaisesLevelChanged()
        {
            int raised = 0;
            level.LevelChanged += () => raised++;
            level.Refresh();
            Assert.AreEqual(1, raised);
        }

        [Test]
        public void ForgivingPlayerWalksTheDeskAndNeverReachesAGoal()
        {
            var board = new FakeBoard { KeyWorldSize = 1f };
            board.Place(Key.LeftShift, Vector2.zero);
            board.Place(Key.Z, new Vector2(1f, 0f));
            board.Connect(Key.LeftShift, Key.Z);
            var animations = new FakeAnimations();
            var game = new PlayerGame(board, level, animations, new FakePresentation(), new FakeDashGust(), new PlayerGameSettings(1, 1, 1f, true));
            int goals = 0;
            game.GoalReached += () => goals++;
            game.Restart();
            game.OnKeyPressed(Key.LeftShift);
            game.OnKeyPressed(Key.Q);
            game.OnKeyPressed(Key.Z);
            animations.CompleteAll();
            Assert.AreEqual(PlayerState.Idle, game.State);
            Assert.AreEqual(0, goals);
        }
    }
}
