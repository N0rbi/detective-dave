using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.PlayerPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public sealed class FakeBoard : IKeyboardBoard
    {
        readonly Dictionary<Key, Vector2> positions = new Dictionary<Key, Vector2>();
        readonly HashSet<(Key, Key)> neighbours = new HashSet<(Key, Key)>();

        public float KeyWorldSize { get; set; } = 1f;

        public void Place(Key key, Vector2 world) => positions[key] = world;

        public void Connect(Key key, Key other)
        {
            neighbours.Add((key, other));
            neighbours.Add((other, key));
        }

        public bool IsNeighbour(Key key, Key other) => neighbours.Contains((key, other));

        public IReadOnlyCollection<Key> GetNeighbours(Key key)
        {
            var found = new List<Key>();
            foreach (var pair in neighbours)
            {
                if (pair.Item1 == key) found.Add(pair.Item2);
            }
            return found;
        }

        public bool TryGetPosition(Key key, out Vector2 world) => positions.TryGetValue(key, out world);
    }

    public sealed class FakeLevel : IPlayerLevel
    {
        public readonly Dictionary<Key, CellKind> Kinds = new Dictionary<Key, CellKind>();

        public Key Start { get; set; } = Key.LeftShift;

        public Key Goal { get; set; } = Key.RightShift;

        public bool HasLevel { get; set; } = true;

        public bool EndsAvailable { get; set; } = true;

        public event Action LevelChanged;

        public void RaiseLevelChanged() => LevelChanged?.Invoke();

        public bool TryGetEnds(out Key start, out Key goal)
        {
            start = Start;
            goal = Goal;
            return EndsAvailable;
        }

        public CellKind KindOf(Key key) => Kinds.TryGetValue(key, out var kind) ? kind : CellKind.Floor;
    }

    public sealed class FakePresentation : IPlayerPresentation
    {
        public bool Visible { get; private set; }

        public int Frame { get; private set; }

        public Facing Facing { get; private set; }

        public bool Climbing { get; private set; }

        public void SetVisible(bool visible) => Visible = visible;

        public void SetFrame(int frame) => Frame = frame;

        public void SetFacing(Facing facing) => Facing = facing;

        public void SetClimbing(bool climbing) => Climbing = climbing;
    }

    public sealed class FakeAnimations : IPlayerAnimations
    {
        readonly List<Action> pending = new List<Action>();

        public readonly List<string> Calls = new List<string>();

        public bool KeepsPendingOnCancel { get; set; }

        public Vector2 LastMoveTarget { get; private set; }

        public Vector2 LastSlipOffset { get; private set; }

        public Vector2 LastBumpTarget { get; private set; }

        public Vector2 LastBumpHome { get; private set; }

        public Action BumpReturning { get; private set; }

        public Vector2 LastSquashDirection => (LastBumpTarget - LastBumpHome).normalized;

        public bool IsBreathing { get; private set; }

        public int BreathingStarts { get; private set; }

        public Vector2 LastDropTarget { get; private set; }

        public float LastDropMargin { get; private set; }

        public Vector2 LastShownAt { get; private set; }

        public void ShowResetAt(Vector2 world)
        {
            Calls.Add(nameof(ShowResetAt));
            IsBreathing = false;
            LastShownAt = world;
        }

        public void MoveTo(Vector2 world, Action done)
        {
            Calls.Add(nameof(MoveTo));
            IsBreathing = false;
            LastMoveTarget = world;
            pending.Add(done);
        }

        public void SlipBy(Vector2 worldOffset, Action done)
        {
            Calls.Add(nameof(SlipBy));
            IsBreathing = false;
            LastSlipOffset = worldOffset;
            pending.Add(done);
        }

        public void BumpWall(Vector2 towards, Vector2 home, Action returning, Action done)
        {
            Calls.Add(nameof(BumpWall));
            BumpReturning = returning;
            LastBumpTarget = towards;
            LastBumpHome = home;
            IsBreathing = false;
            pending.Add(done);
        }

        public void StartBreathing()
        {
            if (IsBreathing) return;
            IsBreathing = true;
            BreathingStarts++;
        }

        public void StopBreathing() => IsBreathing = false;

        public void FadeIntoGap(Action done)
        {
            Calls.Add(nameof(FadeIntoGap));
            IsBreathing = false;
            pending.Add(done);
        }

        public void DropFromTop(Vector2 target, float marginWorld, Action done)
        {
            Calls.Add(nameof(DropFromTop));
            IsBreathing = false;
            LastDropTarget = target;
            LastDropMargin = marginWorld;
            pending.Add(done);
        }

        public void CancelAllWithoutCallbacks()
        {
            Calls.Add(nameof(CancelAllWithoutCallbacks));
            IsBreathing = false;
            if (!KeepsPendingOnCancel) pending.Clear();
        }

        public void CompleteLast()
        {
            var done = pending[pending.Count - 1];
            pending.RemoveAt(pending.Count - 1);
            done();
        }

        public void CompleteAll()
        {
            var batch = pending.ToArray();
            pending.Clear();
            foreach (var done in batch) done();
        }
    }

    public sealed class FakeDashGust : IDashGust
    {
        public readonly List<(Vector2 From, Vector2 To)> Dashes = new List<(Vector2 From, Vector2 To)>();

        public int Clears { get; private set; }

        public void Show(Vector2 from, Vector2 to) => Dashes.Add((from, to));

        public void Clear() => Clears++;
    }
}
