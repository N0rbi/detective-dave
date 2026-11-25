using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class BackgroundFitTests
    {
        static readonly Vector2 View = new Vector2(16f, 9f);
        static readonly Vector2 Portrait = new Vector2(1.41f, 2.118f);

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
        public void ThePictureCoversTheViewAtZoomOne()
        {
            var fit = BackgroundFit.Of(Portrait, View, new BackgroundPlacement());
            Assert.AreEqual(View.x / Portrait.x, fit.Scale, 0.001f);
        }

        [Test]
        public void NoOffsetKeepsThePictureCentred()
        {
            var fit = BackgroundFit.Of(Portrait, View, new BackgroundPlacement());
            Assert.AreEqual(Vector2.zero, fit.Shift);
        }

        [Test]
        public void ZoomMakesThePictureLarger()
        {
            var fit = BackgroundFit.Of(Portrait, View, Placement(Vector2.zero, 2f));
            Assert.AreEqual(2f * View.x / Portrait.x, fit.Scale, 0.001f);
        }

        [Test]
        public void TheOffsetNeverShowsAnEdge()
        {
            var fit = BackgroundFit.Of(Portrait, View, Placement(new Vector2(1f, 1f), 1f));
            Assert.AreEqual(0f, fit.Shift.x, 0.001f);
            var spareHeight = Portrait.y * fit.Scale - View.y;
            Assert.AreEqual(spareHeight * 0.5f, fit.Shift.y, 0.001f);
        }

        [Test]
        public void ASmallOffsetMovesByThatShareOfTheView()
        {
            var fit = BackgroundFit.Of(Portrait, View, Placement(new Vector2(0f, 0.1f), 1f));
            Assert.AreEqual(0.9f, fit.Shift.y, 0.001f);
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
