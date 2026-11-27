namespace DetectiveDave.Diagnostics
{
    public interface IAudioEvents
    {
        void Record(AudioEventKind kind, string clipName, float seconds);
    }
}
