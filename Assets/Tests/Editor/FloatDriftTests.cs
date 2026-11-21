using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public sealed class FloatDriftTests
    {
        sealed class SteppingRandom : IRandomSource
        {
            float next;

            public float Range(float min, float max)
            {
                next = Mathf.Repeat(next + 0.37f, 1f);
                return Mathf.Lerp(min, max, next);
            }

            public int Index(int count) => 0;
        }

        static readonly CaseFloatSettings Settings = new CaseFloatSettings(true, 0.03f, 0.08f, 2.5f, 4.5f, 1.2f);

        [Test]
        public void TwoRolledDriftsDoNotMoveTogether()
        {
            var random = new SteppingRandom();
            var first = FloatDrift.Roll(Settings, random);
            var second = FloatDrift.Roll(Settings, random);
            Assert.That(first.OffsetInKeys(1.3f), Is.Not.EqualTo(second.OffsetInKeys(1.3f)));
        }

        [Test]
        public void OffsetNeverExceedsTheLargestAmplitude()
        {
            var drift = FloatDrift.Roll(Settings, new SteppingRandom());
            for (float seconds = 0f; seconds < 30f; seconds += 0.1f)
            {
                var offset = drift.OffsetInKeys(seconds);
                Assert.That(Mathf.Abs(offset.x), Is.LessThanOrEqualTo(Settings.AmplitudeMaxInKeys));
                Assert.That(Mathf.Abs(offset.y), Is.LessThanOrEqualTo(Settings.AmplitudeMaxInKeys));
            }
        }

        [Test]
        public void SwayNeverExceedsTheLargestAngle()
        {
            var drift = FloatDrift.Roll(Settings, new SteppingRandom());
            for (float seconds = 0f; seconds < 30f; seconds += 0.1f) Assert.That(Mathf.Abs(drift.SwayDegreesAt(seconds)), Is.LessThanOrEqualTo(Settings.SwayDegreesMax));
        }

        [Test]
        public void ZeroAmplitudeAndSwayStayStill()
        {
            var still = new CaseFloatSettings(true, 0f, 0f, 3f, 3f, 0f);
            var drift = FloatDrift.Roll(still, new SteppingRandom());
            Assert.That(drift.OffsetInKeys(1.7f), Is.EqualTo(Vector2.zero));
            Assert.That(drift.SwayDegreesAt(1.7f), Is.EqualTo(0f));
        }

        [Test]
        public void TheOffsetRepeatsAfterOneCycleOfTheVerticalBob()
        {
            var drift = new FloatDrift(0.4f, 1f, 2f, 2f, 0.1f, 0f);
            Assert.That(drift.OffsetInKeys(0.5f).y, Is.EqualTo(drift.OffsetInKeys(2.5f).y).Within(0.0001f));
        }
    }
}
