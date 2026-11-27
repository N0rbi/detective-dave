using System.Linq;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class AnyClipPickerTests
    {
        [Test]
        public void In_order_plays_every_clip_in_turn_and_starts_again()
        {
            var picker = new AnyClipPicker(PianoOrder.InOrder, new UnityRandomSource(), 3);
            var picks = Enumerable.Range(0, 5).Select(_ => picker.Next(ChordMoment.Outside)).ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 0, 1 }, picks);
        }

        [Test]
        public void Random_and_chord_aware_pick_among_all_clips_and_never_repeat_in_a_row()
        {
            foreach (var order in new[] { PianoOrder.Random, PianoOrder.ChordAware })
            {
                var picker = new AnyClipPicker(order, new UnityRandomSource(), 3);
                var previous = picker.Next(ChordMoment.Outside);
                for (var count = 0; count < 200; count++)
                {
                    var next = picker.Next(ChordMoment.Outside);
                    Assert.AreNotEqual(previous, next);
                    Assert.That(next, Is.InRange(0, 2));
                    previous = next;
                }
            }
        }

        [Test]
        public void One_clip_plays_every_time()
        {
            foreach (var order in new[] { PianoOrder.InOrder, PianoOrder.Random, PianoOrder.ChordAware })
            {
                var picker = new AnyClipPicker(order, new UnityRandomSource(), 1);
                Assert.AreEqual(0, picker.Next(ChordMoment.Outside));
                Assert.AreEqual(0, picker.Next(ChordMoment.Outside));
            }
        }

        [Test]
        public void Without_clips_there_is_nothing_to_play()
        {
            foreach (var order in new[] { PianoOrder.InOrder, PianoOrder.Random, PianoOrder.ChordAware })
                Assert.AreEqual(PianoPicker.NoClip, new AnyClipPicker(order, new UnityRandomSource(), 0).Next(ChordMoment.Outside));
        }
    }
}
