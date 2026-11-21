using System.Text;

namespace DetectiveDave.OverworldPlay
{
    public sealed class ResponsePaperModel
    {
        const string ColorOpen = "<color=#";
        const string ColorMiddle = ">";
        const string ColorClose = "</color>";
        const string InvisibleColorHex = "00000000";

        readonly string plain;

        public ResponsePaperModel(string text, int lettersPerLine)
        {
            plain = TextWrap.Wrap(text, lettersPerLine);
        }

        public string Plain => plain;

        public string RenderTyping(int typedCount, int currentUnitEnd, string typedColorHex, string pendingColorHex)
        {
            var display = StoryDisplay.Of(plain, typedCount, currentUnitEnd);
            var text = new StringBuilder();
            AppendRun(text, display.Typed, typedColorHex);
            AppendRun(text, display.Pending, pendingColorHex);
            AppendRun(text, display.Hidden, InvisibleColorHex);
            return text.ToString();
        }

        public string RenderReview(string typedColorHex)
        {
            var text = new StringBuilder();
            AppendRun(text, plain, typedColorHex);
            return text.ToString();
        }

        static void AppendRun(StringBuilder text, string run, string colorHex)
        {
            if (run.Length == 0) return;
            text.Append(ColorOpen).Append(colorHex).Append(ColorMiddle).Append(run).Append(ColorClose);
        }
    }
}
