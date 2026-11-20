using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class TimedReveal
    {
        readonly int length;
        readonly float charactersPerSecond;
        readonly float skipMultiplier;
        float position;

        public TimedReveal(int length, float secondsPerCharacter, float skipSpeedMultiplier)
        {
            this.length = Math.Max(length, 0);
            charactersPerSecond = 1f / Math.Max(secondsPerCharacter, 0.0001f);
            skipMultiplier = Math.Max(skipSpeedMultiplier, 1f);
        }

        public int VisibleCount => (int)Math.Min(position, length);

        public bool IsDone => VisibleCount >= length;

        public bool IsSkipping { get; private set; }

        public void Skip() => IsSkipping = true;

        public int Advance(float deltaSeconds)
        {
            int before = VisibleCount;
            position += deltaSeconds * charactersPerSecond * (IsSkipping ? skipMultiplier : 1f);
            return VisibleCount - before;
        }
    }
}
