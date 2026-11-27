namespace DetectiveDave.OverworldPlay
{
    public readonly struct StoryDisplay
    {
        public StoryDisplay(string typed, string pending, string hidden)
        {
            Typed = typed;
            Pending = pending;
            Hidden = hidden;
        }

        public string Typed { get; }

        public string Pending { get; }

        public string Hidden { get; }

        public static StoryDisplay Of(string text, int typedCount, int currentUnitEnd)
        {
            int typedEnd = Clamp(typedCount, 0, text.Length);
            int pendingEnd = Clamp(currentUnitEnd, typedEnd, text.Length);
            return new StoryDisplay(text.Substring(0, typedEnd), text.Substring(typedEnd, pendingEnd - typedEnd), text.Substring(pendingEnd));
        }

        static int Clamp(int value, int minimum, int maximum) => System.Math.Max(minimum, System.Math.Min(value, maximum));
    }
}
