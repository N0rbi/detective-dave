using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class BackgroundFitTests
    {
        static readonly Vector2 View = new Vector2(16f, 9f);
        static readonly Vector2 Portrait = new Vector2(1.41f, 2.118f);
        static readonly ScreenRect ViewRect = ScreenRect.Around(Vector2.zero, View * 0.5f);

        [System.Serializable]
        sealed class PlacementData
        {
            public bool visible = true;
            public Vector2 offset;
            public float zoom;
        }

        static BackgroundPlacement Placement(Vector2 offset, float zoom)
        {
            var placement = new BackgroundPlacement();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(new PlacementData { offset = offset, zoom = zoom }), placement);
            return placement;
        }

        [Test]
        public void ThePictureCoversTheCoverageAtZoomOne()
        {
            var fit = BackgroundFit.Of(Portrait, ViewRect, View, new BackgroundPlacement());
            Assert.AreEqual(View.x / Portrait.x, fit.Scale, 0.001f);
        }

        [Test]
        public void NoOffsetKeepsThePictureCentredOnTheCoverage()
        {
            var coverage = ScreenRect.Around(new Vector2(3f, -2f), View * 0.5f);
            var fit = BackgroundFit.Of(Portrait, coverage, View, new BackgroundPlacement());
            Assert.AreEqual(new Vector2(3f, -2f), fit.Center);
        }

        [Test]
        public void ZoomMakesThePictureLarger()
        {
            var fit = BackgroundFit.Of(Portrait, ViewRect, View, Placement(Vector2.zero, 2f));
            Assert.AreEqual(2f * View.x / Portrait.x, fit.Scale, 0.001f);
        }

        [Test]
        public void TheOffsetNeverShowsAnEdge()
        {
            var fit = BackgroundFit.Of(Portrait, ViewRect, View, Placement(new Vector2(1f, 1f), 1f));
            Assert.AreEqual(0f, fit.Center.x, 0.001f);
            var spareHeight = Portrait.y * fit.Scale - View.y;
            Assert.AreEqual(spareHeight * 0.5f, fit.Center.y, 0.001f);
        }

        [Test]
        public void ASmallOffsetMovesByThatShareOfTheReferenceView()
        {
            var fit = BackgroundFit.Of(Portrait, ViewRect, View, Placement(new Vector2(0f, 0.1f), 1f));
            Assert.AreEqual(0.9f, fit.Center.y, 0.001f);
        }

        [Test]
        public void TheReferenceViewAndNotTheCoverageSetsTheOffsetDistance()
        {
            var wideCoverage = ScreenRect.Around(Vector2.zero, View);
            var fit = BackgroundFit.Of(Portrait, wideCoverage, View, Placement(new Vector2(0f, 0.1f), 1f));
            Assert.AreEqual(0.9f, fit.Center.y, 0.001f);
        }

        [Test]
        public void ThePictureStillCoversAWideCoverageAroundAPortraitPicture()
        {
            var wideCoverage = ScreenRect.Around(Vector2.zero, new Vector2(30f, 8f));
            var fit = BackgroundFit.Of(Portrait, wideCoverage, View, new BackgroundPlacement());
            Assert.GreaterOrEqual(Portrait.x * fit.Scale, wideCoverage.Size.x - 0.001f);
            Assert.GreaterOrEqual(Portrait.y * fit.Scale, wideCoverage.Size.y - 0.001f);
        }

        [Test]
        public void ThePictureCoversATallCoverageAroundAWidePicture()
        {
            var widePicture = new Vector2(4f, 1f);
            var tallCoverage = ScreenRect.Around(Vector2.zero, new Vector2(5f, 12f));
            var fit = BackgroundFit.Of(widePicture, tallCoverage, View, new BackgroundPlacement());
            Assert.AreEqual(tallCoverage.Size.y / widePicture.y, fit.Scale, 0.001f);
            Assert.GreaterOrEqual(widePicture.x * fit.Scale, tallCoverage.Size.x);
        }

        [Test]
        public void AnOffsetBeyondTheSpareRoomStopsAtTheCoverageEdge()
        {
            var fit = BackgroundFit.Of(Portrait, ViewRect, View, Placement(new Vector2(5f, -5f), 1f));
            var spare = Portrait * fit.Scale - View;
            Assert.AreEqual(0f, fit.Center.x, 0.001f);
            Assert.AreEqual(-spare.y * 0.5f, fit.Center.y, 0.001f);
        }

        [Test]
        public void MirroringDoesNotMoveTheCentreOfTheDrawnPicture()
        {
            var boundsCenter = new Vector2(0.3f, -0.2f);
            var wanted = new Vector2(2f, 1f);
            var plain = BackgroundFit.PivotPosition(wanted, boundsCenter, false, false, 4f);
            var mirrored = BackgroundFit.PivotPosition(wanted, boundsCenter, true, true, 4f);
            Assert.AreEqual(wanted, plain + boundsCenter * 4f);
            Assert.AreEqual(wanted, mirrored + new Vector2(-boundsCenter.x, -boundsCenter.y) * 4f);
        }

        [Test]
        public void ACentredPictureHasItsPivotOnTheWantedCentre()
        {
            Assert.AreEqual(new Vector2(2f, 1f), BackgroundFit.PivotPosition(new Vector2(2f, 1f), Vector2.zero, true, false, 3f));
        }

        [Test]
        public void ANewPlacementIsVisibleAndNotFlipped()
        {
            var placement = new BackgroundPlacement();
            Assert.IsTrue(placement.Visible);
            Assert.IsFalse(placement.FlipX);
            Assert.IsFalse(placement.FlipY);
            Assert.AreEqual(1f, placement.Zoom);
        }
    }
}
