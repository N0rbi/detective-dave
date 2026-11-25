using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.DogPlay
{
    public sealed class DogDirector
    {
        readonly IKeyboardBoard board;
        readonly Func<IDogView> createView;
        readonly List<Dog> dogs = new List<Dog>();
        readonly List<Key> keys = new List<Key>();
        int step;

        public DogDirector(IKeyboardBoard board, Func<IDogView> createView)
        {
            this.board = board;
            this.createView = createView;
        }

        public int Count => dogs.Count;

        public IReadOnlyList<Key> Keys => keys;

        public void Spawn(IEnumerable<Key> dogKeys, Key playerKey)
        {
            Clear();
            foreach (var key in dogKeys)
            {
                if (!board.TryGetPosition(key, out var world)) continue;
                dogs.Add(new Dog(key, world, createView()));
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
            foreach (var dog in dogs) dog.View.Show(dog.World, DogMoods.Of(board, dog.Key, playerKey), step);
        }

        public void ShowStep(int newStep, Key playerKey)
        {
            if (newStep == step) return;
            step = newStep;
            Draw(playerKey);
        }

        sealed class Dog
        {
            public readonly Key Key;
            public readonly Vector2 World;
            public readonly IDogView View;

            public Dog(Key key, Vector2 world, IDogView view)
            {
                Key = key;
                World = world;
                View = view;
            }
        }
    }
}
