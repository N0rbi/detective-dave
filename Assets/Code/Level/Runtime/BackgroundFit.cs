using DetectiveDave.Levels;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    public readonly struct BackgroundFit
    {
        public readonly float Scale;
        public readonly Vector2 Shift;

        BackgroundFit(float scale, Vector2 shift)
        {
            Scale = scale;
            Shift = shift;
        }

        public static BackgroundFit Of(Vector2 spriteSize, Vector2 viewSize, BackgroundPlacement placement)
        {
            var scale = Mathf.Max(viewSize.x / spriteSize.x, viewSize.y / spriteSize.y) * placement.Zoom;
            var spare = spriteSize * scale - viewSize;
            var reach = new Vector2(Mathf.Max(0f, spare.x) * 0.5f, Mathf.Max(0f, spare.y) * 0.5f);
            return new BackgroundFit(scale, new Vector2(Reach(placement.Offset.x, viewSize.x, reach.x), Reach(placement.Offset.y, viewSize.y, reach.y)));
        }

        static float Reach(float offset, float viewLength, float limit) => Mathf.Clamp(offset * viewLength, -limit, limit);
    }
}
