using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class TimedReveal
    {
        readonly int length;
        readonly float charactersPerSecond;
        float elapsed;

        public TimedReveal(int length, float charactersPerSecond)
        {
            this.length = Math.Max(length, 0);
            this.charactersPerSecond = Math.Max(charactersPerSecond, 0.01f);
        }

        public int VisibleCount => (int)Math.Min(elapsed * charactersPerSecond, length);

        public bool IsDone => VisibleCount >= length;

        public void Advance(float deltaSeconds) => elapsed += deltaSeconds;
    }
}
