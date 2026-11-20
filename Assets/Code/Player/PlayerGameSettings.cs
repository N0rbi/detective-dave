namespace DetectiveDave.PlayerPlay
{
    public readonly struct PlayerGameSettings
    {
        public int WalkFrameCount { get; }

        public int ClimbFrameCount { get; }

        public float SlipDistanceInKeys { get; }

        public float DropMarginInKeys { get; }

        public float BumpDistanceInKeys { get; }

        public PlayerGameSettings(int walkFrameCount, int climbFrameCount, float slipDistanceInKeys, float dropMarginInKeys, float bumpDistanceInKeys)
        {
            WalkFrameCount = walkFrameCount;
            ClimbFrameCount = climbFrameCount;
            SlipDistanceInKeys = slipDistanceInKeys;
            DropMarginInKeys = dropMarginInKeys;
            BumpDistanceInKeys = bumpDistanceInKeys;
        }
    }
}
