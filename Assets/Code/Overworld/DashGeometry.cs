using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class DashGeometry
    {
        public static List<Vector2[]> Dashes(IReadOnlyList<Vector2> points, float dashLength, float gapLength, float flowOffset)
        {
            var dashes = new List<Vector2[]>();
            float period = dashLength + gapLength;
            if (points == null || points.Count < 2 || dashLength <= 0f || gapLength < 0f || period <= 0f) return dashes;

            float total = LengthOf(points);
            for (float start = -Mathf.Repeat(flowOffset, period); start < total; start += period)
            {
                float from = Mathf.Max(start, 0f);
                float to = Mathf.Min(start + dashLength, total);
                if (to > from) dashes.Add(Slice(points, from, to));
            }
            return dashes;
        }

        public static float LengthOf(IReadOnlyList<Vector2> points)
        {
            float length = 0f;
            for (int index = 1; index < points.Count; index++) length += Vector2.Distance(points[index - 1], points[index]);
            return length;
        }

        static Vector2[] Slice(IReadOnlyList<Vector2> points, float from, float to)
        {
            var slice = new List<Vector2> { PointAt(points, from) };
            float walked = 0f;
            for (int index = 1; index < points.Count; index++)
            {
                walked += Vector2.Distance(points[index - 1], points[index]);
                if (walked > from && walked < to) slice.Add(points[index]);
            }
            slice.Add(PointAt(points, to));
            return slice.ToArray();
        }

        static Vector2 PointAt(IReadOnlyList<Vector2> points, float distance)
        {
            float walked = 0f;
            for (int index = 1; index < points.Count; index++)
            {
                float segment = Vector2.Distance(points[index - 1], points[index]);
                if (distance <= walked + segment && segment > 0f) return Vector2.Lerp(points[index - 1], points[index], (distance - walked) / segment);
                walked += segment;
            }
            return points[points.Count - 1];
        }
    }
}
