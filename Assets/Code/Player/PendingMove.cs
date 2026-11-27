using System;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public sealed class PendingMove
    {
        public static readonly PendingMove Never = new PendingMove(0f, () => 0f);

        readonly float maxAgeSeconds;
        readonly Func<float> clock;
        Key key;
        float heldAt;
        bool holds;

        public PendingMove(float maxAgeSeconds, Func<float> clock)
        {
            this.maxAgeSeconds = maxAgeSeconds;
            this.clock = clock;
        }

        public void Hold(Key pressed)
        {
            if (maxAgeSeconds <= 0f) return;
            key = pressed;
            heldAt = clock();
            holds = true;
        }

        public bool TryTake(out Key taken)
        {
            taken = key;
            var fresh = holds && clock() - heldAt <= maxAgeSeconds;
            holds = false;
            return fresh;
        }

        public void Clear() => holds = false;
    }
}
