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

            public void Play(NoteKind kind, int clipIndex, float volume) => Clips.Add(clipIndex);
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
        public void A_pick_that_must_fit_skips_the_clips_that_do_not()
        {
            var picker = new SequentialPianoPicker(4);
            var picks = new[] { picker.Next(clip => clip % 2 == 1), picker.Next(clip => clip % 2 == 1), picker.Next(clip => clip % 2 == 1) };
            CollectionAssert.AreEqual(new[] { 1, 3, 1 }, picks);
        }

        [Test]
        public void When_no_clip_fits_the_place_in_the_order_is_kept()
        {
            var picker = new SequentialPianoPicker(3);
            picker.Next();
            Assert.AreEqual(PianoPicker.NoClip, picker.Next(_ => false));
            Assert.AreEqual(1, picker.Next());
        }

        [Test]
        public void A_blocked_move_also_takes_the_next_clip()
        {
            var voices = new RecordingVoices();
            var reaction = new PianoReaction(new InOrderChordPicker(new SequentialPianoPicker(12), ChordFixtures.Palette()), new AnyClipPicker(PianoOrder.InOrder, new NoRandom(), 0), new DebounceGate(0f, true), new PianoNoteVolume(new NoRandom(), 0.8f, 0f, 0.5f), new ClickGate(0f), voices, () => 0f, () => new ChordMoment(1, 0, true, true), _ => { });
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            reaction.OnMoveAttempted(MoveAttempt.Blocked);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, voices.Clips);
        }
    }
}
