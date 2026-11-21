using System;
using System.Text;

namespace DetectiveDave.OverworldPlay
{
    public sealed class LetterPaperModel
    {
        const string ColorOpen = "<color=#";
        const string ColorMiddle = ">";
        const string ColorClose = "</color>";

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

        public string Render(int typedCount, LetterPath path, string typedColorHex, string pendingColorHex)
        {
            var typed = TypedMask(typedCount, path);
            var text = new StringBuilder();
            int runStart = 0;
            for (int position = 1; position <= plain.Length; position++)
            {
                if (position < plain.Length && typed[position] == typed[runStart]) continue;
                AppendRun(text, plain.Substring(runStart, position - runStart), typed[runStart] ? typedColorHex : pendingColorHex);
                runStart = position;
            }
            return text.ToString();
        }

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
