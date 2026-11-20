using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class EndDecorationsTests
    {
        const float KeyWorldSize = 2f;
        const float BurstRiseInKeys = 0.55f;

        static readonly Vector2 StartWorld = new Vector2(0f, 0f);
        static readonly Vector2 GoalWorld = new Vector2(4f, 1f);

        FakeBoard board;
        FakeLevel level;
        FakeEndDecorations decorations;
        EndDecorationsDirector director;

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard { KeyWorldSize = KeyWorldSize };
            board.Place(Key.LeftShift, StartWorld);
            board.Place(Key.RightShift, GoalWorld);
            level = new FakeLevel();
            decorations = new FakeEndDecorations();
            director = new EndDecorationsDirector(board, level, decorations, BurstRiseInKeys);
        }

        [Test]
        public void RefreshShowsGrateOnStartAndIconOnGoal()
        {
            director.Refresh(true);
            Assert.AreEqual(StartWorld, decorations.Start);
            Assert.AreEqual(GoalWorld, decorations.Goal);
        }

        [Test]
        public void RefreshHidesEverythingWhenThePlayerIsNotActive()
        {
            director.Refresh(true);
            director.Refresh(false);
            Assert.IsNull(decorations.Start);
            Assert.IsNull(decorations.Goal);
        }

        [Test]
        public void RefreshHidesEverythingWhenTheEndsAreMissing()
        {
            level.EndsAvailable = false;
            director.Refresh(true);
            Assert.IsNull(decorations.Start);
            Assert.IsNull(decorations.Goal);
        }

        [Test]
        public void RefreshHidesTheBurst()
        {
            director.Refresh(true);
            director.ShowBurstOnGoal();
            director.Refresh(true);
            Assert.IsNull(decorations.Burst);
        }

        [Test]
        public void BurstIsCentredAboveTheGoalByTheSettingRise()
        {
            director.Refresh(true);
            director.ShowBurstOnGoal();
            Assert.AreEqual(GoalWorld + Vector2.up * (BurstRiseInKeys * KeyWorldSize), decorations.Burst);
        }

        [Test]
        public void BurstIsNotShownBeforeTheGoal()
        {
            director.Refresh(true);
            Assert.IsNull(decorations.Burst);
        }

        [Test]
        public void ClearHidesEverything()
        {
            director.Refresh(true);
            director.ShowBurstOnGoal();
            director.Clear();
            Assert.IsNull(decorations.Start);
            Assert.IsNull(decorations.Goal);
            Assert.IsNull(decorations.Burst);
        }

        [Test]
        public void ScaleFitsTheSpriteWidthToTheSizeInKeys()
        {
            Assert.AreEqual(0.5f, DecorationScale.For(2f, 0.25f, 1f), 1e-5f);
            Assert.AreEqual(0.25f, DecorationScale.For(1f, 0.25f, 1f), 1e-5f);
        }

        [Test]
        public void ScaleGrowsWithTheKeyScaleAndShrinksWithTheSpriteWidth()
        {
            Assert.AreEqual(2f, DecorationScale.For(1f, 4f, 2f), 1e-5f);
        }

        [Test]
        public void SecondsPerTurnIsFullTurnOverSpeed()
        {
            Assert.AreEqual(12f, DecorationScale.SecondsPerTurn(30f), 1e-5f);
        }

        [Test]
        public void SecondsPerTurnStaysFiniteForAZeroSpeed()
        {
            Assert.AreEqual(360f, DecorationScale.SecondsPerTurn(0f), 1e-5f);
        }

        sealed class FakeEndDecorations : IEndDecorations
        {
            public Vector2? Start { get; private set; }

            public Vector2? Goal { get; private set; }

            public Vector2? Burst { get; private set; }

            public void ShowStart(Vector2 world) => Start = world;

            public void ShowGoal(Vector2 world) => Goal = world;

            public void ShowBurst(Vector2 world) => Burst = world;

            public void HideAll()
            {
                Start = null;
                Goal = null;
                Burst = null;
            }
        }
    }
}
