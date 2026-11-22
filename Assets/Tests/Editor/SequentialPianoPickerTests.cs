using System.Collections.Generic;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class SequentialPianoPickerTests
    {
        sealed class NoRandom : IRandomSource
        {
            public float Range(float min, float max) => (min + max) / 2f;

            public int Index(int count) => 0;
        }

        sealed class RecordingVoices : IPianoVoices
        {
            public readonly List<int> Clips = new List<int>();

            public void Play(int clipIndex, float volume) => Clips.Add(clipIndex);
        }

        [Test]
        public void No_clips_gives_no_clip()
        {
            Assert.AreEqual(PianoPicker.NoClip, new SequentialPianoPicker(0).Next());
        }

        [Test]
        public void One_clip_is_picked_every_time()
        {
            var picker = new SequentialPianoPicker(1);
            Assert.AreEqual(0, picker.Next());
            Assert.AreEqual(0, picker.Next());
        }

        [Test]
        public void Each_pick_takes_the_next_clip_and_goes_back_to_the_first_after_the_last()
        {
            var picker = new SequentialPianoPicker(3);
            var picks = new[] { picker.Next(), picker.Next(), picker.Next(), picker.Next(), picker.Next() };
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 0, 1 }, picks);
        }

        [Test]
        public void In_order_gives_the_sequential_picker()
        {
            Assert.IsInstanceOf<SequentialPianoPicker>(PianoSequences.For(PianoOrder.InOrder, new NoRandom(), 3));
        }

        [Test]
        public void Random_gives_the_random_picker()
        {
            Assert.IsInstanceOf<PianoPicker>(PianoSequences.For(PianoOrder.Random, new NoRandom(), 3));
        }

        [Test]
        public void A_blocked_move_also_takes_the_next_clip()
        {
            var voices = new RecordingVoices();
            var reaction = new PianoReaction(new SequentialPianoPicker(3), new PianoNoteVolume(new NoRandom(), 0.8f, 0f, 0.5f), new ClickGate(0f), voices, () => 0f);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            reaction.OnMoveAttempted(MoveAttempt.Blocked);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, voices.Clips);
        }
    }
}
