using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public sealed class BassPulseClock
    {
        readonly double[] onsetSeconds;

        public BassPulseClock(IEnumerable<float> onsetBeats, int loopLengthBeats, double loopSeconds)
        {
            var secondsPerBeat = loopLengthBeats > 0 ? loopSeconds / loopLengthBeats : 0.0;
            onsetSeconds = onsetBeats.Select(beat => beat * secondsPerBeat).OrderBy(seconds => seconds).ToArray();
        }

        public int Count => onsetSeconds.Length;

        public int CountBetween(double fromSeconds, double toSeconds)
        {
            if (toSeconds >= fromSeconds) return onsetSeconds.Count(seconds => seconds > fromSeconds && seconds <= toSeconds);
            return onsetSeconds.Count(seconds => seconds > fromSeconds || seconds <= toSeconds);
        }
    }
}
