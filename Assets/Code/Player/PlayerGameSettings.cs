namespace DetectiveDave.PlayerPlay
{
    public readonly struct PlayerGameSettings
    {
        public int WalkFrameCount { get; }

        public int ClimbFrameCount { get; }

        public float SlipDistanceInKeys { get; }

        public float BumpDistanceInKeys { get; }

        public bool ForgivingMoves { get; }

        public PlayerGameSettings(int walkFrameCount, int climbFrameCount, float slipDistanceInKeys, float bumpDistanceInKeys, bool forgivingMoves = false)
        {
            WalkFrameCount = walkFrameCount;
            ClimbFrameCount = climbFrameCount;
            SlipDistanceInKeys = slipDistanceInKeys;
            BumpDistanceInKeys = bumpDistanceInKeys;
            ForgivingMoves = forgivingMoves;
        }
    }
}
