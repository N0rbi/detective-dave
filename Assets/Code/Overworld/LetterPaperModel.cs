using System;
using System.Text;

namespace DetectiveDave.OverworldPlay
{
    public sealed class LetterPaperModel
    {
        const string ColorOpen = "<color=#";
        const string ColorMiddle = ">";
        const string ColorClose = "</color>";

        enum Line
        {
            Head,
            Accept,
            Decline,
            Tail
        }

        readonly int headLength;
        readonly int acceptLength;
        readonly int declineLength;
        readonly int divergence;
        readonly string plain;

        public LetterPaperModel(string client, string acceptMessage, string declineMessage, int lettersPerLine)
        {
            var accept = acceptMessage ?? string.Empty;
            var decline = declineMessage ?? string.Empty;
            headLength = LetterText.Head(client).Length;
            acceptLength = accept.Length;
            declineLength = decline.Length;
            divergence = DivergingTyper.DivergenceIndex(LetterText.Compose(client, accept), LetterText.Compose(client, decline));
            plain = TextWrap.Wrap(LetterText.Head(client) + accept + "\n" + decline + LetterText.Tail, lettersPerLine);
        }

        public string Plain => plain;

        public static string Unwrapped(string client, string acceptMessage, string declineMessage) =>
            LetterText.Head(client) + (acceptMessage ?? string.Empty) + "\n" + (declineMessage ?? string.Empty) + LetterText.Tail;

        public string Render(int typedCount, LetterPath path, LetterPalette palette, float unchosenAlpha)
        {
            var typed = TypedMask(typedCount, path);
            var alphaHex = AlphaHex(unchosenAlpha);
            var text = new StringBuilder();
            int runStart = 0;
            string runColor = ColorAt(0, typed[0], path, palette, alphaHex);
            for (int position = 1; position <= plain.Length; position++)
            {
                string color = position < plain.Length ? ColorAt(position, typed[position], path, palette, alphaHex) : null;
                if (color == runColor) continue;
                AppendRun(text, plain.Substring(runStart, position - runStart), runColor);
                runStart = position;
                runColor = color;
            }
            return text.ToString();
        }

        string ColorAt(int position, bool isTyped, LetterPath path, LetterPalette palette, string alphaHex)
        {
            var line = LineAt(position);
            var color = isTyped ? palette.TypedHex : PendingHex(line, path, palette);
            return IsUnchosen(line, path) ? color + alphaHex : color;
        }

        Line LineAt(int position)
        {
            if (position < headLength) return Line.Head;
            if (position < headLength + acceptLength) return Line.Accept;
            if (position <= headLength + acceptLength) return Line.Head;
            if (position < headLength + acceptLength + 1 + declineLength) return Line.Decline;
            return Line.Tail;
        }

        static string PendingHex(Line line, LetterPath path, LetterPalette palette)
        {
            if (path != LetterPath.Undecided) return palette.PendingHex;
            if (line == Line.Accept) return palette.AcceptPendingHex;
            if (line == Line.Decline) return palette.DeclinePendingHex;
            return palette.PendingHex;
        }

        static bool IsUnchosen(Line line, LetterPath path) =>
            (line == Line.Accept && path == LetterPath.Decline) || (line == Line.Decline && path == LetterPath.Accept);

        static string AlphaHex(float alpha) => ((int)Math.Round(Math.Max(0f, Math.Min(1f, alpha)) * 255f)).ToString("X2");

        bool[] TypedMask(int typedCount, LetterPath path)
        {
            var mask = new bool[plain.Length];
            int acceptTyped = path == LetterPath.Decline ? Math.Min(typedCount, divergence) : typedCount;
            int declineTyped = path == LetterPath.Accept ? Math.Min(typedCount, divergence) : typedCount;
            int acceptStart = headLength;
            int declineStart = headLength + acceptLength + 1;
            int tailStart = declineStart + declineLength;
            Mark(mask, 0, headLength, typedCount);
            Mark(mask, acceptStart, acceptLength, acceptTyped - headLength);
            Mark(mask, declineStart, declineLength, declineTyped - headLength);
            Mark(mask, tailStart, plain.Length - tailStart, TailTyped(typedCount, path));
            return mask;
        }

        int TailTyped(int typedCount, LetterPath path)
        {
            if (path == LetterPath.Accept) return typedCount - headLength - acceptLength;
            if (path == LetterPath.Decline) return typedCount - headLength - declineLength;
            return 0;
        }

        static void Mark(bool[] mask, int start, int length, int typedLength)
        {
            int count = Math.Min(Math.Min(typedLength, length), mask.Length - start);
            for (int offset = 0; offset < count; offset++) mask[start + offset] = true;
        }

        static void AppendRun(StringBuilder text, string run, string colorHex) =>
            text.Append(ColorOpen).Append(colorHex).Append(ColorMiddle).Append(run).Append(ColorClose);
    }
}
