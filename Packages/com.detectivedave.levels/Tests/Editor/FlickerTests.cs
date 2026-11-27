using System.Collections.Generic;
using NUnit.Framework;

namespace DetectiveDave.Levels.Tests
{
    public class FlickerPatternTests
    {
        static FlickerPattern Pattern(params FlickerPoint[] points) => new FlickerPattern("test", 1f, points);

        [Test]
        public void AnEmptyPatternLeavesTheLightUnchanged() =>
            Assert.AreEqual(FlickerPattern.UnchangedBrightness, Pattern().Evaluate(0.5f));

        [Test]
        public void BrightnessMovesInAStraightLineBetweenTwoPoints() =>
            Assert.AreEqual(0.5f, Pattern(new FlickerPoint(0f, 1f), new FlickerPoint(1f, 0f)).Evaluate(0.5f), 0.0001f);

        [Test]
        public void BeforeTheFirstPointTheFirstValueHolds() =>
            Assert.AreEqual(0.2f, Pattern(new FlickerPoint(0.5f, 0.2f), new FlickerPoint(1f, 1f)).Evaluate(0.1f), 0.0001f);

        [Test]
        public void AfterTheLastPointTheLastValueHolds() =>
            Assert.AreEqual(0.3f, Pattern(new FlickerPoint(0f, 1f), new FlickerPoint(0.5f, 0.3f)).Evaluate(0.9f), 0.0001f);

        [Test]
        public void PointsOutOfOrderGiveTheSameAnswerAsPointsInOrder()
        {
            var inOrder = Pattern(new FlickerPoint(0f, 1f), new FlickerPoint(0.5f, 0f), new FlickerPoint(1f, 1f));
            var shuffled = Pattern(new FlickerPoint(1f, 1f), new FlickerPoint(0f, 1f), new FlickerPoint(0.5f, 0f));
            foreach (float time in new[] { 0f, 0.1f, 0.25f, 0.5f, 0.75f, 1f })
                Assert.AreEqual(inOrder.Evaluate(time), shuffled.Evaluate(time), 0.0001f, $"at {time}");
        }

        [Test]
        public void TwoPointsAtTheSameTimeJumpToTheLaterValue()
        {
            var pattern = Pattern(new FlickerPoint(0f, 1f), new FlickerPoint(0.5f, 1f), new FlickerPoint(0.5f, 0f), new FlickerPoint(1f, 0f));
            Assert.AreEqual(0f, pattern.Evaluate(0.6f), 0.0001f);
        }
    }

    public class FlickerPickerTests
    {
        static FlickerPattern Weighted(float weight) => new FlickerPattern("p", 1f, new FlickerPoint[0], weight);

        [Test]
        public void NothingCanBePickedFromNoPatterns() =>
            Assert.AreEqual(FlickerPicker.NoPattern, FlickerPicker.Pick(new List<FlickerPattern>(), 0.5f));

        [Test]
        public void NothingCanBePickedWhenEveryWeightIsZero() =>
            Assert.AreEqual(FlickerPicker.NoPattern, FlickerPicker.Pick(new[] { Weighted(0f), Weighted(0f) }, 0.5f));

        [Test]
        public void TheRandomNumberDecidesWhichPatternIsPicked()
        {
            var patterns = new[] { Weighted(1f), Weighted(1f), Weighted(2f) };
            Assert.AreEqual(0, FlickerPicker.Pick(patterns, 0.1f));
            Assert.AreEqual(1, FlickerPicker.Pick(patterns, 0.3f));
            Assert.AreEqual(2, FlickerPicker.Pick(patterns, 0.9f));
        }

        [Test]
        public void ARandomNumberOfExactlyOneStillPicksAPattern() =>
            Assert.AreEqual(1, FlickerPicker.Pick(new[] { Weighted(1f), Weighted(1f), Weighted(0f) }, 1f));

        [Test]
        public void APatternWithWeightZeroIsNeverPicked()
        {
            var patterns = new[] { Weighted(0f), Weighted(1f) };
            for (float random = 0f; random <= 1f; random += 0.1f)
                Assert.AreEqual(1, FlickerPicker.Pick(patterns, random));
        }
    }

    public class FlickerSchedulerTests
    {
        static readonly FlickerPattern Dip = new FlickerPattern("dip", 1f, new[] { new FlickerPoint(0f, 0f), new FlickerPoint(1f, 0f) });

        static FlickerScheduler Scheduler(bool enabled, float random = 0f, float min = 2f, float max = 2f, params FlickerPattern[] patterns) =>
            new FlickerScheduler(enabled, min, max, patterns.Length == 0 ? new[] { Dip } : patterns, () => random);

        [Test]
        public void TheLightStaysFullWhileWaiting()
        {
            var scheduler = Scheduler(true);
            Assert.AreEqual(1f, scheduler.Tick(1.9f));
            Assert.IsFalse(scheduler.IsFlickering);
        }

        [Test]
        public void AFlickerStartsWhenTheWaitIsOver()
        {
            var scheduler = Scheduler(true);
            scheduler.Tick(1.9f);
            Assert.AreEqual(0f, scheduler.Tick(0.2f));
            Assert.IsTrue(scheduler.IsFlickering);
        }

        [Test]
        public void AfterTheFlickerTheLightIsFullAndTheWaitStartsAgain()
        {
            var scheduler = Scheduler(true);
            scheduler.Tick(2.1f);
            Assert.AreEqual(1f, scheduler.Tick(1f));
            Assert.IsFalse(scheduler.IsFlickering);
            Assert.AreEqual(2f, scheduler.WaitLeft, 0.0001f);
        }

        [Test]
        public void AWaitIsARandomTimeBetweenTheShortestAndTheLongest()
        {
            Assert.AreEqual(3f, Scheduler(true, 0.5f, 2f, 4f).WaitLeft, 0.0001f);
            Assert.AreEqual(2f, Scheduler(true, 0f, 2f, 4f).WaitLeft, 0.0001f);
        }

        [Test]
        public void AReversedWaitRangeIsReadTheRightWayRound() =>
            Assert.AreEqual(2f, Scheduler(true, 0f, 4f, 2f).WaitLeft, 0.0001f);

        [Test]
        public void ADisabledFlickerNeverDimsTheLight()
        {
            var scheduler = Scheduler(false);
            for (int frame = 0; frame < 100; frame++) Assert.AreEqual(1f, scheduler.Tick(0.1f));
        }

        [Test]
        public void WithoutPatternsTheLightNeverDims()
        {
            var scheduler = new FlickerScheduler(true, 1f, 1f, new List<FlickerPattern>(), () => 0f);
            for (int frame = 0; frame < 50; frame++) Assert.AreEqual(1f, scheduler.Tick(0.1f));
        }

        [Test]
        public void ALongFrameCarriesItsLeftoverTimeIntoTheFlicker()
        {
            var ramp = new FlickerPattern("ramp", 1f, new[] { new FlickerPoint(0f, 0f), new FlickerPoint(1f, 1f) });
            var scheduler = Scheduler(true, 0f, 1f, 1f, ramp);
            Assert.AreEqual(0.5f, scheduler.Tick(1.5f), 0.0001f);
        }

        [Test]
        public void TheSettingsOfALevelFeedTheScheduler()
        {
            var settings = new LevelLightingSettings();
            var scheduler = new FlickerScheduler(settings, () => 0f);
            for (int frame = 0; frame < 50; frame++) Assert.AreEqual(1f, scheduler.Tick(1f));
        }
    }
}
