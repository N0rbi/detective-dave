using System;
using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    public sealed class GuardBrain
    {
        readonly IKeyboardBoard board;
        readonly KeyPathfinder pathfinder;
        readonly IReadOnlyList<Key> route;
        readonly GuardBrainSettings settings;

        int routeIndex;
        int direction = 1;
        float waitRemaining;
        bool moving;
        Key arrivalKey;
        float movingRemaining;
        float movingTotal;

        public GuardBrain(IKeyboardBoard board, KeyPathfinder pathfinder, IReadOnlyList<Key> route, GuardBrainSettings settings)
        {
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.pathfinder = pathfinder ?? throw new ArgumentNullException(nameof(pathfinder));
            if (route == null || route.Count == 0) throw new ArgumentException("A guard needs at least its start key.", nameof(route));
            this.route = route;
            this.settings = settings;
            Current = route[0];
            Mode = GuardMode.WaitingToStart;
            waitRemaining = settings.WaitBeforeMoveSeconds;
        }

        public Key Current { get; private set; }

        public GuardMode Mode { get; private set; }

        public bool IsMoving => moving;

        public GuardAction Tick(float deltaSeconds, Key playerKey)
        {
            if (Mode == GuardMode.Catching) return GuardAction.Stay();
            if (!ArriveIfDone(deltaSeconds)) return TickMidMove(playerKey);

            if (Current == playerKey) return CatchPlayer();
            if (ShouldStartChase(playerKey)) Mode = GuardMode.Chasing;
            if (Mode == GuardMode.Chasing) return Chase(playerKey);
            if (Mode == GuardMode.Rejoining) return Rejoin(deltaSeconds);
            return Patrol(deltaSeconds);
        }

        bool ArriveIfDone(float deltaSeconds)
        {
            if (!moving) return true;
            movingRemaining -= deltaSeconds;
            if (movingRemaining > 0f) return false;
            moving = false;
            Current = arrivalKey;
            return true;
        }

        GuardAction TickMidMove(Key playerKey)
        {
            if (playerKey == Current) return CatchPlayer();
            if (!ShouldStartChaseFromOccupiedKey(playerKey)) return GuardAction.Stay();
            Mode = GuardMode.Chasing;
            return ContinueStepAtChaseSpeed();
        }

        bool ShouldStartChaseFromOccupiedKey(Key playerKey)
        {
            if (Mode == GuardMode.Chasing) return false;
            return board.IsNeighbour(Current, playerKey) && pathfinder.HasPath(Current, playerKey);
        }

        float MovingProgress() => movingTotal > 0f ? 1f - movingRemaining / movingTotal : 1f;

        GuardAction ContinueStepAtChaseSpeed()
        {
            float seconds = (1f - MovingProgress()) * settings.ChaseSecondsPerKey;
            movingRemaining = seconds;
            movingTotal = seconds;
            return GuardAction.Redirect(arrivalKey, seconds, true);
        }

        bool ShouldStartChase(Key playerKey)
        {
            if (Mode == GuardMode.Chasing) return false;
            return board.IsNeighbour(Current, playerKey) && pathfinder.HasPath(Current, playerKey);
        }

        GuardAction CatchPlayer()
        {
            Mode = GuardMode.Catching;
            return GuardAction.Catch();
        }

        GuardAction Chase(Key playerKey)
        {
            var path = pathfinder.FindPath(Current, playerKey);
            if (path.Count < 2)
            {
                Mode = GuardMode.Rejoining;
                return Rejoin(0f);
            }
            return Step(path[1], settings.ChaseSecondsPerKey, true);
        }

        GuardAction Rejoin(float deltaSeconds)
        {
            int nearest = NearestRouteIndex();
            if (nearest < 0) return GuardAction.Stay();
            if (route[nearest] == Current)
            {
                routeIndex = nearest;
                Mode = GuardMode.Patrolling;
                return Patrol(deltaSeconds);
            }
            var path = pathfinder.FindPath(Current, route[nearest]);
            return Step(path[1], settings.PatrolSecondsPerKey, false);
        }

        int NearestRouteIndex()
        {
            int best = -1;
            int bestLength = int.MaxValue;
            for (int i = 0; i < route.Count; i++)
            {
                int length = pathfinder.FindPath(Current, route[i]).Count;
                if (length == 0 || length >= bestLength) continue;
                best = i;
                bestLength = length;
            }
            return best;
        }

        GuardAction Patrol(float deltaSeconds)
        {
            if (Mode == GuardMode.WaitingToStart || Mode == GuardMode.WaitingAtEnd) return WaitThenGo(deltaSeconds);

            int next = routeIndex + direction;
            if (next >= 0 && next < route.Count)
            {
                routeIndex = next;
                return Step(route[next], settings.PatrolSecondsPerKey, false);
            }
            BeginWaitAtTurn();
            return GuardAction.Stay();
        }

        GuardAction WaitThenGo(float deltaSeconds)
        {
            waitRemaining -= deltaSeconds;
            if (waitRemaining > 0f) return GuardAction.Stay();
            Mode = GuardMode.Patrolling;
            return Patrol(0f);
        }

        void BeginWaitAtTurn()
        {
            bool atEnd = direction > 0;
            direction = atEnd ? -1 : 1;
            Mode = atEnd ? GuardMode.WaitingAtEnd : GuardMode.WaitingToStart;
            waitRemaining = atEnd ? settings.WaitAtEndSeconds : settings.WaitBeforeMoveSeconds;
        }

        GuardAction Step(Key target, float seconds, bool chasing)
        {
            moving = true;
            arrivalKey = target;
            movingRemaining = seconds;
            movingTotal = seconds;
            return GuardAction.StepTo(target, seconds, chasing);
        }
    }
}
