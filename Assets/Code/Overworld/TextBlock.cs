namespace DetectiveDave.OverworldPlay
{
    public readonly struct TextBlock
    {
        public TextBlock(string text, float sizeShare)
        {
            Text = text ?? string.Empty;
            SizeShare = sizeShare;
        }

        public string Text { get; }

        public float SizeShare { get; }
    }
}
