namespace DetectiveDave.PlayerPlay
{
    public readonly struct PlayerGameSettings
    {
        public int WalkFrameCount { get; }

        public int ClimbFrameCount { get; }

        public float BumpDistanceInKeys { get; }

        public bool ForgivingMoves { get; }

        public bool WrongKeyStunEnabled { get; }

        public PlayerGameSettings(int walkFrameCount, int climbFrameCount, float bumpDistanceInKeys, bool forgivingMoves = false, bool wrongKeyStunEnabled = true)
        {
            WalkFrameCount = walkFrameCount;
            ClimbFrameCount = climbFrameCount;
            BumpDistanceInKeys = bumpDistanceInKeys;
            ForgivingMoves = forgivingMoves;
            WrongKeyStunEnabled = wrongKeyStunEnabled;
        }
    }
}
