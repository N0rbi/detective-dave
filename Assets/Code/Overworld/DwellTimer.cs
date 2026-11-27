using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DwellTimer
    {
        const float MinimumSeconds = 0.01f;

        readonly float seconds;
        float elapsed;

        public DwellTimer(float seconds)
        {
            this.seconds = Math.Max(seconds, MinimumSeconds);
        }

        public bool IsRunning { get; private set; }

        public float Progress => Math.Min(elapsed / seconds, 1f);

        public void Start()
        {
            elapsed = 0f;
            IsRunning = true;
        }

        public void Cancel()
        {
            elapsed = 0f;
            IsRunning = false;
        }

        public bool Tick(float deltaSeconds)
        {
            if (!IsRunning) return false;
            elapsed += deltaSeconds;
            if (elapsed < seconds) return false;
            IsRunning = false;
            return true;
        }
    }
}
