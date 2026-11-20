using System.Text;

namespace DetectiveDave.OverworldPlay
{
    public sealed class FindingsPaperModel
    {
        const string ColorOpen = "<color=#";
        const string ColorMiddle = ">";
        const string ColorClose = "</color>";

        readonly string plain;

        public FindingsPaperModel(string text, int lettersPerLine)
        {
            plain = TextWrap.Wrap(text, lettersPerLine);
        }

        public string Plain => plain;

        public string Render(int revealedCount, string typedColorHex, string pendingColorHex)
        {
            int split = System.Math.Max(System.Math.Min(revealedCount, plain.Length), 0);
            var text = new StringBuilder();
            AppendRun(text, plain.Substring(0, split), typedColorHex);
            AppendRun(text, plain.Substring(split), pendingColorHex);
            return text.ToString();
        }

        static void AppendRun(StringBuilder text, string run, string colorHex)
        {
            if (run.Length == 0) return;
            text.Append(ColorOpen).Append(colorHex).Append(ColorMiddle).Append(run).Append(ColorClose);
        }
    }
}
