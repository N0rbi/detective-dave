namespace DetectiveDave.MusicPlay
{
    public readonly struct GridSlot
    {
        public GridSlot(long index, double delaySeconds)
        {
            Index = index;
            DelaySeconds = delaySeconds;
        }

        public long Index { get; }

        public double DelaySeconds { get; }
    }
}
