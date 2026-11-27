using System;
using System.Collections.Generic;

namespace DetectiveDave.OverworldPlay
{
    public static class GlyphPlan
    {
        public static List<Glyph> Build(IReadOnlyList<ColoredRun> runs, FontSlot[] slots)
        {
            if (VisibleLength(runs) != slots.Length) throw new ArgumentException("Every visible character needs a font slot");
            var glyphs = new List<Glyph>();
            int row = 0;
            int column = 0;
            int index = 0;
            foreach (var run in runs)
            {
                foreach (char character in run.Text)
                {
                    if (character == '\n')
                    {
                        row++;
                        column = 0;
                    }
                    else
                    {
                        if (!char.IsWhiteSpace(character)) glyphs.Add(new Glyph(character, slots[index], run.ColorHex, row, column));
                        column++;
                    }
                    index++;
                }
            }
            return glyphs;
        }

        static int VisibleLength(IReadOnlyList<ColoredRun> runs)
        {
            int length = 0;
            foreach (var run in runs) length += run.Text.Length;
            return length;
        }
    }
}
