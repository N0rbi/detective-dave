namespace DetectiveDave.OverworldPlay
{
    public sealed class BriefingGate
    {
        readonly TimedReveal description;

        public BriefingGate(TimedReveal description)
        {
            this.description = description;
        }

        public bool IsWritable { get; private set; }

        public int VisibleCount => description.VisibleCount;

        public int Tick(float deltaSeconds)
        {
            if (IsWritable) return 0;
            int revealed = description.Advance(deltaSeconds);
            IsWritable = description.IsDone;
            return revealed;
        }

        public void OnAnyKey()
        {
            if (!IsWritable) description.Skip();
        }
    }
}
