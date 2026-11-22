using System;

namespace DetectiveDave.MusicPlay
{
    public sealed class ChordNoteTiming : INoteTiming
    {
        readonly ChordClock clock;
        readonly NoteQuantizer quantizer;
        readonly Func<double> loopSeconds;

        public ChordNoteTiming(ChordClock clock, NoteQuantizer quantizer, Func<double> loopSeconds)
        {
            this.clock = clock;
            this.quantizer = quantizer;
            this.loopSeconds = loopSeconds;
        }

        public bool TryPlan(out double delaySeconds, out ChordMoment moment)
        {
            var now = loopSeconds();
            if (!quantizer.TryPlan(now, out delaySeconds))
            {
                moment = ChordMoment.Outside;
                return false;
            }
            moment = clock.MomentAt(now + delaySeconds);
            return true;
        }
    }
}
