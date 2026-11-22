namespace DetectiveDave.MusicPlay
{
    public readonly struct ChordMoment
    {
        public const int NoSegment = -1;

        public ChordMoment(int segmentIndex, int beatInBar, bool isStrongBeat, bool isChordStart)
        {
            SegmentIndex = segmentIndex;
            BeatInBar = beatInBar;
            IsStrongBeat = isStrongBeat;
            IsChordStart = isChordStart;
        }

        public static ChordMoment Outside => new ChordMoment(NoSegment, 0, false, false);

        public int SegmentIndex { get; }

        public int BeatInBar { get; }

        public bool IsStrongBeat { get; }

        public bool IsChordStart { get; }

        public bool IsInsideChart => SegmentIndex != NoSegment;

        public bool FavoursChordTones => IsStrongBeat || IsChordStart;
    }
}
