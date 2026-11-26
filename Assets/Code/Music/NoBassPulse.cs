namespace DetectiveDave.MusicPlay
{
    public sealed class NoBassPulse : IBassPulse
    {
        public bool IsAvailable => false;

        public int Poll() => 0;
    }
}
