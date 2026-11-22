namespace DetectiveDave.MusicPlay
{
    public sealed class NoteQuantizer
    {
        readonly ChordClock clock;
        readonly QuantizeGrid grid;
        readonly double maxDelaySeconds;
        readonly double leadSeconds;
        readonly bool dropExtraNotesInSlot;
        long lastClaimedSlot = long.MinValue;

        public NoteQuantizer(ChordClock clock, QuantizeGrid grid, float maxDelaySeconds, float leadSeconds, bool dropExtraNotesInSlot)
        {
            this.clock = clock;
            this.grid = grid;
            this.maxDelaySeconds = maxDelaySeconds;
            this.leadSeconds = leadSeconds;
            this.dropExtraNotesInSlot = dropExtraNotesInSlot;
        }

        public bool TryPlan(double loopSeconds, out double delaySeconds)
        {
            if (grid == QuantizeGrid.Off) return PlayNow(out delaySeconds);
            var gridBeats = QuantizeGrids.Beats(grid);
            var slot = clock.NextSlot(loopSeconds + leadSeconds, gridBeats);
            var delay = leadSeconds + slot.DelaySeconds;
            if (delay > maxDelaySeconds) return PlayNow(out delaySeconds);
            if (slot.Index > lastClaimedSlot) return Claim(slot.Index, delay, out delaySeconds);
            return TryTakeFollowingSlot(slot, delay, gridBeats, out delaySeconds);
        }

        bool TryTakeFollowingSlot(GridSlot slot, double delay, float gridBeats, out double delaySeconds)
        {
            var following = lastClaimedSlot + 1;
            var followingDelay = delay + (following - slot.Index) * gridBeats * clock.SecondsPerBeat;
            if (dropExtraNotesInSlot || followingDelay > maxDelaySeconds) return Drop(out delaySeconds);
            return Claim(following, followingDelay, out delaySeconds);
        }

        bool Claim(long slotIndex, double delay, out double delaySeconds)
        {
            lastClaimedSlot = slotIndex;
            delaySeconds = delay;
            return true;
        }

        static bool PlayNow(out double delaySeconds)
        {
            delaySeconds = 0.0;
            return true;
        }

        static bool Drop(out double delaySeconds)
        {
            delaySeconds = 0.0;
            return false;
        }
    }
}
