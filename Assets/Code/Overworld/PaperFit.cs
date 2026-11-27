using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class PaperFit
    {
        public static Vector2 HalfExtents(float width, float aspect, float rotationDegrees)
        {
            float radians = rotationDegrees * Mathf.Deg2Rad;
            float cos = Math.Abs(Mathf.Cos(radians));
            float sin = Math.Abs(Mathf.Sin(radians));
            float halfWidth = width * 0.5f;
            float halfHeight = width * aspect * 0.5f;
            return new Vector2(cos * halfWidth + sin * halfHeight, sin * halfWidth + cos * halfHeight);
        }

        public static PaperSpot Fit(PaperSpot spot, float aspect, Vector2 viewCentre, Vector2 viewSize, float marginShare)
        {
            var allowed = new Vector2(viewSize.x * (0.5f - marginShare), viewSize.y * (0.5f - marginShare));
            var half = HalfExtents(spot.Width, aspect, spot.RotationDegrees);
            float shrink = Math.Min(1f, Math.Min(allowed.x / half.x, allowed.y / half.y));
            float width = spot.Width * shrink;
            half *= shrink;
            var offset = spot.Centre - viewCentre;
            offset.x = Mathf.Clamp(offset.x, -(allowed.x - half.x), allowed.x - half.x);
            offset.y = Mathf.Clamp(offset.y, -(allowed.y - half.y), allowed.y - half.y);
            return new PaperSpot(viewCentre + offset, width, spot.RotationDegrees);
        }
    }
}
