using System.Collections.Generic;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PianoPickerTests
    {
        sealed class ScriptedIndexes : IRandomSource
        {
            readonly Queue<int> indexes;

            public ScriptedIndexes(params int[] indexes) => this.indexes = new Queue<int>(indexes);

            public int LastCount { get; private set; }

            public float Range(float min, float max) => min;

            public int Index(int count)
            {
                LastCount = count;
                return indexes.Dequeue();
            }
        }

        [Test]
        public void No_clips_gives_no_clip()
        {
            var picker = new PianoPicker(new ScriptedIndexes(), 0);
            Assert.AreEqual(PianoPicker.NoClip, picker.Next());
            Assert.AreEqual(PianoPicker.NoClip, picker.Next());
        }

        [Test]
        public void One_clip_is_picked_every_time()
        {
            var picker = new PianoPicker(new ScriptedIndexes(), 1);
            Assert.AreEqual(0, picker.Next());
            Assert.AreEqual(0, picker.Next());
        }

        [Test]
        public void The_first_pick_can_be_any_clip()
        {
            var random = new ScriptedIndexes(2);
            var picker = new PianoPicker(random, 3);
            Assert.AreEqual(2, picker.Next());
            Assert.AreEqual(3, random.LastCount);
        }

        [Test]
        public void The_next_pick_asks_for_one_fewer_clip()
        {
            var random = new ScriptedIndexes(1, 0);
            var picker = new PianoPicker(random, 3);
            picker.Next();
            picker.Next();
            Assert.AreEqual(2, random.LastCount);
        }

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        public void Picks_at_or_after_the_last_clip_skip_over_it(int scripted, int expected)
        {
            var picker = new PianoPicker(new ScriptedIndexes(0, scripted), 3);
            Assert.AreEqual(0, picker.Next());
            Assert.AreEqual(expected, picker.Next());
        }

        [Test]
        public void A_pick_before_the_last_clip_is_kept()
        {
            var picker = new PianoPicker(new ScriptedIndexes(2, 1), 3);
            picker.Next();
            Assert.AreEqual(1, picker.Next());
        }

        [Test]
        public void The_same_clip_never_plays_twice_in_a_row()
        {
            const int clipCount = 4;
            var picker = new PianoPicker(new UnityRandomSource(), clipCount);
            var previous = picker.Next();
            for (var count = 0; count < 500; count++)
            {
                var next = picker.Next();
                Assert.AreNotEqual(previous, next);
                Assert.That(next, Is.InRange(0, clipCount - 1));
                previous = next;
            }
        }
    }
}
