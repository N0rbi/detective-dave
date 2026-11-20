using System.Linq;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class SpiralGeometryTests
    {
        const int Segments = 12;
        const int Turns = 3;
        const float Radius = 10f;
        const float Tolerance = 1e-3f;

        static readonly ScreenRect Screen = new ScreenRect(new Vector2(-8f, -5f), new Vector2(8f, 5f));

        [Test]
        public void StripCountIsSegmentsTimesTurns()
        {
            Assert.AreEqual(36, SpiralGeometry.StripCount(Segments, Turns));
        }

        [Test]
        public void AtLeastTwoTurnsAreUsedSoTheSpiralReachesTheCentre()
        {
            Assert.AreEqual(24, SpiralGeometry.StripCount(Segments, 1));
        }

        [Test]
        public void TheArmStaysOnTheCoverRadiusForTheFirstTurnThenWindsInToTheCentre()
        {
            var arm = SpiralGeometry.ArmPoints(Radius, Segments, Turns);
            for (int index = 0; index <= Segments; index++) Assert.AreEqual(Radius, arm[index].magnitude, Tolerance);
            for (int index = Segments; index < arm.Length - 1; index++) Assert.Greater(arm[index].magnitude, arm[index + 1].magnitude);
            Assert.AreEqual(0f, arm[arm.Length - 1].magnitude, Tolerance);
        }

        [Test]
        public void TheArmTurnsTheSameWayByOneSegmentEachStep()
        {
            var arm = SpiralGeometry.ArmPoints(Radius, Segments, Turns);
            Assert.AreEqual(-30f, Vector2.SignedAngle(arm[1], arm[2]), Tolerance);
            Assert.AreEqual(-30f, Vector2.SignedAngle(arm[25], arm[26]), Tolerance);
        }

        [Test]
        public void StripsAreOrderedFromTheOuterEdgeInwards()
        {
            var strips = SpiralGeometry.Strips(Radius, Segments, Turns);
            Assert.AreEqual(Radius, MaxRadius(strips[0]), Tolerance);
            for (int index = Segments; index < strips.Length - 1; index++)
                Assert.GreaterOrEqual(MaxRadius(strips[index]) + Tolerance, MaxRadius(strips[index + 1]), index.ToString());
        }

        [Test]
        public void TheLastTurnOfStripsAllEndAtTheCentre()
        {
            var strips = SpiralGeometry.Strips(Radius, Segments, Turns);
            for (int index = strips.Length - Segments; index < strips.Length; index++)
                Assert.IsTrue(strips[index].Any(corner => corner.magnitude < Tolerance), index.ToString());
        }

        [Test]
        public void TheFirstStripIsATriangleBecauseTheOuterRingStartsAndEndsOnTheSamePoint()
        {
            Assert.AreEqual(3, SpiralGeometry.Strips(Radius, Segments, Turns)[0].Length);
        }

        [Test]
        public void CoverRadiusReachesTheFarthestScreenCornerEvenThroughAFlatEdge()
        {
            var centre = new Vector2(2f, 1f);
            float radius = SpiralGeometry.CoverRadius(centre, Screen.Min, Screen.Max, Segments);
            float farthest = Vector2.Distance(centre, Screen.Min);
            Assert.GreaterOrEqual(radius * Mathf.Cos(Mathf.PI / Segments), farthest - Tolerance);
        }

        [Test]
        public void TheStripsCoverTheWholeScreenFromAnyCentre()
        {
            foreach (var centre in new[] { Vector2.zero, new Vector2(-6f, 4f), new Vector2(7f, -4f) })
            {
                var strips = SpiralGeometry.StripsOverScreen(centre, Screen, Segments, Turns);
                foreach (var point in ScreenSamples())
                    Assert.IsTrue(strips.Any(strip => Contains(strip, point - centre)), centre + " " + point);
            }
        }

        [Test]
        public void StripsCompletelyOutsideTheScreenAreDropped()
        {
            var centre = new Vector2(-6f, 4f);
            var strips = SpiralGeometry.StripsOverScreen(centre, Screen, Segments, Turns);
            Assert.Less(strips.Length, SpiralGeometry.StripCount(Segments, Turns));
            foreach (var strip in strips) Assert.IsTrue(SpiralGeometry.Overlaps(strip, centre, Screen));
        }

        [Test]
        public void TheFirstStripThatIsKeptTouchesTheScreen()
        {
            var strips = SpiralGeometry.StripsOverScreen(Vector2.zero, Screen, Segments, Turns);
            Assert.IsTrue(SpiralGeometry.Overlaps(strips[0], Vector2.zero, Screen));
        }

        [Test]
        public void VisibleCountShowsTheFirstStripAsSoonAsProgressStarts()
        {
            Assert.AreEqual(0, SpiralGeometry.VisibleCount(0f, 36));
            Assert.AreEqual(1, SpiralGeometry.VisibleCount(0.001f, 36));
        }

        [Test]
        public void VisibleCountFollowsProgressInWholeStrips()
        {
            Assert.AreEqual(18, SpiralGeometry.VisibleCount(0.5f, 36));
            Assert.AreEqual(12, SpiralGeometry.VisibleCount(1f / 3f, 36));
            Assert.AreEqual(36, SpiralGeometry.VisibleCount(1f, 36));
        }

        [Test]
        public void VisibleCountStaysInsideTheStripRange()
        {
            Assert.AreEqual(0, SpiralGeometry.VisibleCount(-1f, 36));
            Assert.AreEqual(36, SpiralGeometry.VisibleCount(2f, 36));
        }

        static float MaxRadius(Vector2[] strip) => strip.Max(corner => corner.magnitude);

        static Vector2[] ScreenSamples()
        {
            var samples = new System.Collections.Generic.List<Vector2>();
            for (int x = 0; x <= 8; x++)
                for (int y = 0; y <= 8; y++)
                    samples.Add(new Vector2(Mathf.Lerp(Screen.Min.x, Screen.Max.x, x / 8f), Mathf.Lerp(Screen.Min.y, Screen.Max.y, y / 8f)));
            return samples.ToArray();
        }

        static bool Contains(Vector2[] polygon, Vector2 point)
        {
            for (int corner = 1; corner < polygon.Length - 1; corner++)
                if (InsideTriangle(point, polygon[0], polygon[corner], polygon[corner + 1])) return true;
            return false;
        }

        static bool InsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float first = Cross(a, b, point);
            float second = Cross(b, c, point);
            float third = Cross(c, a, point);
            bool hasNegative = first < -Tolerance || second < -Tolerance || third < -Tolerance;
            bool hasPositive = first > Tolerance || second > Tolerance || third > Tolerance;
            return !(hasNegative && hasPositive);
        }

        static float Cross(Vector2 from, Vector2 to, Vector2 point) => (to.x - from.x) * (point.y - from.y) - (to.y - from.y) * (point.x - from.x);
    }
}
