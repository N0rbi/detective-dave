using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class PaperReveal
    {
        public const string Separator = "\n\n";

        readonly string settled;
        readonly string revealed;

        public PaperReveal(string settledText, string revealedText)
        {
            settled = settledText ?? string.Empty;
            revealed = revealedText ?? string.Empty;
        }

        public int RevealedLength => revealed.Length;

        public string Full => settled + Divider + revealed;

        public string Shown(int revealedCount) => settled + Divider + revealed.Substring(0, Math.Max(0, Math.Min(revealedCount, revealed.Length)));

        string Divider => settled.Length > 0 && revealed.Length > 0 ? Separator : string.Empty;
    }
}
