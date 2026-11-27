using System;

namespace DetectiveDave.MusicPlay
{
    public sealed class BassPulseSource : IBassPulse
    {
        readonly BassPulseClock clock;
        readonly Func<double> loopSeconds;
        double last;
        bool hasLast;

        public BassPulseSource(BassPulseClock clock, Func<double> loopSeconds)
        {
            this.clock = clock;
            this.loopSeconds = loopSeconds;
        }

        public bool IsAvailable => clock.Count > 0;

        public int Poll()
        {
            var now = loopSeconds();
            if (!hasLast)
            {
                hasLast = true;
                last = now;
                return 0;
            }
            var notes = clock.CountBetween(last, now);
            last = now;
            return notes;
        }
    }
}
