using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public readonly struct ScreenRect
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public ScreenRect(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public Vector2 Center => (Min + Max) * 0.5f;

        public Vector2 Size => Max - Min;

        public static ScreenRect Around(Vector2 center, Vector2 halfSize) => new ScreenRect(center - halfSize, center + halfSize);

        public ScreenRect Union(ScreenRect other) => new ScreenRect(Vector2.Min(Min, other.Min), Vector2.Max(Max, other.Max));

        public ScreenRect Expanded(Vector2 margin) => new ScreenRect(Min - margin, Max + margin);
    }
}
