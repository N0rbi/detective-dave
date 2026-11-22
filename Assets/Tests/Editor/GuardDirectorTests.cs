using System.Collections.Generic;
using DetectiveDave.GuardPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class GuardDirectorTests
    {
        sealed class FakeGuardView : IGuardView
        {
            public Vector2 World;
            public bool Chasing;
            public bool FacesRight;
            public bool Released;
            public int Shown;

            public void Show(Vector2 world, bool chasing, int frame, bool facesRight)
            {
                World = world;
                Chasing = chasing;
                FacesRight = facesRight;
                Shown++;
            }

            public int FrameCount(bool chasing) => 3;

            public void Release() => Released = true;
        }

        FakeBoard board;
        FakeLevel level;
        List<FakeGuardView> views;
        GuardDirector director;
        List<Key> caught;

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard();
            level = new FakeLevel();
            var row = new[] { Key.A, Key.S, Key.D };
            for (int i = 0; i < row.Length; i++) board.Place(row[i], new Vector2(i * 2f, 0f));
            board.Connect(Key.A, Key.S);
            board.Connect(Key.S, Key.D);
            views = new List<FakeGuardView>();
            var pathfinder = new KeyPathfinder(board, key => level.KindOf(key) == DetectiveDave.PlayerPlay.CellKind.Floor);
            director = new GuardDirector(board, pathfinder, () =>
            {
                var view = new FakeGuardView();
                views.Add(view);
                return view;
            });
            caught = new List<Key>();
            director.Caught += caught.Add;
        }

        static GuardSpawn Spawn(params Key[] route) => new GuardSpawn(route, new GuardBrainSettings(0f, 0f, 1f, 0.5f));

        [Test]
        public void SpawnCreatesAViewPerGuardAtItsStartKey()
        {
            director.Spawn(new[] { Spawn(Key.A, Key.D), Spawn(Key.D) });
            Assert.AreEqual(2, director.Count);
            Assert.AreEqual(new Vector2(0f, 0f), views[0].World);
            Assert.AreEqual(new Vector2(4f, 0f), views[1].World);
        }

        [Test]
        public void SpawnAgainReleasesTheOldViews()
        {
            director.Spawn(new[] { Spawn(Key.A) });
            director.Spawn(new[] { Spawn(Key.A) });
            Assert.IsTrue(views[0].Released);
            Assert.AreEqual(1, director.Count);
        }

        [Test]
        public void ClearReleasesEveryView()
        {
            director.Spawn(new[] { Spawn(Key.A), Spawn(Key.D) });
            director.Clear();
            Assert.IsTrue(views.TrueForAll(view => view.Released));
            Assert.AreEqual(0, director.Count);
        }

        [Test]
        public void AGuardWalksSmoothlyBetweenKeys()
        {
            director.Spawn(new[] { Spawn(Key.A, Key.S) });
            director.Tick(0f, Key.Z);
            director.Tick(0.5f, Key.Z);
            Assert.AreEqual(1f, views[0].World.x, 0.001f);
            Assert.IsTrue(views[0].FacesRight);
        }

        [Test]
        public void AGuardFacesTheWayItWalks()
        {
            director.Spawn(new[] { Spawn(Key.D, Key.S) });
            director.Tick(0f, Key.Z);
            director.Tick(0.5f, Key.Z);
            Assert.IsFalse(views[0].FacesRight);
        }

        [Test]
        public void ANeighbouringPlayerMakesTheGuardChase()
        {
            director.Spawn(new[] { Spawn(Key.A) });
            director.Tick(0f, Key.S);
            Assert.IsTrue(views[0].Chasing);
        }

        [Test]
        public void ANeighbourAppearingMidStepRedirectsTheGuardFromWhereItIsWithoutAPop()
        {
            director.Spawn(new[] { Spawn(Key.A, Key.S) });
            director.Tick(0f, Key.Z);
            director.Tick(0.5f, Key.Z);
            director.Tick(0.1f, Key.D);
            Assert.AreEqual(1.2f, views[0].World.x, 0.001f);
            Assert.IsTrue(views[0].Chasing);
            director.Tick(0.1f, Key.D);
            Assert.AreEqual(1.6f, views[0].World.x, 0.001f);
        }

        [Test]
        public void ANeighbourAppearingEarlyInAStepTurnsTheGuardBackSmoothly()
        {
            director.Spawn(new[] { Spawn(Key.A, Key.S) });
            director.Tick(0f, Key.Z);
            director.Tick(0.2f, Key.D);
            Assert.AreEqual(0.4f, views[0].World.x, 0.001f);
            director.Tick(0.025f, Key.D);
            Assert.AreEqual(0.3f, views[0].World.x, 0.001f);
            Assert.IsFalse(views[0].FacesRight);
        }

        [Test]
        public void CatchingThePlayerRaisesCaughtOnce()
        {
            director.Spawn(new[] { Spawn(Key.A) });
            director.Tick(0.1f, Key.A);
            director.Tick(0.1f, Key.A);
            CollectionAssert.AreEqual(new[] { Key.A }, caught);
        }

        [Test]
        public void NobodyMovesAfterTheCatch()
        {
            director.Spawn(new[] { Spawn(Key.A), Spawn(Key.D, Key.S) });
            director.Tick(0.1f, Key.A);
            int shown = views[1].Shown;
            director.Tick(1f, Key.A);
            Assert.AreEqual(shown, views[1].Shown);
        }

        [Test]
        public void SpawnAfterACatchWorksAgain()
        {
            director.Spawn(new[] { Spawn(Key.A) });
            director.Tick(0.1f, Key.A);
            director.Spawn(new[] { Spawn(Key.A) });
            director.Tick(0.1f, Key.A);
            Assert.AreEqual(2, caught.Count);
        }
    }
}
