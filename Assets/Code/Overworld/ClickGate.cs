using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class ClickGate
    {
        readonly float minSecondsBetween;
        float lastPassed = float.NegativeInfinity;

        public ClickGate(float minSecondsBetween)
        {
            this.minSecondsBetween = Math.Max(minSecondsBetween, 0f);
        }

        public bool TryPass(float nowSeconds)
        {
            if (nowSeconds - lastPassed < minSecondsBetween) return false;
            lastPassed = nowSeconds;
            return true;
        }
    }
}
