using System.Collections.Generic;
using DetectiveDave.DogPlay;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class DogTests
    {
        const Key DogKey = Key.G;
        const Key NextToDog = Key.H;
        const Key FarAway = Key.P;

        sealed class FakeDogView : IDogView
        {
            public Vector2 World;
            public DogMood Mood;
            public int Step;
            public bool Released;

            public void Show(Vector2 world, DogMood mood, int step)
            {
                World = world;
                Mood = mood;
                Step = step;
            }

            public void Release() => Released = true;
        }

        sealed class ScriptedRandom : IRandomSource
        {
            public Queue<int> Indexes = new Queue<int>();
            public float Roll;

            public float Range(float min, float max) => Roll;

            public int Index(int count) => Indexes.Count > 0 ? Indexes.Dequeue() : 0;
        }

        FakeBoard board;
        List<FakeDogView> views;
        DogDirector director;
        ScriptedRandom random;

        DogDirector NewDirector(DogPacing pacing) => new DogDirector(board, () =>
        {
            var view = new FakeDogView();
            views.Add(view);
            return view;
        }, random, pacing);

        [SetUp]
        public void SetUp()
        {
            board = new FakeBoard();
            board.Place(DogKey, new Vector2(1f, 2f));
            board.Place(NextToDog, new Vector2(2f, 2f));
            board.Place(FarAway, new Vector2(9f, 2f));
            board.Connect(DogKey, NextToDog);
            views = new List<FakeDogView>();
            random = new ScriptedRandom { Roll = 1f };
            director = NewDirector(new DogPacing(0f, 2));
        }

        [Test]
        public void ADogIsCalmWhenThePlayerIsFarAway() => Assert.AreEqual(DogMood.Calm, DogMoods.Of(board, DogKey, FarAway));

        [Test]
        public void ADogIsAngryWhenThePlayerStandsNextToIt() => Assert.AreEqual(DogMood.Angry, DogMoods.Of(board, DogKey, NextToDog));

        [Test]
        public void ADogCalmsDownWhenThePlayerLandsOnIt() => Assert.AreEqual(DogMood.Calm, DogMoods.Of(board, DogKey, DogKey));

        [TestCase(0, 3, 0)]
        [TestCase(1, 3, 1)]
        [TestCase(2, 3, 2)]
        [TestCase(3, 3, 1)]
        [TestCase(4, 3, 0)]
        [TestCase(5, 3, 1)]
        [TestCase(0, 1, 0)]
        [TestCase(7, 1, 0)]
        [TestCase(1, 2, 1)]
        [TestCase(2, 2, 0)]
        public void FramesPlayBackAndForth(int step, int count, int expected) => Assert.AreEqual(expected, DogFrames.PingPongIndex(step, count));

        [Test]
        public void StepsAdvanceWithTime()
        {
            Assert.AreEqual(0, DogFrames.StepAt(0.1f, 8f));
            Assert.AreEqual(8, DogFrames.StepAt(1f, 8f));
        }

        [Test]
        public void SpawnShowsADogOnItsKeyWithTheMoodOfThePlayer()
        {
            director.Spawn(new[] { DogKey }, NextToDog);
            Assert.AreEqual(1, director.Count);
            Assert.AreEqual(new Vector2(1f, 2f), views[0].World);
            Assert.AreEqual(DogMood.Angry, views[0].Mood);
        }

        [Test]
        public void ADogOnAKeyTheBoardDoesNotHaveIsSkipped()
        {
            director.Spawn(new[] { Key.Z }, FarAway);
            Assert.AreEqual(0, director.Count);
        }

        [Test]
        public void DrawChangesTheMoodWhenThePlayerMoves()
        {
            director.Spawn(new[] { DogKey }, FarAway);
            Assert.AreEqual(DogMood.Calm, views[0].Mood);
            director.Draw(NextToDog);
            Assert.AreEqual(DogMood.Angry, views[0].Mood);
            director.Draw(DogKey);
            Assert.AreEqual(DogMood.Calm, views[0].Mood);
        }

        [Test]
        public void ShowStepRedrawsOnlyWhenTheStepChanges()
        {
            director.Spawn(new[] { DogKey }, NextToDog);
            director.ShowStep(2, NextToDog);
            Assert.AreEqual(2, views[0].Step);
            views[0].Step = -1;
            director.ShowStep(2, NextToDog);
            Assert.AreEqual(-1, views[0].Step);
        }

        [Test]
        public void EveryDogStartsOnItsOwnFrame()
        {
            random.Indexes = new Queue<int>(new[] { 5, 0, 17, 0 });
            director.Spawn(new[] { DogKey, FarAway }, Key.Q);
            Assert.AreEqual(5, views[0].Step);
            Assert.AreEqual(17, views[1].Step);
        }

        [Test]
        public void ADogThatRollsASkipDoesNotStepOnATimerTick()
        {
            director = NewDirector(new DogPacing(0.5f, 2));
            random.Roll = 0.1f;
            director.Spawn(new[] { DogKey }, FarAway);
            director.ShowStep(1, FarAway);
            Assert.AreEqual(0, views[0].Step);
            random.Roll = 0.9f;
            director.ShowStep(2, FarAway);
            Assert.AreEqual(1, views[0].Step);
        }

        [Test]
        public void AnAngryDogStepsOnEveryBassNote()
        {
            director.Spawn(new[] { DogKey }, NextToDog);
            director.HearBassNotes(3, NextToDog);
            Assert.AreEqual(3, views[0].Step);
        }

        [Test]
        public void ACalmDogStepsOnEveryOtherBassNote()
        {
            director.Spawn(new[] { DogKey }, FarAway);
            director.HearBassNotes(4, FarAway);
            Assert.AreEqual(2, views[0].Step);
        }

        [Test]
        public void NoBassNotesChangeNothing()
        {
            director.Spawn(new[] { DogKey }, NextToDog);
            views[0].Step = -1;
            director.HearBassNotes(0, NextToDog);
            Assert.AreEqual(-1, views[0].Step);
        }

        [Test]
        public void ClearReleasesEveryView()
        {
            director.Spawn(new[] { DogKey }, FarAway);
            director.Clear();
            Assert.IsTrue(views[0].Released);
            Assert.AreEqual(0, director.Count);
            Assert.AreEqual(0, director.Keys.Count);
        }

        [Test]
        public void SpawnReplacesTheDogsOfBefore()
        {
            director.Spawn(new[] { DogKey }, FarAway);
            director.Spawn(new[] { NextToDog }, FarAway);
            Assert.IsTrue(views[0].Released);
            Assert.AreEqual(new[] { NextToDog }, director.Keys);
        }
    }
}
