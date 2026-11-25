using System.Collections.Generic;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class PlayerMoveAttemptTests
    {
        const Key Start = Key.LeftShift;
        const Key Next = Key.S;
        const Key Goal = Key.RightShift;
        const Key Far = Key.L;

        FakeBoard board;
        FakeLevel level;
        FakeAnimations animations;
        List<MoveAttempt> attempts;

        PlayerGame Build(bool forgiving)
        {
            board = new FakeBoard { KeyWorldSize = 2f };
            board.Place(Start, new Vector2(0f, 0f));
            board.Place(Next, new Vector2(1f, 0f));
            board.Place(Goal, new Vector2(2f, 0f));
            board.Place(Far, new Vector2(9f, 9f));
            board.Connect(Start, Next);
            board.Connect(Next, Goal);
            level = new FakeLevel();
            animations = new FakeAnimations();
            var game = new PlayerGame(board, level, animations, new FakePresentation(), new FakeDashGust(), new PlayerGameSettings(3, 3, 0.35f, forgiving));
            attempts = new List<MoveAttempt>();
            game.MoveAttempted += attempts.Add;
            return game;
        }

        static void Begin(PlayerGame game)
        {
            game.Restart();
            game.OnKeyPressed(Start);
        }

        [Test]
        public void Nothing_is_reported_before_the_player_has_started()
        {
            var game = Build(false);
            game.Restart();
            game.OnKeyPressed(Next);
            game.OnKeyPressed(Start);
            Assert.IsEmpty(attempts);
        }

        [Test]
        public void A_step_is_a_move()
        {
            var game = Build(false);
            Begin(game);
            game.OnKeyPressed(Next);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Moved }, attempts);
        }

        [Test]
        public void Reaching_the_goal_is_a_move()
        {
            var game = Build(false);
            level.Goal = Next;
            Begin(game);
            game.OnKeyPressed(Next);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Moved }, attempts);
        }

        [Test]
        public void A_gap_is_a_move()
        {
            var game = Build(false);
            level.Kinds[Next] = CellKind.Gap;
            Begin(game);
            game.OnKeyPressed(Next);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Moved }, attempts);
        }

        [Test]
        public void A_gutter_is_a_move()
        {
            var game = Build(false);
            level.Kinds[Next] = CellKind.Gutter;
            Begin(game);
            game.OnKeyPressed(Next);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Moved }, attempts);
        }

        [Test]
        public void A_wall_is_blocked()
        {
            var game = Build(false);
            level.Kinds[Next] = CellKind.Wall;
            Begin(game);
            game.OnKeyPressed(Next);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Blocked }, attempts);
        }

        [Test]
        public void Pressing_the_key_the_player_stands_on_is_blocked()
        {
            var game = Build(false);
            Begin(game);
            game.OnKeyPressed(Start);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Blocked }, attempts);
        }

        [Test]
        public void A_key_that_is_not_next_to_the_player_is_blocked_when_slips_are_forgiven()
        {
            var game = Build(true);
            Begin(game);
            game.OnKeyPressed(Far);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Blocked }, attempts);
        }

        [Test]
        public void A_key_that_is_not_next_to_the_player_is_blocked_when_the_player_is_stunned()
        {
            var game = Build(false);
            Begin(game);
            game.OnKeyPressed(Far);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Blocked }, attempts);
        }

        [Test]
        public void Presses_while_the_player_is_stunned_report_nothing_more()
        {
            var game = Build(false);
            Begin(game);
            game.OnKeyPressed(Far);
            game.OnKeyPressed(Far);
            game.OnKeyPressed(Next);
            CollectionAssert.AreEqual(new[] { MoveAttempt.Blocked }, attempts);
        }

        [Test]
        public void Presses_while_the_player_is_busy_report_nothing()
        {
            var game = Build(false);
            Begin(game);
            game.OnKeyPressed(Next);
            game.OnKeyPressed(Goal);
            game.OnKeyPressed(Start);
            Assert.AreEqual(1, attempts.Count);
        }

        [Test]
        public void Presses_while_the_player_falls_report_nothing()
        {
            var game = Build(false);
            level.Kinds[Next] = CellKind.Gap;
            Begin(game);
            game.OnKeyPressed(Next);
            game.OnKeyPressed(Start);
            Assert.AreEqual(1, attempts.Count);
        }
    }
}
