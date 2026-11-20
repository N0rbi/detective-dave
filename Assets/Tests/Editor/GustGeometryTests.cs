using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class GustGeometryTests
    {
        const float PlayerHeight = 2f;
        const float Tolerance = 0.0001f;

        static readonly Vector2 Origin = Vector2.zero;

        [Test]
        public void NoMovementGivesNoSegments()
        {
            var segments = GustGeometry.Build(Origin, Origin, new[] { new GustLine(0f, 0.05f, 1f, 0f) }, PlayerHeight);
            Assert.IsEmpty(segments);
        }

        [Test]
        public void NoLinesGiveNoSegments()
        {
            Assert.IsEmpty(GustGeometry.Build(Origin, Vector2.right, new GustLine[0], PlayerHeight));
            Assert.IsEmpty(GustGeometry.Build(Origin, Vector2.right, null, PlayerHeight));
        }

        [Test]
        public void FullLengthLineCoversTheTravelledDistance()
        {
            var segment = GustGeometry.Build(Origin, new Vector2(4f, 0f), new[] { new GustLine(0f, 0.05f, 1f, 0f) }, PlayerHeight)[0];
            Assert.AreEqual(Origin, segment.Start);
            Assert.AreEqual(new Vector2(4f, 0f), segment.End);
        }

        [Test]
        public void LengthIsAShareOfTheDistanceTravelled()
        {
            var segment = GustGeometry.Build(Origin, new Vector2(4f, 0f), new[] { new GustLine(0f, 0.05f, 0.5f, 0f) }, PlayerHeight)[0];
            Assert.AreEqual(2f, segment.Length, Tolerance);
        }

        [Test]
        public void ShortDistanceGivesShortLines()
        {
            var lines = new[] { new GustLine(0f, 0.05f, 1f, 0f) };
            var longDash = GustGeometry.Build(Origin, new Vector2(4f, 0f), lines, PlayerHeight)[0];
            var bump = GustGeometry.Build(Origin, new Vector2(0.7f, 0f), lines, PlayerHeight)[0];
            Assert.AreEqual(0.7f, bump.Length, Tolerance);
            Assert.Less(bump.Length, longDash.Length);
        }

        [Test]
        public void StartShareMovesTheLineAlongTheDash()
        {
            var segment = GustGeometry.Build(Origin, new Vector2(4f, 0f), new[] { new GustLine(0f, 0.05f, 0.25f, 0.5f) }, PlayerHeight)[0];
            Assert.AreEqual(new Vector2(2f, 0f), segment.Start);
            Assert.AreEqual(new Vector2(3f, 0f), segment.End);
        }

        [Test]
        public void LineNeverRunsPastTheEndOfTheDash()
        {
            var segment = GustGeometry.Build(Origin, new Vector2(4f, 0f), new[] { new GustLine(0f, 0.05f, 1f, 0.75f) }, PlayerHeight)[0];
            Assert.AreEqual(4f, segment.End.x, Tolerance);
        }

        [Test]
        public void OffsetIsPerpendicularToTheDashInPlayerHeights()
        {
            var horizontal = GustGeometry.Build(Origin, new Vector2(4f, 0f), new[] { new GustLine(0.25f, 0.05f, 1f, 0f) }, PlayerHeight)[0];
            Assert.AreEqual(0.5f, horizontal.Start.y, Tolerance);
            Assert.AreEqual(0.5f, horizontal.End.y, Tolerance);
            var vertical = GustGeometry.Build(Origin, new Vector2(0f, 4f), new[] { new GustLine(0.25f, 0.05f, 1f, 0f) }, PlayerHeight)[0];
            Assert.AreEqual(-0.5f, vertical.Start.x, Tolerance);
            Assert.AreEqual(0f, vertical.Start.y, Tolerance);
        }

        [Test]
        public void ThicknessIsAShareOfThePlayerHeight()
        {
            var segment = GustGeometry.Build(Origin, Vector2.right, new[] { new GustLine(0f, 0.05f, 1f, 0f) }, PlayerHeight)[0];
            Assert.AreEqual(0.1f, segment.Thickness, Tolerance);
        }

        [Test]
        public void EachLineKeepsItsOwnThicknessAndOffset()
        {
            var lines = new[] { new GustLine(-0.25f, 0.03f, 0.9f, 0f), new GustLine(0.3f, 0.06f, 1f, 0f) };
            var segments = GustGeometry.Build(Origin, new Vector2(2f, 0f), lines, PlayerHeight);
            Assert.AreEqual(2, segments.Length);
            Assert.AreNotEqual(segments[0].Thickness, segments[1].Thickness);
            Assert.AreNotEqual(segments[0].Start.y, segments[1].Start.y);
        }

        [Test]
        public void DiagonalDashKeepsTheLinesParallelToTheMove()
        {
            var segment = GustGeometry.Build(Origin, new Vector2(3f, 3f), new[] { new GustLine(0f, 0.05f, 1f, 0f) }, PlayerHeight)[0];
            var direction = (segment.End - segment.Start).normalized;
            Assert.AreEqual(Mathf.Sqrt(0.5f), direction.x, Tolerance);
            Assert.AreEqual(Mathf.Sqrt(0.5f), direction.y, Tolerance);
        }

        [Test]
        public void ShrinkingMovesTheTailTowardsTheHead()
        {
            var segment = new GustSegment(Origin, new Vector2(4f, 0f), 0.1f);
            Assert.AreEqual(new Vector2(2f, 0f), GustGeometry.Shrunk(segment, 0.5f).Start);
            Assert.AreEqual(segment.End, GustGeometry.Shrunk(segment, 0.5f).End);
            Assert.AreEqual(0f, GustGeometry.Shrunk(segment, 1f).Length, Tolerance);
            Assert.AreEqual(segment.Length, GustGeometry.Shrunk(segment, 0f).Length, Tolerance);
        }
    }
}
