using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>The maths of the flicker timeline: where a time and a brightness sit inside the drawn rectangle, and snapping to the grid.</summary>
    internal readonly struct TimelineMapper
    {
        readonly Rect area;
        readonly float duration;

        public TimelineMapper(Rect area, float duration)
        {
            this.area = area;
            this.duration = Mathf.Max(duration, 0.0001f);
        }

        public float TimeToX(float time) => area.xMin + Mathf.Clamp01(time / duration) * area.width;

        public float XToTime(float x) => Mathf.Clamp01((x - area.xMin) / area.width) * duration;

        public float BrightnessToY(float brightness) => area.yMax - Mathf.Clamp01(brightness) * area.height;

        public float YToBrightness(float y) => Mathf.Clamp01((area.yMax - y) / area.height);

        public Vector2 ToScreen(FlickerPoint point) => new Vector2(TimeToX(point.Time), BrightnessToY(point.Brightness));

        public static float Snap(float value, float step, bool enabled) => enabled && step > 0f ? Mathf.Round(value / step) * step : value;

        public static int NearestPoint(Vector2[] screenPoints, Vector2 mouse, float radius)
        {
            int nearest = -1;
            float best = radius * radius;
            for (int index = 0; index < screenPoints.Length; index++)
            {
                float distance = (screenPoints[index] - mouse).sqrMagnitude;
                if (distance > best) continue;
                best = distance;
                nearest = index;
            }
            return nearest;
        }
    }
}
