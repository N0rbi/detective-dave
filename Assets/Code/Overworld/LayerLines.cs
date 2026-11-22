using System;
using System.Collections.Generic;
using System.Text;

namespace DetectiveDave.OverworldPlay
{
    public static class LayerLines
    {
        const string InvisibleOpen = "<color=#00000000>";
        const string ColorOpen = "<color=#";
        const string ColorClose = "</color>";

        public static string[] Build(IReadOnlyList<ColoredRun> runs, FontSlot[] slots, FontSlot layer)
        {
            if (VisibleLength(runs) != slots.Length) throw new ArgumentException("Every visible character needs a font slot");
            var writer = new LineWriter();
            int index = 0;
            foreach (var run in runs)
            {
                foreach (char character in run.Text)
                {
                    writer.Add(character, run.ColorHex, slots[index] == layer);
                    index++;
                }
            }
            return writer.Finish();
        }

        static int VisibleLength(IReadOnlyList<ColoredRun> runs)
        {
            int length = 0;
            foreach (var run in runs) length += run.Text.Length;
            return length;
        }

        sealed class LineWriter
        {
            readonly List<string> lines = new List<string>();
            readonly StringBuilder line = new StringBuilder();
            readonly StringBuilder segment = new StringBuilder();
            string segmentColor;
            bool segmentOwned;
            bool lineOwns;

            public void Add(char character, string colorHex, bool owned)
            {
                if (character == '\n')
                {
                    EndLine();
                    return;
                }
                if (segment.Length > 0 && (segmentColor != colorHex || segmentOwned != owned)) FlushSegment();
                segmentColor = colorHex;
                segmentOwned = owned;
                segment.Append(character);
                lineOwns |= owned;
            }

            public string[] Finish()
            {
                EndLine();
                return lines.ToArray();
            }

            void EndLine()
            {
                FlushSegment();
                lines.Add(lineOwns ? line.ToString() : null);
                line.Clear();
                lineOwns = false;
            }

            void FlushSegment()
            {
                if (segment.Length == 0) return;
                if (!segmentOwned) line.Append(InvisibleOpen).Append(segment).Append(ColorClose);
                else if (segmentColor == null) line.Append(segment);
                else line.Append(ColorOpen).Append(segmentColor).Append('>').Append(segment).Append(ColorClose);
                segment.Clear();
            }
        }
    }
}
