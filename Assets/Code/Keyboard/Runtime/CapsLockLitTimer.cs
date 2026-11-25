using System;

namespace DetectiveDave.KeyboardGrid
{
    public sealed class CapsLockLitTimer
    {
        readonly Func<float> clock;
        readonly float maxLitSeconds;
        bool held;
        bool pressedThisFrame;
        float pressedAt;

        public CapsLockLitTimer(Func<float> clock, float maxLitSeconds)
        {
            this.clock = clock;
            this.maxLitSeconds = maxLitSeconds;
        }

        public bool IsLit => held && (pressedThisFrame || clock() - pressedAt < maxLitSeconds);

        public void OnPressed()
        {
            held = true;
            pressedThisFrame = true;
            pressedAt = clock();
        }

        public void OnReleased() => held = false;

        public void EndFrame() => pressedThisFrame = false;
    }
}
