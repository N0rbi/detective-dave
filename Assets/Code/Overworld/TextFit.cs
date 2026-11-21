using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public readonly struct TextFitResult
    {
        public float LineHeight { get; }
        public int LettersPerLine { get; }
        public bool Fits { get; }

        public TextFitResult(float lineHeight, int lettersPerLine, bool fits)
        {
            LineHeight = lineHeight;
            LettersPerLine = lettersPerLine;
            Fits = fits;
        }
    }

    public static class TextFit
    {
        public static TextFitResult Fit(string text, Vector2 area, float maxLineHeight, float minLineHeight, float step, float advanceRatio, float lineHeightRatio)
        {
            float lineHeight = Mathf.Max(maxLineHeight, minLineHeight);
            float smallest = Mathf.Min(maxLineHeight, minLineHeight);
            float decrement = Mathf.Max(step, 0.0001f);
            while (true)
            {
                int letters = LettersPerLine(area.x, lineHeight, advanceRatio);
                bool fits = LineCount(TextWrap.Wrap(text, letters)) * lineHeight * lineHeightRatio <= area.y;
                if (fits || lineHeight <= smallest) return new TextFitResult(lineHeight, letters, fits);
                lineHeight = Mathf.Max(lineHeight - decrement, smallest);
            }
        }

        public static int LettersPerLine(float width, float lineHeight, float advanceRatio)
        {
            float letterWidth = Mathf.Max(lineHeight * advanceRatio, 0.0001f);
            return Mathf.Max(Mathf.FloorToInt(width / letterWidth), 1);
        }

        public static int LineCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int lines = 1;
            foreach (char character in text)
            {
                if (character == '\n') lines++;
            }
            return lines;
        }
    }
}
