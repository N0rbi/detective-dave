using System;
using System.Collections.Generic;

namespace DetectiveDave.OverworldPlay
{
    public static class RichRuns
    {
        const string ColorOpen = "<color=#";
        const string ColorClose = "</color>";

        public static List<ColoredRun> Parse(string richText)
        {
            var runs = new List<ColoredRun>();
            int position = 0;
            while (position < richText.Length)
            {
                int open = richText.IndexOf(ColorOpen, position, StringComparison.Ordinal);
                if (open < 0)
                {
                    runs.Add(new ColoredRun(richText.Substring(position), null));
                    break;
                }
                if (open > position) runs.Add(new ColoredRun(richText.Substring(position, open - position), null));
                position = AppendColored(runs, richText, open);
            }
            return runs;
        }

        static int AppendColored(List<ColoredRun> runs, string richText, int open)
        {
            int colorStart = open + ColorOpen.Length;
            int colorEnd = richText.IndexOf('>', colorStart);
            if (colorEnd < 0) throw new FormatException("Colour tag is not closed: " + richText);
            int close = richText.IndexOf(ColorClose, colorEnd + 1, StringComparison.Ordinal);
            if (close < 0) throw new FormatException("Colour run has no closing tag: " + richText);
            runs.Add(new ColoredRun(richText.Substring(colorEnd + 1, close - colorEnd - 1), richText.Substring(colorStart, colorEnd - colorStart)));
            return close + ColorClose.Length;
        }
    }
}
