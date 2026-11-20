using System;
using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public sealed class PlayerGame
    {
        readonly IKeyboardBoard board;
        readonly IPlayerLevel level;
        readonly IPlayerAnimations animations;
        readonly IPlayerPresentation presentation;
        readonly PlayerGameSettings settings;
        readonly PlayerStateMachine state = new PlayerStateMachine();
        readonly PlayerFacing facing = new PlayerFacing();
        readonly PlayerMoveRule rule = new PlayerMoveRule();
        readonly WalkCycle walk;
        readonly WalkCycle climb;
        Key startKey;
        Key goalKey;
        Key currentKey;
        int generation;

        public PlayerGame(IKeyboardBoard board, IPlayerLevel level, IPlayerAnimations animations, IPlayerPresentation presentation, PlayerGameSettings settings)
        {
            this.board = board;
            this.level = level;
            this.animations = animations;
            this.presentation = presentation;
            this.settings = settings;
            walk = new WalkCycle(settings.WalkFrameCount);
            climb = new WalkCycle(settings.ClimbFrameCount);
            state.Changed += BreatheWhileIdle;
        }

        public event Action<PlayerState> StateChanged { add => state.Changed += value; remove => state.Changed -= value; }
        public event Action<Key> Moved;
        public event Action Failed;
        public event Action GoalReached;

        public PlayerState State => state.State;

        public bool IsActive { get; private set; }

        public bool EndsMissing { get; private set; }

        public void Restart()
        {
            generation++;
            animations.CancelAllWithoutCallbacks();
            state.Reset();
            EndsMissing = false;
            if (!level.HasLevel)
            {
                HidePlayer();
                return;
            }
            if (!TryFindEnds(out var startWorld, out var goalWorld))
            {
                EndsMissing = true;
                HidePlayer();
                return;
            }
            IsActive = true;
            facing.SetTowardsGoal(startWorld, goalWorld);
            ResetToStart();
            presentation.SetVisible(true);
            animations.ShowResetAt(startWorld);
            animations.StartBreathing();
        }

        public void Stop()
        {
            generation++;
            animations.CancelAllWithoutCallbacks();
            IsActive = false;
        }

        public void OnKeyPressed(Key pressed)
        {
            if (!IsActive) return;
            if (state.State == PlayerState.NotStarted)
            {
                if (pressed == startKey) state.Begin();
                return;
            }
            if (state.AcceptsMoves) PerformOutcome(rule.Decide(board, level, goalKey, currentKey, pressed), pressed);
        }

        void HidePlayer()
        {
            presentation.SetVisible(false);
            IsActive = false;
        }

        bool TryFindEnds(out Vector2 startWorld, out Vector2 goalWorld)
        {
            startWorld = default;
            goalWorld = default;
            return level.TryGetEnds(out startKey, out goalKey) && board.TryGetPosition(startKey, out startWorld) && board.TryGetPosition(goalKey, out goalWorld);
        }

        void PerformOutcome(MoveOutcome outcome, Key target)
        {
            switch (outcome)
            {
                case MoveOutcome.Step: Walk(target, PlayerState.Moving, () => StepTo(target)); break;
                case MoveOutcome.HangOnGutter: Walk(target, PlayerState.Moving, () => HangOn(target)); break;
                case MoveOutcome.ReachGoal: Walk(target, PlayerState.Moving, () => FinishGoalWalk(target)); break;
                case MoveOutcome.FallIntoGap: Walk(target, PlayerState.FallingInGap, FallInGap); break;
                case MoveOutcome.Slip: Slip(); break;
                case MoveOutcome.BumpWall: BumpInto(target); break;
            }
        }

        void Walk(Key target, PlayerState during, Action arrived)
        {
            var to = WorldOf(target);
            state.Enter(during);
            FaceTowards(to);
            ShowMovePose(target);
            animations.MoveTo(to, Guard(arrived));
        }

        void ShowMovePose(Key target)
        {
            if (TouchesGutter(target))
            {
                climb.Advance();
                ShowClimbPose();
                return;
            }
            walk.Advance();
            ShowWalkPose();
        }

        bool TouchesGutter(Key target) => level.KindOf(target) == CellKind.Gutter || level.KindOf(currentKey) == CellKind.Gutter;

        void ShowWalkPose()
        {
            presentation.SetClimbing(false);
            presentation.SetFrame(walk.Frame);
        }

        void ShowClimbPose()
        {
            presentation.SetClimbing(true);
            presentation.SetFrame(climb.Frame);
        }

        void StepTo(Key target)
        {
            ShowWalkPose();
            Arrive(target);
            state.Enter(PlayerState.Idle);
        }

        void HangOn(Key target)
        {
            ShowClimbPose();
            Arrive(target);
            state.Enter(PlayerState.Idle);
        }

        void FinishGoalWalk(Key target)
        {
            ShowWalkPose();
            Arrive(target);
            state.Enter(PlayerState.Goal);
            GoalReached?.Invoke();
        }

        void FallInGap() => animations.FadeIntoGap(Guard(Respawn));

        void Slip()
        {
            state.Enter(PlayerState.Slipping);
            animations.SlipBy(facing.BackwardsDirection() * settings.SlipDistanceInKeys * board.KeyWorldSize, Guard(Respawn));
        }

        void BumpInto(Key wall)
        {
            var to = WorldOf(wall);
            var home = WorldOf(currentKey);
            state.Enter(PlayerState.BumpingWall);
            FaceTowards(to);
            ResetCycles();
            ShowCurrentPose();
            animations.BumpWall(home + DirectionToWall(to) * settings.BumpDistanceInKeys * board.KeyWorldSize, home, Guard(RecoverFromBump));
        }

        void RecoverFromBump() => state.Enter(PlayerState.Idle);

        void ShowCurrentPose()
        {
            if (level.KindOf(currentKey) == CellKind.Gutter) ShowClimbPose();
            else ShowWalkPose();
        }

        void BreatheWhileIdle(PlayerState next)
        {
            if (next == PlayerState.Idle) animations.StartBreathing();
            else animations.StopBreathing();
        }

        Vector2 DirectionToWall(Vector2 wallWorld)
        {
            var direction = wallWorld - WorldOf(currentKey);
            if (direction.sqrMagnitude > 0f) return direction.normalized;
            return facing.ForwardDirection();
        }

        void Respawn()
        {
            Failed?.Invoke();
            state.Enter(PlayerState.Respawning);
            ResetToStart();
            animations.DropFromTop(WorldOf(startKey), settings.DropMarginInKeys * board.KeyWorldSize, Guard(() => state.Enter(PlayerState.Idle)));
        }

        void ResetToStart()
        {
            ResetProgress();
            ShowStartPose();
        }

        void ResetProgress()
        {
            currentKey = startKey;
            facing.ResetToDefault();
            ResetCycles();
        }

        void ResetCycles()
        {
            walk.Reset();
            climb.Reset();
        }

        void ShowStartPose()
        {
            ShowWalkPose();
            presentation.SetFacing(facing.Current);
        }

        void FaceTowards(Vector2 to)
        {
            facing.ResolveMove(WorldOf(currentKey), to);
            presentation.SetFacing(facing.Current);
        }

        void Arrive(Key target)
        {
            currentKey = target;
            Moved?.Invoke(target);
        }

        Action Guard(Action action)
        {
            int token = generation;
            return () =>
            {
                if (token == generation) action();
            };
        }

        Vector2 WorldOf(Key key)
        {
            if (!board.TryGetPosition(key, out var world)) throw new InvalidOperationException($"The board has no position for key {key}.");
            return world;
        }
    }
}
