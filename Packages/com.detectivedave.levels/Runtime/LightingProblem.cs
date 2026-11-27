namespace DetectiveDave.Levels
{
    public enum LightingProblemKind
    {
        FlickerWithoutPatterns,
        NoPatternCanBePicked,
        WaitRangeReversed,
        PatternWithoutDuration,
        PatternWithoutPoints,
        PointAfterTheEnd,
        LightsOff
    }

    public readonly struct LightingProblem
    {
        public LightingProblemKind Kind { get; }
        public bool IsError { get; }
        public string Message { get; }
        public int PatternIndex { get; }

        public LightingProblem(LightingProblemKind kind, bool isError, string message, int patternIndex = -1)
        {
            Kind = kind;
            IsError = isError;
            Message = message;
            PatternIndex = patternIndex;
        }
    }
}
