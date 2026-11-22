namespace DetectiveDave.MusicPlay
{
    public interface INoteTiming
    {
        bool TryPlan(out double delaySeconds, out ChordMoment moment);
    }
}
