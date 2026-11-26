using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public static class BassOnsetBeats
    {
        public static float[] ToLoopBeats(IEnumerable<float> onsetsSeconds, double secondsPerBeat, float startOffsetBeats, int loopLengthBeats, float quantizeBeats)
        {
            if (secondsPerBeat <= 0.0 || loopLengthBeats <= 0) return new float[0];
            var step = Math.Max(quantizeBeats, 0.0001f);
            return onsetsSeconds
                .Select(seconds => Wrap(Math.Round((seconds / secondsPerBeat + startOffsetBeats) / step) * step, loopLengthBeats))
                .Select(beat => (float)beat)
                .Distinct()
                .OrderBy(beat => beat)
                .ToArray();
        }

        public static double SecondsPerBeatForDuration(double durationSeconds, int loopLengthBeats) =>
            loopLengthBeats > 0 ? durationSeconds / loopLengthBeats : 0.0;

        public static bool DurationFitsLoop(double durationSeconds, double loopSeconds, double secondsPerBeat) =>
            Math.Abs(durationSeconds - loopSeconds) <= secondsPerBeat;

        static double Wrap(double beat, int loopLengthBeats)
        {
            var wrapped = beat % loopLengthBeats;
            if (wrapped < 0.0) wrapped += loopLengthBeats;
            return wrapped >= loopLengthBeats ? 0.0 : wrapped;
        }
    }
}
