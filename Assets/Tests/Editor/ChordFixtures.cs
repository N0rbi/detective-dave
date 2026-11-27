using DetectiveDave.MusicPlay;

namespace DetectiveDave.Tests
{
    static class ChordFixtures
    {
        public const float TempoBpm = 75f;
        public const int BeatsPerBar = 4;
        public const int LoopLengthBars = 8;
        public const float LoopSeconds = 25.6f;

        static readonly int[] AmChordTones = { 9, 0, 4, 7 };
        static readonly int[] AmScale = { 9, 11, 0, 2, 4, 7 };
        static readonly int[] DmChordTones = { 2, 5, 9, 0 };
        static readonly int[] DmScale = { 2, 4, 5, 7, 9, 11, 0 };
        static readonly int[] BdimChordTones = { 11, 2, 5, 9 };
        static readonly int[] BdimScale = { 11, 0, 2, 4, 5, 7, 9 };
        static readonly int[] EChordTones = { 4, 8, 11, 2 };
        static readonly int[] EScale = { 4, 5, 8, 9, 11, 0, 2 };

        public static ChordSegment[] Segments() => new[]
        {
            new ChordSegment("Am7", 0f, 4f, AmChordTones, AmScale),
            new ChordSegment("Dm7", 4f, 4f, DmChordTones, DmScale),
            new ChordSegment("Bm7b5", 8f, 4f, BdimChordTones, BdimScale),
            new ChordSegment("E7", 12f, 4f, EChordTones, EScale),
            new ChordSegment("Am7", 16f, 4f, AmChordTones, AmScale),
            new ChordSegment("Dm7", 20f, 4f, DmChordTones, DmScale),
            new ChordSegment("Bm7b5", 24f, 4f, BdimChordTones, BdimScale),
            new ChordSegment("E7", 28f, 4f, EChordTones, EScale)
        };

        public static int[][] ClipNotes() => new[]
        {
            new[] { 0, 4, 7 },
            new[] { 5, 9, 0 },
            new[] { 5, 9, 0 },
            new[] { 2, 5, 9 },
            new[] { 2, 8, 11 },
            new[] { 2, 8, 11 },
            new[] { 0, 4, 7 },
            new[] { 5, 9, 0 },
            new[] { 5, 9, 0 },
            new[] { 2, 5, 9 },
            new[] { 2, 8, 11 },
            new[] { 2, 8, 11 }
        };

        public static PitchClassSet[] ClipPitches() => System.Array.ConvertAll(ClipNotes(), PitchClassSet.Of);

        public static ChordClock Clock(float chartBeatAtLoopStart = 0f) => new ChordClock(TempoBpm, BeatsPerBar, LoopLengthBars, chartBeatAtLoopStart, Segments());

        public static ChordPalette Palette() => new ChordPalette(Segments(), ClipPitches());
    }
}
