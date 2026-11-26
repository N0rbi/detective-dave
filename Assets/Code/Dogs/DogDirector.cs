using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.OverworldPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.DogPlay
{
    public sealed class DogDirector
    {
        public const int RandomStartRange = 120;
        const int MostTimerStepsPerCall = 8;

        readonly IKeyboardBoard board;
        readonly Func<IDogView> createView;
        readonly IRandomSource random;
        readonly DogPacing pacing;
        readonly List<Dog> dogs = new List<Dog>();
        readonly List<Key> keys = new List<Key>();
        int timerStep;

        public DogDirector(IKeyboardBoard board, Func<IDogView> createView, IRandomSource random, DogPacing pacing)
        {
            this.board = board;
            this.createView = createView;
            this.random = random;
            this.pacing = pacing;
        }

        public int Count => dogs.Count;

        public IReadOnlyList<Key> Keys => keys;

        public void Spawn(IEnumerable<Key> dogKeys, Key playerKey)
        {
            Clear();
            foreach (var key in dogKeys)
            {
                if (!board.TryGetPosition(key, out var world)) continue;
                dogs.Add(new Dog(key, world, createView(), random.Index(RandomStartRange), random.Index(pacing.CalmEveryNotes)));
                keys.Add(key);
            }
            Draw(playerKey);
        }

        public void Clear()
        {
            foreach (var dog in dogs) dog.View.Release();
            dogs.Clear();
            keys.Clear();
        }

        public void Draw(Key playerKey)
        {
            foreach (var dog in dogs) dog.View.Show(dog.World, DogMoods.Of(board, dog.Key, playerKey), dog.Step);
        }

        public void ShowStep(int newStep, Key playerKey)
        {
            if (newStep == timerStep) return;
            var ticks = Mathf.Min(Mathf.Abs(newStep - timerStep), MostTimerStepsPerCall);
            timerStep = newStep;
            for (var tick = 0; tick < ticks; tick++)
                foreach (var dog in dogs)
                    if (!Skips()) dog.Step++;
            Draw(playerKey);
        }

        public void HearBassNotes(int notes, Key playerKey)
        {
            if (notes <= 0) return;
            for (var note = 0; note < notes; note++)
                foreach (var dog in dogs)
                    if (StepsOnThisNote(dog, playerKey)) dog.Step++;
            Draw(playerKey);
        }

        bool StepsOnThisNote(Dog dog, Key playerKey)
        {
            dog.NotesHeard++;
            var calm = DogMoods.Of(board, dog.Key, playerKey) == DogMood.Calm;
            if (calm && dog.NotesHeard % pacing.CalmEveryNotes != 0) return false;
            return !Skips();
        }

        bool Skips() => pacing.SkipChance > 0f && random.Range(0f, 1f) < pacing.SkipChance;

        sealed class Dog
        {
            public readonly Key Key;
            public readonly Vector2 World;
            public readonly IDogView View;
            public int Step;
            public int NotesHeard;

            public Dog(Key key, Vector2 world, IDogView view, int step, int notesHeard)
            {
                Key = key;
                World = world;
                View = view;
                Step = step;
                NotesHeard = notesHeard;
            }
        }
    }
}
