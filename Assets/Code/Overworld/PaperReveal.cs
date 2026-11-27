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

        public string Shown(int revealedCount) => settled + Divider + revealed.Substring(0, Clamped(revealedCount));

        public FontSlot[] Slots(int revealedCount, TextRole settledRole, TextRole revealedRole)
        {
            int settledLength = settled.Length + Divider.Length;
            var slots = new FontSlot[settledLength + Clamped(revealedCount)];
            FontRoles.FillRange(slots, 0, settledLength, settledRole);
            FontRoles.FillRange(slots, settledLength, slots.Length - settledLength, revealedRole);
            return slots;
        }

        int Clamped(int revealedCount) => Math.Max(0, Math.Min(revealedCount, revealed.Length));

        string Divider => settled.Length > 0 && revealed.Length > 0 ? Separator : string.Empty;
    }
}
