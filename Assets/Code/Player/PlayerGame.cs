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
        readonly IDashGust gust;
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
        bool climbPoseShown;

        public PlayerGame(IKeyboardBoard board, IPlayerLevel level, IPlayerAnimations animations, IPlayerPresentation presentation, IDashGust gust, PlayerGameSettings settings)
        {
            this.board = board;
            this.level = level;
            this.animations = animations;
            this.presentation = presentation;
            this.gust = gust;
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

        public Key CurrentKey => currentKey;

        public bool IsActive { get; private set; }

        public bool EndsMissing { get; private set; }

        public void Restart()
        {
            generation++;
            animations.CancelAllWithoutCallbacks();
            gust.Clear();
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
            gust.Clear();
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
            if (!level.TryGetEnds(out startKey, out goalKey) || !board.TryGetPosition(startKey, out startWorld)) return false;
            if (goalKey == Key.None)
            {
                goalWorld = startWorld;
                return true;
            }
            return board.TryGetPosition(goalKey, out goalWorld);
        }

        void PerformOutcome(MoveOutcome outcome, Key target)
        {
            if (outcome == MoveOutcome.Slip && settings.ForgivingMoves) return;
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
            gust.Show(WorldOf(currentKey), to, climbPoseShown);
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
            climbPoseShown = false;
            presentation.SetClimbing(false);
            presentation.SetFrame(walk.Frame);
        }

        void ShowClimbPose()
        {
            climbPoseShown = true;
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
            var home = WorldOf(currentKey);
            var offset = facing.BackwardsDirection() * settings.SlipDistanceInKeys * board.KeyWorldSize;
            state.Enter(PlayerState.Slipping);
            gust.Show(home, home + offset, climbPoseShown);
            animations.SlipBy(offset, Guard(Respawn));
        }

        void BumpInto(Key wall)
        {
            var to = WorldOf(wall);
            var home = WorldOf(currentKey);
            state.Enter(PlayerState.BumpingWall);
            FaceTowards(to);
            ResetCycles();
            ShowCurrentPose();
            var leanTarget = home + DirectionToWall(to) * settings.BumpDistanceInKeys * board.KeyWorldSize;
            gust.Show(home, leanTarget, climbPoseShown);
            animations.BumpWall(leanTarget, home, Guard(() => gust.Show(leanTarget, home, climbPoseShown)), Guard(RecoverFromBump));
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

        public void GetCaught()
        {
            if (!IsActive || !IsVulnerable) return;
            generation++;
            animations.CancelAllWithoutCallbacks();
            gust.Clear();
            state.Enter(PlayerState.Caught);
            animations.FallForward(facing.ForwardDirection(), Guard(Respawn));
        }

        public void CompleteRespawn()
        {
            if (!IsActive || state.State != PlayerState.Respawning) return;
            ResetToStart();
            animations.ShowResetAt(WorldOf(startKey));
            state.Enter(PlayerState.Idle);
        }

        public bool IsVulnerable => state.State == PlayerState.Idle || state.State == PlayerState.Moving || state.State == PlayerState.Slipping || state.State == PlayerState.BumpingWall;

        void Respawn()
        {
            state.Enter(PlayerState.Respawning);
            Failed?.Invoke();
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
