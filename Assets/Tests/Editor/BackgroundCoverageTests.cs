using DetectiveDave.LevelPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class BackgroundCoverageTests
    {
        const float Size = 5f;
        const float Aspect = 2f;
        const float Zoom = 0.35f;
        const float Tolerance = 0.001f;

        static readonly ScreenRect Board = ScreenRect.Around(Vector2.zero, new Vector2(10f, 5f));

        static void AssertRect(ScreenRect rect, Vector2 min, Vector2 max)
        {
            Assert.AreEqual(min.x, rect.Min.x, Tolerance);
            Assert.AreEqual(min.y, rect.Min.y, Tolerance);
            Assert.AreEqual(max.x, rect.Max.x, Tolerance);
            Assert.AreEqual(max.y, rect.Max.y, Tolerance);
        }

        [Test]
        public void WithoutZoomTheCoverageIsTheHomeView()
        {
            AssertRect(BackgroundCoverage.Still(Vector2.zero, Size, Aspect, 0f), new Vector2(-10f, -5f), new Vector2(10f, 5f));
        }

        [Test]
        public void TheOverscanAddsThatShareOfTheSizeOnEachSide()
        {
            AssertRect(BackgroundCoverage.Still(Vector2.zero, Size, Aspect, 0.05f), new Vector2(-11f, -5.5f), new Vector2(11f, 5.5f));
        }

        [Test]
        public void ANegativeOverscanIsIgnored()
        {
            AssertRect(BackgroundCoverage.Still(Vector2.zero, Size, Aspect, -1f), new Vector2(-10f, -5f), new Vector2(10f, 5f));
        }

        [Test]
        public void TheHomeViewFollowsTheHomeCamera()
        {
            AssertRect(BackgroundCoverage.Still(new Vector2(3f, -2f), Size, Aspect, 0f), new Vector2(-7f, -7f), new Vector2(13f, 3f));
        }

        [Test]
        public void AWiderScreenWidensTheCoverage()
        {
            var wide = BackgroundCoverage.Still(Vector2.zero, Size, 21f / 9f, 0f);
            var narrow = BackgroundCoverage.Still(Vector2.zero, Size, 4f / 3f, 0f);
            Assert.AreEqual(5f * 21f / 9f, wide.Max.x, Tolerance);
            Assert.AreEqual(5f * 4f / 3f, narrow.Max.x, Tolerance);
            Assert.AreEqual(wide.Max.y, narrow.Max.y, Tolerance);
        }

        [Test]
        public void TheZoomedViewOnAnEdgeKeyStaysInsideTheCoverage()
        {
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, Board, Zoom, 0f);
            var cornerFrustumMax = Board.Max + new Vector2(10f, 5f) * Zoom;
            Assert.GreaterOrEqual(coverage.Max.x, cornerFrustumMax.x - Tolerance);
            Assert.GreaterOrEqual(coverage.Max.y, cornerFrustumMax.y - Tolerance);
        }

        [Test]
        public void TheCoverageReachesTheZoomedViewOnEveryCorner()
        {
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, Board, Zoom, 0f);
            AssertRect(coverage, new Vector2(-13.5f, -6.75f), new Vector2(13.5f, 6.75f));
        }

        [Test]
        public void ASmallBoardStillKeepsTheWholeHomeView()
        {
            var small = ScreenRect.Around(Vector2.zero, new Vector2(2f, 1f));
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, small, Zoom, 0f);
            AssertRect(coverage, new Vector2(-10f, -5f), new Vector2(10f, 5f));
        }

        [Test]
        public void ABoardSmallerThanTheHomeViewIsExpandedByTheZoomedHalfView()
        {
            var board = ScreenRect.Around(Vector2.zero, new Vector2(8f, 4f));
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, board, Zoom, 0f);
            AssertRect(coverage, new Vector2(-11.5f, -5.75f), new Vector2(11.5f, 5.75f));
        }

        [Test]
        public void AnOffCentreBoardMovesTheCoverageWithIt()
        {
            var board = ScreenRect.Around(new Vector2(4f, 0f), new Vector2(10f, 5f));
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, board, Zoom, 0f);
            AssertRect(coverage, new Vector2(-10f, -6.75f), new Vector2(17.5f, 6.75f));
        }

        [Test]
        public void AZoomFactorOfOneAddsTheWholeHomeViewAroundTheBoard()
        {
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, Board, 1f, 0f);
            AssertRect(coverage, new Vector2(-20f, -10f), new Vector2(20f, 10f));
        }

        [Test]
        public void TheOverscanAppliesToTheZoomedCoverageToo()
        {
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, Board, Zoom, 0.1f);
            AssertRect(coverage, new Vector2(-13.5f - 2.7f, -6.75f - 1.35f), new Vector2(13.5f + 2.7f, 6.75f + 1.35f));
        }

        [Test]
        public void ScreenRectCentreSizeAndExpansionAgree()
        {
            var rect = new ScreenRect(new Vector2(-1f, 2f), new Vector2(3f, 6f));
            Assert.AreEqual(new Vector2(1f, 4f), rect.Center);
            Assert.AreEqual(new Vector2(4f, 4f), rect.Size);
            AssertRect(rect.Expanded(new Vector2(1f, 2f)), new Vector2(-2f, 0f), new Vector2(4f, 8f));
        }
    }
}
