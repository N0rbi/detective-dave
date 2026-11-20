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

        public ScreenRect Union(ScreenRect other) => new ScreenRect(Vector2.Min(Min, other.Min), Vector2.Max(Max, other.Max));
    }
}
