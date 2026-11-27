using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    public readonly struct BackgroundFit
    {
        public readonly float Scale;
        public readonly Vector2 Center;

        BackgroundFit(float scale, Vector2 center)
        {
            Scale = scale;
            Center = center;
        }

        public static BackgroundFit Of(Vector2 spriteSize, ScreenRect coverage, Vector2 referenceView, BackgroundPlacement placement)
        {
            var cover = coverage.Size;
            var scale = Mathf.Max(cover.x / spriteSize.x, cover.y / spriteSize.y) * placement.Zoom;
            var spare = spriteSize * scale - cover;
            var reach = new Vector2(Mathf.Max(0f, spare.x) * 0.5f, Mathf.Max(0f, spare.y) * 0.5f);
            var shift = new Vector2(Reach(placement.Offset.x, referenceView.x, reach.x), Reach(placement.Offset.y, referenceView.y, reach.y));
            return new BackgroundFit(scale, coverage.Center + shift);
        }

        public static Vector2 PivotPosition(Vector2 wantedCenter, Vector2 boundsCenter, bool flipX, bool flipY, float scale)
        {
            var mirrored = new Vector2(flipX ? -boundsCenter.x : boundsCenter.x, flipY ? -boundsCenter.y : boundsCenter.y);
            return wantedCenter - mirrored * scale;
        }

        static float Reach(float offset, float viewLength, float limit) => Mathf.Clamp(offset * viewLength, -limit, limit);
    }
}
