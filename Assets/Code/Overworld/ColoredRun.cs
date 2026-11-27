namespace DetectiveDave.OverworldPlay
{
    public readonly struct ColoredRun
    {
        public ColoredRun(string text, string colorHex)
        {
            Text = text;
            ColorHex = colorHex;
        }

        public string Text { get; }

        public string ColorHex { get; }
    }
}
