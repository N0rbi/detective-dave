namespace DetectiveDave.Diagnostics
{
    public sealed class NoAudioEvents : IAudioEvents
    {
        public void Record(AudioEventKind kind, string clipName, float seconds)
        {
        }
    }
}
