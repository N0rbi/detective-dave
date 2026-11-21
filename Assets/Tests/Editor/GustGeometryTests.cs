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
        static readonly Vector2 FarEnd = new Vector2(4f, 0f);

        static GustLine[] OneLine(float offset = 0f, float thickness = 0.05f, float lengthShare = 1f) => new[] { new GustLine(offset, thickness, lengthShare) };

        [Test]
        public void NoMovementGivesNoSegments()
        {
            Assert.IsEmpty(GustGeometry.Follow(Origin, Origin, Origin, OneLine(), PlayerHeight));
        }

        [Test]
        public void PlayerStillAtTheStartGivesNoSegments()
        {
            Assert.IsEmpty(GustGeometry.Follow(Origin, FarEnd, Origin, OneLine(), PlayerHeight));
        }

        [Test]
        public void PlayerBehindTheStartGivesNoSegments()
        {
            Assert.IsEmpty(GustGeometry.Follow(Origin, FarEnd, new Vector2(-1f, 0f), OneLine(), PlayerHeight));
        }

        [Test]
        public void NoLinesGiveNoSegments()
        {
            Assert.IsEmpty(GustGeometry.Follow(Origin, FarEnd, Vector2.right, new GustLine[0], PlayerHeight));
            Assert.IsEmpty(GustGeometry.Follow(Origin, FarEnd, Vector2.right, null, PlayerHeight));
        }

        [Test]
        public void HeadIsAtThePlayer()
        {
            var segment = GustGeometry.Follow(Origin, FarEnd, new Vector2(1.5f, 0f), OneLine(), PlayerHeight)[0];
            Assert.AreEqual(1.5f, segment.End.x, Tolerance);
            Assert.AreEqual(0f, segment.End.y, Tolerance);
        }

        [Test]
        public void TailTrailsBehindTheHeadAndNeverBeforeTheStart()
        {
            var segment = GustGeometry.Follow(Origin, FarEnd, new Vector2(1.5f, 0f), OneLine(), PlayerHeight)[0];
            Assert.AreEqual(0f, segment.Start.x, Tolerance);
            Assert.LessOrEqual(segment.Start.x, segment.End.x);
        }

        [Test]
        public void LengthIsAShareOfTheDistanceTravelledSoFar()
        {
            var segment = GustGeometry.Follow(Origin, FarEnd, new Vector2(3f, 0f), OneLine(lengthShare: 0.5f), PlayerHeight)[0];
            Assert.AreEqual(1.5f, segment.Length, Tolerance);
            Assert.AreEqual(1.5f, segment.Start.x, Tolerance);
        }

        [Test]
        public void LinesGrowAsThePlayerMovesAway()
        {
            var early = GustGeometry.Follow(Origin, FarEnd, new Vector2(1f, 0f), OneLine(lengthShare: 0.5f), PlayerHeight)[0];
            var late = GustGeometry.Follow(Origin, FarEnd, new Vector2(3f, 0f), OneLine(lengthShare: 0.5f), PlayerHeight)[0];
            Assert.Less(early.Length, late.Length);
        }

        [Test]
        public void PlayerPastTheLimitIsHeldAtTheLimit()
        {
            var segment = GustGeometry.Follow(Origin, FarEnd, new Vector2(9f, 0f), OneLine(), PlayerHeight)[0];
            Assert.AreEqual(4f, segment.End.x, Tolerance);
            Assert.AreEqual(4f, segment.Length, Tolerance);
        }

        [Test]
        public void ShortReachGivesShortLines()
        {
            var bump = GustGeometry.Follow(Origin, new Vector2(0.7f, 0f), new Vector2(0.7f, 0f), OneLine(), PlayerHeight)[0];
            var dash = GustGeometry.Follow(Origin, FarEnd, FarEnd, OneLine(), PlayerHeight)[0];
            Assert.AreEqual(0.7f, bump.Length, Tolerance);
            Assert.Less(bump.Length, dash.Length);
        }

        [Test]
        public void OffsetIsPerpendicularToTheDashInPlayerHeights()
        {
            var horizontal = GustGeometry.Follow(Origin, FarEnd, FarEnd, OneLine(0.25f), PlayerHeight)[0];
            Assert.AreEqual(0.5f, horizontal.Start.y, Tolerance);
            Assert.AreEqual(0.5f, horizontal.End.y, Tolerance);
            var upEnd = new Vector2(0f, 4f);
            var vertical = GustGeometry.Follow(Origin, upEnd, upEnd, OneLine(0.25f), PlayerHeight)[0];
            Assert.AreEqual(-0.5f, vertical.End.x, Tolerance);
            Assert.AreEqual(4f, vertical.End.y, Tolerance);
        }

        [Test]
        public void ThicknessIsAShareOfThePlayerHeight()
        {
            var segment = GustGeometry.Follow(Origin, Vector2.right, Vector2.right, OneLine(), PlayerHeight)[0];
            Assert.AreEqual(0.1f, segment.Thickness, Tolerance);
        }

        [Test]
        public void EachLineKeepsItsOwnThicknessOffsetAndLength()
        {
            var lines = new[] { new GustLine(-0.25f, 0.03f, 0.9f), new GustLine(0.3f, 0.06f, 1f) };
            var segments = GustGeometry.Follow(Origin, new Vector2(2f, 0f), new Vector2(2f, 0f), lines, PlayerHeight);
            Assert.AreEqual(2, segments.Length);
            Assert.AreNotEqual(segments[0].Thickness, segments[1].Thickness);
            Assert.AreNotEqual(segments[0].Start.y, segments[1].Start.y);
            Assert.AreNotEqual(segments[0].Length, segments[1].Length);
        }

        [Test]
        public void DiagonalDashKeepsTheLinesParallelToTheMove()
        {
            var end = new Vector2(3f, 3f);
            var segment = GustGeometry.Follow(Origin, end, end, OneLine(), PlayerHeight)[0];
            var direction = (segment.End - segment.Start).normalized;
            Assert.AreEqual(Mathf.Sqrt(0.5f), direction.x, Tolerance);
            Assert.AreEqual(Mathf.Sqrt(0.5f), direction.y, Tolerance);
        }

        [Test]
        public void ShrinkingMovesTheTailTowardsTheHead()
        {
            var segment = new GustSegment(Origin, FarEnd, 0.1f);
            Assert.AreEqual(new Vector2(2f, 0f), GustGeometry.Shrunk(segment, 0.5f).Start);
            Assert.AreEqual(segment.End, GustGeometry.Shrunk(segment, 0.5f).End);
            Assert.AreEqual(0f, GustGeometry.Shrunk(segment, 1f).Length, Tolerance);
            Assert.AreEqual(segment.Length, GustGeometry.Shrunk(segment, 0f).Length, Tolerance);
        }
    }
}
