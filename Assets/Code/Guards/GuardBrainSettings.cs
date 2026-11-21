namespace DetectiveDave.GuardPlay
{
    public readonly struct GuardBrainSettings
    {
        public float WaitBeforeMoveSeconds { get; }

        public float WaitAtEndSeconds { get; }

        public float PatrolSecondsPerKey { get; }

        public float ChaseSecondsPerKey { get; }

        public GuardBrainSettings(float waitBeforeMoveSeconds, float waitAtEndSeconds, float patrolSecondsPerKey, float chaseSecondsPerKey)
        {
            WaitBeforeMoveSeconds = waitBeforeMoveSeconds;
            WaitAtEndSeconds = waitAtEndSeconds;
            PatrolSecondsPerKey = patrolSecondsPerKey;
            ChaseSecondsPerKey = chaseSecondsPerKey;
        }
    }
}
