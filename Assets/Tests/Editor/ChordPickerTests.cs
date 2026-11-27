using System.Collections.Generic;
using System.Linq;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class ChordPickerTests
    {
        static readonly ChordMoment AmStrong = new ChordMoment(0, 0, true, true);
        static readonly ChordMoment DmStrong = new ChordMoment(1, 0, true, true);
        static readonly ChordMoment DmWeak = new ChordMoment(1, 1, false, false);
        static readonly ChordMoment E7Strong = new ChordMoment(3, 0, true, true);

        sealed class ScriptedRandom : IRandomSource
        {
            readonly Queue<float> ranges;
            readonly Queue<int> indexes;

            public ScriptedRandom(IEnumerable<float> ranges = null, IEnumerable<int> indexes = null)
            {
                this.ranges = new Queue<float>(ranges ?? new float[0]);
                this.indexes = new Queue<int>(indexes ?? new int[0]);
            }

            public float LastMax { get; private set; }

            public float Range(float min, float max)
            {
                LastMax = max;
                return ranges.Count > 0 ? ranges.Dequeue() : min;
            }

            public int Index(int count) => indexes.Count > 0 ? indexes.Dequeue() : 0;
        }

        sealed class SilentPicker : IPianoPicker
        {
            public int Next(ChordMoment moment) => PianoPicker.NoClip;
        }

        [Test]
        public void The_chord_aware_picker_never_leaves_the_scale_of_the_chord()
        {
            var palette = ChordFixtures.Palette();
            var picker = new ChordAwarePicker(new UnityRandomSource(), palette, 3f);
            for (var round = 0; round < 400; round++)
                for (var segment = 0; segment < 8; segment++)
                    CollectionAssert.Contains(palette.Allowed(segment), picker.Next(new ChordMoment(segment, round % 4, round % 2 == 0, round % 8 == 0)));
        }

        [Test]
        public void The_chord_aware_picker_never_repeats_the_clip_it_just_played()
        {
            var picker = new ChordAwarePicker(new UnityRandomSource(), ChordFixtures.Palette(), 3f);
            var previous = picker.Next(AmStrong);
            for (var count = 0; count < 300; count++)
            {
                var next = picker.Next(AmStrong);
                Assert.AreNotEqual(previous, next);
                previous = next;
            }
        }

        [Test]
        public void The_chord_aware_picker_repeats_a_clip_when_it_is_the_only_one_that_fits()
        {
            var palette = new ChordPalette(ChordFixtures.Segments(), new[] { PitchClassSet.Of(new[] { 0, 4, 7 }) });
            var picker = new ChordAwarePicker(new UnityRandomSource(), palette, 3f);
            Assert.AreEqual(0, picker.Next(AmStrong));
            Assert.AreEqual(0, picker.Next(AmStrong));
        }

        [Test]
        public void A_chord_tone_clip_is_three_times_as_likely_on_a_strong_beat()
        {
            var random = new ScriptedRandom(new[] { 19.5f });
            var picker = new ChordAwarePicker(random, ChordFixtures.Palette(), 3f);
            Assert.AreEqual(9, picker.Next(DmStrong));
            Assert.AreEqual(20f, random.LastMax, 0.001f);
        }

        [Test]
        public void A_clip_that_only_fits_the_scale_has_weight_one()
        {
            var picker = new ChordAwarePicker(new ScriptedRandom(new[] { 0.5f }), ChordFixtures.Palette(), 3f);
            Assert.AreEqual(0, picker.Next(DmStrong));
        }

        [Test]
        public void On_a_weak_beat_the_chord_tone_weight_is_halfway()
        {
            var random = new ScriptedRandom(new[] { 13.5f });
            var picker = new ChordAwarePicker(random, ChordFixtures.Palette(), 3f);
            Assert.AreEqual(9, picker.Next(DmWeak));
            Assert.AreEqual(14f, random.LastMax, 0.001f);
        }

        [Test]
        public void A_chord_tone_weight_of_one_treats_every_clip_the_same()
        {
            var random = new ScriptedRandom(new[] { 0f });
            new ChordAwarePicker(random, ChordFixtures.Palette(), 1f).Next(DmStrong);
            Assert.AreEqual(8f, random.LastMax, 0.001f);
        }

        [Test]
        public void The_chord_aware_picker_gives_no_clip_when_nothing_fits()
        {
            var picker = new ChordAwarePicker(new UnityRandomSource(), ChordFixtures.Palette(), 3f);
            Assert.AreEqual(PianoPicker.NoClip, picker.Next(ChordMoment.Outside));
        }

        [Test]
        public void In_order_takes_the_next_clip_that_fits_and_skips_the_others()
        {
            var picker = new InOrderChordPicker(new SequentialPianoPicker(12), ChordFixtures.Palette());
            Assert.AreEqual(0, picker.Next(AmStrong));
            var overE7 = Enumerable.Range(0, 6).Select(_ => picker.Next(E7Strong)).ToArray();
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 7 }, overE7);
        }

        [Test]
        public void In_order_goes_back_to_the_first_clip_after_the_last()
        {
            var picker = new InOrderChordPicker(new SequentialPianoPicker(12), ChordFixtures.Palette());
            var picks = Enumerable.Range(0, 4).Select(_ => picker.Next(AmStrong)).ToArray();
            CollectionAssert.AreEqual(new[] { 0, 6, 0, 6 }, picks);
        }

        [Test]
        public void In_order_gives_no_clip_when_nothing_fits_and_keeps_its_place()
        {
            var picker = new InOrderChordPicker(new SequentialPianoPicker(12), ChordFixtures.Palette());
            picker.Next(E7Strong);
            Assert.AreEqual(PianoPicker.NoClip, picker.Next(ChordMoment.Outside));
            Assert.AreEqual(2, picker.Next(E7Strong));
        }

        [Test]
        public void Random_picks_only_fitting_clips_and_never_the_same_twice()
        {
            var random = new ScriptedRandom(null, new[] { 1, 0 });
            var picker = new RandomChordPicker(new PianoPicker(random, 12), ChordFixtures.Palette());
            Assert.AreEqual(6, picker.Next(AmStrong));
            Assert.AreEqual(0, picker.Next(AmStrong));
        }

        [Test]
        public void Random_never_leaves_the_scale_of_the_chord()
        {
            var palette = ChordFixtures.Palette();
            var picker = new RandomChordPicker(new PianoPicker(new UnityRandomSource(), 12), palette);
            for (var round = 0; round < 400; round++)
                CollectionAssert.Contains(palette.Allowed(3), picker.Next(E7Strong));
        }

        [Test]
        public void The_order_setting_chooses_the_picker()
        {
            var palette = ChordFixtures.Palette();
            var random = new UnityRandomSource();
            Assert.IsInstanceOf<InOrderChordPicker>(PianoPickers.For(PianoOrder.InOrder, random, palette, 3f));
            Assert.IsInstanceOf<RandomChordPicker>(PianoPickers.For(PianoOrder.Random, random, palette, 3f));
            Assert.IsInstanceOf<ChordAwarePicker>(PianoPickers.For(PianoOrder.ChordAware, random, palette, 3f));
        }

        [Test]
        public void A_silent_chord_warns_once_and_names_the_chord()
        {
            var warnings = new List<string>();
            var picker = new WarnOnSilentChordPicker(new SilentPicker(), ChordFixtures.Palette(), warnings.Add);
            picker.Next(E7Strong);
            picker.Next(E7Strong);
            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains("E7", warnings[0]);
        }

        [Test]
        public void A_different_silent_chord_warns_again()
        {
            var warnings = new List<string>();
            var picker = new WarnOnSilentChordPicker(new SilentPicker(), ChordFixtures.Palette(), warnings.Add);
            picker.Next(E7Strong);
            picker.Next(AmStrong);
            Assert.AreEqual(2, warnings.Count);
        }

        [Test]
        public void A_position_outside_the_chart_warns_once()
        {
            var warnings = new List<string>();
            var picker = new WarnOnSilentChordPicker(new SilentPicker(), ChordFixtures.Palette(), warnings.Add);
            picker.Next(ChordMoment.Outside);
            picker.Next(ChordMoment.Outside);
            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains("outside", warnings[0]);
        }

        [Test]
        public void A_clip_that_plays_causes_no_warning()
        {
            var warnings = new List<string>();
            var picker = new WarnOnSilentChordPicker(new ChordAwarePicker(new UnityRandomSource(), ChordFixtures.Palette(), 3f), ChordFixtures.Palette(), warnings.Add);
            picker.Next(E7Strong);
            Assert.IsEmpty(warnings);
        }
    }
}
