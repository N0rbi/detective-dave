using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    public sealed class GuardDirector
    {
        readonly IKeyboardBoard board;
        readonly KeyPathfinder pathfinder;
        readonly Func<IGuardView> createView;
        readonly List<Actor> actors = new List<Actor>();
        bool caught;

        public GuardDirector(IKeyboardBoard board, KeyPathfinder pathfinder, Func<IGuardView> createView)
        {
            this.board = board;
            this.pathfinder = pathfinder;
            this.createView = createView;
        }

        public event Action<Key> Caught;

        public int Count => actors.Count;

        public void Spawn(IEnumerable<GuardSpawn> spawns)
        {
            Clear();
            foreach (var spawn in spawns)
            {
                var actor = new Actor(new GuardBrain(board, pathfinder, spawn.Route, spawn.Settings), createView());
                actors.Add(actor);
                Draw(actor);
            }
        }

        public void Clear()
        {
            caught = false;
            foreach (var actor in actors) actor.View.Release();
            actors.Clear();
        }

        public void Tick(float deltaSeconds, Key playerKey)
        {
            if (caught) return;
            foreach (var actor in actors)
            {
                if (Advance(actor, deltaSeconds, playerKey)) return;
            }
        }

        bool Advance(Actor actor, float deltaSeconds, Key playerKey)
        {
            actor.Elapsed += deltaSeconds;
            var action = actor.Brain.Tick(deltaSeconds, playerKey);
            if (action.Kind == GuardActionKind.StepTo) actor.BeginStep(WorldOf(actor.Brain.Current), WorldOf(action.Target), action.Seconds);
            Draw(actor);
            if (action.Kind != GuardActionKind.Catch) return false;
            caught = true;
            Caught?.Invoke(actor.Brain.Current);
            return true;
        }

        void Draw(Actor actor)
        {
            bool chasing = actor.Brain.Mode == GuardMode.Chasing || actor.Brain.Mode == GuardMode.Catching;
            if (!actor.Brain.IsMoving)
            {
                actor.View.Show(WorldOf(actor.Brain.Current), chasing, 0, actor.FacesRight);
                return;
            }
            float progress = actor.Seconds > 0f ? Mathf.Clamp01(actor.Elapsed / actor.Seconds) : 1f;
            int frame = GuardFrames.At(progress, actor.View.FrameCount(chasing));
            actor.View.Show(Vector2.Lerp(actor.From, actor.To, progress), chasing, frame, actor.FacesRight);
        }

        Vector2 WorldOf(Key key)
        {
            if (!board.TryGetPosition(key, out var world)) throw new InvalidOperationException($"The board has no position for key {key}.");
            return world;
        }

        sealed class Actor
        {
            public readonly GuardBrain Brain;
            public readonly IGuardView View;
            public Vector2 From;
            public Vector2 To;
            public float Seconds;
            public float Elapsed;
            public bool FacesRight = true;

            public Actor(GuardBrain brain, IGuardView view)
            {
                Brain = brain;
                View = view;
            }

            public void BeginStep(Vector2 from, Vector2 to, float seconds)
            {
                From = from;
                To = to;
                Seconds = seconds;
                Elapsed = 0f;
                if (Mathf.Abs(to.x - from.x) > 0.001f) FacesRight = to.x > from.x;
            }
        }
    }
}
