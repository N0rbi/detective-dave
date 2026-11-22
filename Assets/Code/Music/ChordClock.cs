using System;
using System.Collections.Generic;

namespace DetectiveDave.MusicPlay
{
    public sealed class ChordClock
    {
        const int StrongBeatEvery = 2;
        const double MomentTolerance = 1e-6;

        readonly double secondsPerBeat;
        readonly int beatsPerBar;
        readonly double loopBeats;
        readonly double chartBeatAtLoopStart;
        readonly IReadOnlyList<ChordSegment> segments;

        public ChordClock(float tempoBpm, int beatsPerBar, int loopLengthBars, float chartBeatAtLoopStart, IReadOnlyList<ChordSegment> segments)
        {
            secondsPerBeat = 60.0 / tempoBpm;
            this.beatsPerBar = beatsPerBar;
            loopBeats = (double)beatsPerBar * loopLengthBars;
            this.chartBeatAtLoopStart = chartBeatAtLoopStart;
            this.segments = segments;
        }

        public ChordMoment MomentAt(double loopSeconds)
        {
            var beat = Wrap(ChartBeat(loopSeconds) + MomentTolerance);
            var segmentIndex = SegmentIndexAt(beat);
            if (segmentIndex == ChordMoment.NoSegment) return ChordMoment.Outside;
            var wholeBeat = (int)Math.Floor(beat);
            var beatInBar = wholeBeat % beatsPerBar;
            var isChordStart = wholeBeat == (int)Math.Floor(segments[segmentIndex].StartBeat);
            return new ChordMoment(segmentIndex, beatInBar, beatInBar % StrongBeatEvery == 0, isChordStart);
        }

        double ChartBeat(double loopSeconds) => loopSeconds / secondsPerBeat + chartBeatAtLoopStart;

        double Wrap(double beat)
        {
            var wrapped = beat % loopBeats;
            return wrapped < 0.0 ? wrapped + loopBeats : wrapped;
        }

        int SegmentIndexAt(double beat)
        {
            for (var index = 0; index < segments.Count; index++)
                if (segments[index].Covers(beat)) return index;
            return ChordMoment.NoSegment;
        }
    }
}
