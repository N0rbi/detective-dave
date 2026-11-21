using System.Collections.Generic;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class SoundVarianceTests
    {
        sealed class ScriptedRandom : IRandomSource
        {
            readonly Queue<float> ranges;
            readonly Queue<int> indexes;

            public ScriptedRandom(IEnumerable<float> ranges, IEnumerable<int> indexes)
            {
                this.ranges = new Queue<float>(ranges);
                this.indexes = new Queue<int>(indexes);
            }

            public float Range(float min, float max) => min + (max - min) * ranges.Dequeue();

            public int Index(int count) => indexes.Dequeue();
        }

        [Test]
        public void Pitch_and_volume_stay_inside_their_ranges()
        {
            var variance = new StrikeVariance(new ScriptedRandom(new[] { 0f, 0f, 1f, 1f, 0.5f, 0.5f }, new[] { 0, 0, 0 }), 4, 0.94f, 1.06f, 0.85f, 1f);
            var low = variance.Next();
            var high = variance.Next();
            var middle = variance.Next();
            Assert.AreEqual(0.94f, low.Pitch, 0.0001f);
            Assert.AreEqual(0.85f, low.VolumeShare, 0.0001f);
            Assert.AreEqual(1.06f, high.Pitch, 0.0001f);
            Assert.AreEqual(1f, high.VolumeShare, 0.0001f);
            Assert.AreEqual(1f, middle.Pitch, 0.0001f);
            Assert.AreEqual(0.925f, middle.VolumeShare, 0.0001f);
        }

        [Test]
        public void Swapped_ranges_are_sorted()
        {
            var variance = new StrikeVariance(new ScriptedRandom(new[] { 0f, 0f }, new[] { 0 }), 2, 1.06f, 0.94f, 1f, 0.85f);
            var strike = variance.Next();
            Assert.AreEqual(0.94f, strike.Pitch, 0.0001f);
            Assert.AreEqual(0.85f, strike.VolumeShare, 0.0001f);
        }

        [Test]
        public void The_same_clip_is_never_picked_twice_in_a_row()
        {
            var variance = new StrikeVariance(new ScriptedRandom(new float[8], new[] { 2, 1, 1, 0 }), 3, 1f, 1f, 1f, 1f);
            var first = variance.Next();
            var second = variance.Next();
            var third = variance.Next();
            var fourth = variance.Next();
            Assert.AreEqual(2, first.ClipIndex);
            Assert.AreNotEqual(first.ClipIndex, second.ClipIndex);
            Assert.AreNotEqual(second.ClipIndex, third.ClipIndex);
            Assert.AreNotEqual(third.ClipIndex, fourth.ClipIndex);
        }

        [Test]
        public void Every_other_clip_can_follow_a_clip()
        {
            for (int last = 0; last < 3; last++)
            {
                var seen = new HashSet<int>();
                for (int pick = 0; pick < 2; pick++)
                {
                    var variance = new StrikeVariance(new ScriptedRandom(new float[4], new[] { last, pick }), 3, 1f, 1f, 1f, 1f);
                    variance.Next();
                    seen.Add(variance.Next().ClipIndex);
                }
                Assert.AreEqual(2, seen.Count);
                Assert.IsFalse(seen.Contains(last));
            }
        }

        [Test]
        public void A_single_clip_repeats_and_no_clips_pick_zero()
        {
            var single = new StrikeVariance(new ScriptedRandom(new float[4], new int[0]), 1, 1f, 1f, 1f, 1f);
            Assert.AreEqual(0, single.Next().ClipIndex);
            Assert.AreEqual(0, single.Next().ClipIndex);
        }

        [Test]
        public void The_gate_lets_a_click_through_only_after_the_minimum_interval()
        {
            var gate = new ClickGate(0.03f);
            Assert.IsTrue(gate.TryPass(0f));
            Assert.IsFalse(gate.TryPass(0.02f));
            Assert.IsTrue(gate.TryPass(0.031f));
            Assert.IsFalse(gate.TryPass(0.05f));
            Assert.IsTrue(gate.TryPass(0.07f));
        }

        [Test]
        public void The_description_asks_for_a_click_per_revealing_frame_within_the_interval()
        {
            var reveal = new TimedReveal(1000, 0.025f, 3f);
            var gate = new ClickGate(0.03f);
            int clicks = Tick(reveal, gate, 60, 1f / 60f);
            Assert.GreaterOrEqual(clicks, 20);
            Assert.LessOrEqual(clicks, 34);
        }

        [Test]
        public void Skipping_types_faster_but_clicks_stay_within_the_interval()
        {
            var reveal = new TimedReveal(1000, 0.025f, 3f);
            reveal.Skip();
            var gate = new ClickGate(0.03f);
            int clicks = Tick(reveal, gate, 60, 1f / 60f);
            Assert.GreaterOrEqual(reveal.VisibleCount, 100);
            Assert.LessOrEqual(clicks, 34);
        }

        static int Tick(TimedReveal reveal, ClickGate gate, int frames, float delta)
        {
            int clicks = 0;
            for (int frame = 0; frame < frames; frame++)
            {
                if (reveal.Advance(delta) > 0 && gate.TryPass(frame * delta)) clicks++;
            }
            return clicks;
        }
    }
}
