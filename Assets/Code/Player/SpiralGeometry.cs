using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public static class SpiralGeometry
    {
        const float FullTurnRadians = Mathf.PI * 2f;
        const float CoverMargin = 1.02f;
        const float SamePointDistanceSquared = 1e-8f;
        const float ProgressTolerance = 1e-4f;
        const int MinTurns = 2;

        public static int StripCount(int segmentsPerTurn, int turns) => segmentsPerTurn * Mathf.Max(MinTurns, turns);

        public static float CoverRadius(Vector2 centre, Vector2 screenMin, Vector2 screenMax, int segmentsPerTurn)
        {
            float farthest = Mathf.Max(
                Vector2.Distance(centre, new Vector2(screenMin.x, screenMin.y)),
                Vector2.Distance(centre, new Vector2(screenMin.x, screenMax.y)),
                Vector2.Distance(centre, new Vector2(screenMax.x, screenMin.y)),
                Vector2.Distance(centre, new Vector2(screenMax.x, screenMax.y)));
            return farthest * CoverMargin / Mathf.Cos(Mathf.PI / segmentsPerTurn);
        }

        public static Vector2[] ArmPoints(float coverRadius, int segmentsPerTurn, int turns)
        {
            int count = StripCount(segmentsPerTurn, turns);
            var points = new Vector2[count + 1];
            for (int index = 0; index <= count; index++)
                points[index] = PointAt(index, ArmRadius(index, segmentsPerTurn, count, coverRadius), segmentsPerTurn);
            return points;
        }

        public static Vector2[][] Strips(float coverRadius, int segmentsPerTurn, int turns)
        {
            var arm = ArmPoints(coverRadius, segmentsPerTurn, turns);
            int count = arm.Length - 1;
            var strips = new Vector2[count][];
            for (int index = 0; index < count; index++)
            {
                int innerStart = InnerIndex(index, count, segmentsPerTurn);
                int innerEnd = InnerIndex(index + 1, count, segmentsPerTurn);
                strips[index] = WithoutRepeats(new[] { arm[index], arm[index + 1], arm[innerEnd], arm[innerStart] });
            }
            return strips;
        }

        public static Vector2[][] StripsOverScreen(Vector2 centre, ScreenRect screen, int segmentsPerTurn, int turns)
        {
            float radius = CoverRadius(centre, screen.Min, screen.Max, segmentsPerTurn);
            var kept = new List<Vector2[]>();
            foreach (var strip in Strips(radius, segmentsPerTurn, turns))
                if (Overlaps(strip, centre, screen)) kept.Add(strip);
            return kept.ToArray();
        }

        public static bool Overlaps(Vector2[] strip, Vector2 centre, ScreenRect screen)
        {
            var min = strip[0];
            var max = strip[0];
            foreach (var corner in strip)
            {
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }
            min += centre;
            max += centre;
            return min.x <= screen.Max.x && max.x >= screen.Min.x && min.y <= screen.Max.y && max.y >= screen.Min.y;
        }

        public static int VisibleCount(float progress, int stripCount)
        {
            if (progress <= 0f) return 0;
            int count = Mathf.CeilToInt(progress * stripCount - ProgressTolerance);
            return Mathf.Clamp(count, 1, stripCount);
        }

        static int InnerIndex(int index, int count, int segmentsPerTurn) => Mathf.Min(index + segmentsPerTurn, count);

        static float ArmRadius(int index, int segmentsPerTurn, int count, float coverRadius) =>
            index <= segmentsPerTurn ? coverRadius : coverRadius * (count - index) / (count - segmentsPerTurn);

        static Vector2 PointAt(int index, float radius, int segmentsPerTurn)
        {
            float angle = -index * FullTurnRadians / segmentsPerTurn;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        static Vector2[] WithoutRepeats(Vector2[] corners)
        {
            var result = new List<Vector2>();
            foreach (var corner in corners)
                if (result.Count == 0 || !SamePoint(result[result.Count - 1], corner)) result.Add(corner);
            if (result.Count > 1 && SamePoint(result[0], result[result.Count - 1])) result.RemoveAt(result.Count - 1);
            return result.ToArray();
        }

        static bool SamePoint(Vector2 first, Vector2 second) => (first - second).sqrMagnitude < SamePointDistanceSquared;
    }
}
