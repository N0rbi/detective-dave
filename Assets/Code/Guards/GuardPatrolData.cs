using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    public readonly struct GuardPatrolData
    {
        public Key StartKey { get; }

        public Key EndKey { get; }

        public float WaitBeforeMoveSeconds { get; }

        public float WaitAtEndSeconds { get; }

        public float PatrolSecondsPerKey { get; }

        public float ChaseSecondsPerKey { get; }

        public GuardPatrolData(Key startKey, Key endKey, float waitBeforeMoveSeconds, float waitAtEndSeconds, float patrolSecondsPerKey, float chaseSecondsPerKey)
        {
            StartKey = startKey;
            EndKey = endKey;
            WaitBeforeMoveSeconds = waitBeforeMoveSeconds;
            WaitAtEndSeconds = waitAtEndSeconds;
            PatrolSecondsPerKey = patrolSecondsPerKey;
            ChaseSecondsPerKey = chaseSecondsPerKey;
        }
    }
}
