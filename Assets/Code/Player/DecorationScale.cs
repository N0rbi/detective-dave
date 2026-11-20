using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public static class DecorationScale
    {
        const float MinDegreesPerSecond = 1f;
        const float DegreesPerTurn = 360f;

        public static float For(float sizeInKeys, float keyScale, float spriteWidth) => sizeInKeys * keyScale / spriteWidth;

        public static float SecondsPerTurn(float degreesPerSecond) => DegreesPerTurn / Mathf.Max(MinDegreesPerSecond, degreesPerSecond);
    }
}
