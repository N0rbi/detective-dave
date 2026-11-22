using System;
using System.Collections.Generic;

namespace DetectiveDave.MusicPlay
{
    public sealed class ChordClock
    {
        const int StrongBeatEvery = 2;
        const double SlotTolerance = 1e-6;
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

        public double SecondsPerBeat => secondsPerBeat;

        public double LoopSeconds => loopBeats * secondsPerBeat;

        public double UnwrappedChartBeat(double loopSeconds) => loopSeconds / secondsPerBeat + chartBeatAtLoopStart;

        public ChordMoment MomentAt(double loopSeconds)
        {
            var beat = Wrap(UnwrappedChartBeat(loopSeconds) + MomentTolerance);
            var segmentIndex = SegmentIndexAt(beat);
            if (segmentIndex == ChordMoment.NoSegment) return ChordMoment.Outside;
            var wholeBeat = (int)Math.Floor(beat);
            var beatInBar = wholeBeat % beatsPerBar;
            var isChordStart = wholeBeat == (int)Math.Floor(segments[segmentIndex].StartBeat);
            return new ChordMoment(segmentIndex, beatInBar, beatInBar % StrongBeatEvery == 0, isChordStart);
        }

        public GridSlot NextSlot(double loopSeconds, float gridBeats)
        {
            var position = UnwrappedChartBeat(loopSeconds) / gridBeats;
            var index = (long)Math.Ceiling(position - SlotTolerance);
            var delay = (index - position) * gridBeats * secondsPerBeat;
            return new GridSlot(index, Math.Max(delay, 0.0));
        }

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
