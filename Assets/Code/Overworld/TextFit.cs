using System.Collections.Generic;
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
        public static TextFitResult Fit(string text, Vector2 area, float maxLineHeight, float minLineHeight, float step, float advanceRatio, float lineHeightRatio) =>
            Fit(new[] { new TextBlock(text, 1f) }, area, maxLineHeight, minLineHeight, step, advanceRatio, lineHeightRatio);

        public static TextFitResult Fit(IReadOnlyList<TextBlock> blocks, Vector2 area, float maxLineHeight, float minLineHeight, float step, float advanceRatio, float lineHeightRatio)
        {
            float lineHeight = Mathf.Max(maxLineHeight, minLineHeight);
            float smallest = Mathf.Min(maxLineHeight, minLineHeight);
            float decrement = Mathf.Max(step, 0.0001f);
            while (true)
            {
                bool fits = Fits(blocks, area, lineHeight, advanceRatio, lineHeightRatio);
                if (fits || lineHeight <= smallest) return new TextFitResult(lineHeight, LettersPerLine(area.x, lineHeight, advanceRatio), fits);
                lineHeight = Mathf.Max(lineHeight - decrement, smallest);
            }
        }

        static bool Fits(IReadOnlyList<TextBlock> blocks, Vector2 area, float lineHeight, float advanceRatio, float lineHeightRatio)
        {
            float height = 0f;
            bool anyBlockBefore = false;
            foreach (var block in blocks)
            {
                if (string.IsNullOrEmpty(block.Text)) continue;
                float size = lineHeight * block.SizeShare;
                int letters = LettersPerLine(area.x, size, advanceRatio);
                if (LongestWord(block.Text) > letters) return false;
                if (anyBlockBefore) height += lineHeight * lineHeightRatio;
                height += LineCount(TextWrap.Wrap(block.Text, letters)) * size * lineHeightRatio;
                anyBlockBefore = true;
            }
            return height <= area.y;
        }

        public static int LongestWord(string text)
        {
            int longest = 0;
            int current = 0;
            foreach (char character in text)
            {
                if (character == ' ' || character == '\n') current = 0;
                else current++;
                if (current > longest) longest = current;
            }
            return longest;
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
