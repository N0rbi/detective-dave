namespace DetectiveDave.MusicPlay
{
    public interface IBassPulse
    {
        bool IsAvailable { get; }

        int Poll();
    }
}
