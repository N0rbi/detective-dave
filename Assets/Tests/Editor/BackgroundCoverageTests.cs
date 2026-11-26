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

        [Test]
        public void ALiveViewBeyondThePlanKeepsTheCoverageGrowing()
        {
            var planned = BackgroundCoverage.Still(Vector2.zero, Size, Aspect, 0f);
            var live = ScreenRect.Around(new Vector2(14f, -8f), new Vector2(3f, 1.5f));
            AssertRect(BackgroundCoverage.IncludingLiveView(planned, live, 0f), new Vector2(-10f, -9.5f), new Vector2(17f, 5f));
        }

        [Test]
        public void ALiveViewInsideThePlanChangesNothing()
        {
            var planned = BackgroundCoverage.Still(Vector2.zero, Size, Aspect, 0f);
            var live = ScreenRect.Around(new Vector2(2f, 1f), new Vector2(3f, 1.5f));
            AssertRect(BackgroundCoverage.IncludingLiveView(planned, live, 0f), planned.Min, planned.Max);
        }

        [Test]
        public void EveryFrameOfTheZoomOnTheLeftBottomKeyStaysInsideTheCoverage()
        {
            var focus = Board.Min + new Vector2(0.5f, 0.5f);
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, Aspect, Board, Zoom, 0f);
            for (float progress = 0f; progress <= 1f; progress += 0.05f)
            {
                var position = Vector2.Lerp(Vector2.zero, focus, progress);
                var size = Mathf.Lerp(Size, Size * Zoom, progress);
                var view = ScreenRect.Around(position, new Vector2(size * Aspect, size));
                Assert.IsTrue(BackgroundTraceLine.ViewIsInside(view, coverage), "progress " + progress);
            }
        }

        [Test]
        public void EveryFrameOfTheZoomStaysInsideTheCoverageOnExtremeAspects([Values(0.5f, 1f, 2f, 3.5f)] float aspect)
        {
            var board = ScreenRect.Around(new Vector2(1f, -1f), new Vector2(9f, 3f));
            var coverage = BackgroundCoverage.Zooming(Vector2.zero, Size, aspect, board, Zoom, 0f);
            var targets = new[] { board.Min, board.Max, new Vector2(board.Min.x, board.Max.y), new Vector2(board.Max.x, board.Min.y) };
            foreach (var focus in targets)
                for (float progress = 0f; progress <= 1f; progress += 0.1f)
                {
                    var size = Mathf.Lerp(Size, Size * Zoom, progress);
                    var view = ScreenRect.Around(Vector2.Lerp(Vector2.zero, focus, progress), new Vector2(size * aspect, size));
                    Assert.IsTrue(BackgroundTraceLine.ViewIsInside(view, coverage));
                }
        }

        [Test]
        public void TheTraceLineListsEveryValueAndTheBackdropState()
        {
            var rect = new ScreenRect(new Vector2(-1f, -2f), new Vector2(3f, 4f));
            var line = BackgroundTraceLine.Format(1.5f, new Vector2(0.25f, 0f), 2f, 1.5f, rect, rect, rect, true);
            Assert.AreEqual("1.5;0.25;0;2;1.5;-1,-2;3,4;-1,-2;3,4;-1,-2;3,4;yes", line);
        }

        [Test]
        public void AViewBeyondThePictureIsOutside()
        {
            var sprite = new ScreenRect(Vector2.zero, new Vector2(4f, 4f));
            Assert.IsFalse(BackgroundTraceLine.ViewIsInside(new ScreenRect(new Vector2(-0.1f, 1f), new Vector2(2f, 2f)), sprite));
            Assert.IsTrue(BackgroundTraceLine.ViewIsInside(new ScreenRect(new Vector2(1f, 1f), new Vector2(2f, 2f)), sprite));
        }
    }
}
