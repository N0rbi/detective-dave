using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.EditorTools
{
    public static class ChordTimelineLayout
    {
        public static (float Start, float Length) SpanOf(float startBeat, float endBeat, float loopBeats)
        {
            var start = Clamp01(startBeat / loopBeats);
            var end = Clamp01(endBeat / loopBeats);
            return (start, Math.Max(end - start, 0f));
        }

        public static IReadOnlyList<float> BarLines(int bars) =>
            Enumerable.Range(0, bars + 1).Select(bar => bar / (float)bars).ToList();

        static float Clamp01(float value) => Math.Min(Math.Max(value, 0f), 1f);
    }
}
