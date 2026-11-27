using System.Collections.Generic;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class PlayerGameMoveBufferTests
    {
        const Key Start = Key.LeftShift;
        const Key First = Key.S;
        const Key Second = Key.D;

        float now;
        FakeAnimations animations;
        PlayerGame game;
        List<Key> moved;

        [SetUp]
        public void SetUp()
        {
            now = 0f;
            var board = new FakeBoard { KeyWorldSize = 2f };
            board.Place(Start, Vector2.zero);
            board.Place(First, new Vector2(1f, 0f));
            board.Place(Second, new Vector2(2f, 0f));
            board.Place(Key.RightShift, new Vector2(9f, 9f));
            board.Connect(Start, First);
            board.Connect(First, Second);
            animations = new FakeAnimations();
            game = new PlayerGame(board, new FakeLevel(), animations, new FakePresentation(), new FakeDashGust(), new PlayerGameSettings(3, 3, 0.35f), new PendingMove(0.15f, () => now));
            moved = new List<Key>();
            game.Moved += moved.Add;
            game.Restart();
            game.OnKeyPressed(Start);
        }

        [Test]
        public void KeyPressedDuringAStepIsPlayedWhenTheStepEnds()
        {
            game.OnKeyPressed(First);
            now = 0.05f;
            game.OnKeyPressed(Second);
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Moving, game.State);
            animations.CompleteLast();
            CollectionAssert.AreEqual(new[] { First, Second }, moved);
        }

        [Test]
        public void KeyPressedTooLongBeforeTheStepEndsIsLost()
        {
            game.OnKeyPressed(First);
            game.OnKeyPressed(Second);
            now = 0.5f;
            animations.CompleteLast();
            Assert.AreEqual(PlayerState.Idle, game.State);
            CollectionAssert.AreEqual(new[] { First }, moved);
        }

        [Test]
        public void RestartForgetsTheKey()
        {
            game.OnKeyPressed(First);
            game.OnKeyPressed(Second);
            game.Restart();
            game.OnKeyPressed(Start);
            Assert.AreEqual(PlayerState.Idle, game.State);
            CollectionAssert.IsEmpty(moved);
        }
    }
}
