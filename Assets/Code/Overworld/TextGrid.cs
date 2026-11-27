namespace DetectiveDave.OverworldPlay
{
    public readonly struct TextGrid
    {
        public TextGrid(float lineHeight, float lineHeightRatio, float advanceRatio)
        {
            LineHeight = lineHeight;
            LineHeightRatio = lineHeightRatio;
            AdvanceRatio = advanceRatio;
        }

        public float LineHeight { get; }

        public float LineHeightRatio { get; }

        public float AdvanceRatio { get; }

        public float LineStep => LineHeight * LineHeightRatio;

        public float LetterWidth => LineHeight * AdvanceRatio;
    }
}
