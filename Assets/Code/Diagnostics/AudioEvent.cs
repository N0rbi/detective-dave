namespace DetectiveDave.Diagnostics
{
    public readonly struct AudioEvent
    {
        public int Frame { get; }

        public AudioEventKind Kind { get; }

        public string ClipName { get; }

        public float Seconds { get; }

        public AudioEvent(int frame, AudioEventKind kind, string clipName, float seconds)
        {
            Frame = frame;
            Kind = kind;
            ClipName = clipName;
            Seconds = seconds;
        }
    }
}
