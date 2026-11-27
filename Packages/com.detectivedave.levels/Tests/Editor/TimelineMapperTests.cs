using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Levels.Tests
{
    public class TimelineMapperTests
    {
        static readonly Rect Area = new Rect(10f, 20f, 200f, 100f);

        static TimelineMapper Mapper(float duration = 2f) => new TimelineMapper(Area, duration);

        [Test]
        public void TimeRunsLeftToRight()
        {
            Assert.AreEqual(10f, Mapper().TimeToX(0f), 0.0001f);
            Assert.AreEqual(110f, Mapper().TimeToX(1f), 0.0001f);
            Assert.AreEqual(210f, Mapper().TimeToX(2f), 0.0001f);
        }

        [Test]
        public void FullBrightnessIsAtTheTop()
        {
            Assert.AreEqual(20f, Mapper().BrightnessToY(1f), 0.0001f);
            Assert.AreEqual(120f, Mapper().BrightnessToY(0f), 0.0001f);
        }

        [Test]
        public void ScreenAndTimelineValuesConvertBackAndForth()
        {
            var mapper = Mapper();
            Assert.AreEqual(0.75f, mapper.XToTime(mapper.TimeToX(0.75f)), 0.0001f);
            Assert.AreEqual(0.3f, mapper.YToBrightness(mapper.BrightnessToY(0.3f)), 0.0001f);
        }

        [Test]
        public void ValuesOutsideTheRectAreKeptInside()
        {
            var mapper = Mapper();
            Assert.AreEqual(2f, mapper.XToTime(5000f), 0.0001f);
            Assert.AreEqual(0f, mapper.XToTime(-5000f), 0.0001f);
            Assert.AreEqual(1f, mapper.YToBrightness(-100f), 0.0001f);
            Assert.AreEqual(0f, mapper.YToBrightness(5000f), 0.0001f);
        }

        [Test]
        public void ADurationOfZeroDoesNotDivideByZero() =>
            Assert.IsFalse(float.IsNaN(Mapper(0f).TimeToX(1f)));

        [Test]
        public void SnapRoundsToTheNearestStep()
        {
            Assert.AreEqual(0.15f, TimelineMapper.Snap(0.17f, 0.05f, true), 0.0001f);
            Assert.AreEqual(0.2f, TimelineMapper.Snap(0.18f, 0.05f, true), 0.0001f);
        }

        [Test]
        public void SnapDoesNothingWhenOffOrWithoutAStep()
        {
            Assert.AreEqual(0.17f, TimelineMapper.Snap(0.17f, 0.05f, false));
            Assert.AreEqual(0.17f, TimelineMapper.Snap(0.17f, 0f, true));
        }

        [Test]
        public void TheNearestPointInsideTheGrabRadiusIsFound()
        {
            var points = new[] { new Vector2(10f, 10f), new Vector2(50f, 50f), new Vector2(52f, 52f) };
            Assert.AreEqual(2, TimelineMapper.NearestPoint(points, new Vector2(53f, 53f), 10f));
        }

        [Test]
        public void NoPointIsFoundWhenAllAreFarAway() =>
            Assert.AreEqual(-1, TimelineMapper.NearestPoint(new[] { new Vector2(10f, 10f) }, new Vector2(100f, 100f), 10f));
    }
}
