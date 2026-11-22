using System.Collections.Generic;
using System.Linq;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class GuaranteedClipPickerTests
    {
        static readonly ChordMoment OverC = new ChordMoment(0, 0, true, true);

        sealed class SilentPicker : IPianoPicker
        {
            public int Next(ChordMoment moment) => PianoPicker.NoClip;
        }

        sealed class ScriptedPicker : IPianoPicker
        {
            readonly Queue<int> clips;

            public ScriptedPicker(params int[] clips) => this.clips = new Queue<int>(clips);

            public int Next(ChordMoment moment) => clips.Dequeue();
        }

        static ChordPalette PaletteWhereNothingFits() => new ChordPalette(
            new[] { new ChordSegment("C", 0f, 4f, new[] { 0, 4, 7 }, new[] { 0, 4, 7 }) },
            new[]
            {
                PitchClassSet.Of(new[] { 5, 6, 8 }),
                PitchClassSet.Of(new[] { 0, 5, 6 }),
                PitchClassSet.Of(new[] { 0, 4, 9 }),
                PitchClassSet.Of(new[] { 0, 4, 10 })
            });

        static GuaranteedClipPicker Silent(ChordPalette palette, WhenNoClipFits rule) => new GuaranteedClipPicker(new SilentPicker(), palette, rule);

        [Test]
        public void The_clip_of_the_inner_picker_is_kept()
        {
            var picker = new GuaranteedClipPicker(new ScriptedPicker(1), PaletteWhereNothingFits(), WhenNoClipFits.PlayClosestToChord);
            Assert.AreEqual(1, picker.Next(OverC));
        }

        [Test]
        public void The_closest_clip_is_the_one_with_the_most_notes_inside_the_scale()
        {
            Assert.AreEqual(2, Silent(PaletteWhereNothingFits(), WhenNoClipFits.PlayClosestToChord).Next(OverC));
        }

        [Test]
        public void A_tie_in_the_closest_rule_goes_to_the_lowest_clip()
        {
            var palette = PaletteWhereNothingFits();
            Assert.AreEqual(2, palette.Closest(0, PianoPicker.NoClip));
        }

        [Test]
        public void The_closest_rule_never_plays_the_clip_it_just_played()
        {
            var picker = Silent(PaletteWhereNothingFits(), WhenNoClipFits.PlayClosestToChord);
            CollectionAssert.AreEqual(new[] { 2, 3, 2, 3 }, new[] { picker.Next(OverC), picker.Next(OverC), picker.Next(OverC), picker.Next(OverC) });
        }

        [Test]
        public void The_next_clip_rule_goes_through_the_clips_in_order_and_starts_again()
        {
            var picker = Silent(PaletteWhereNothingFits(), WhenNoClipFits.PlayNextClip);
            var picks = Enumerable.Range(0, 5).Select(_ => picker.Next(OverC)).ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 0 }, picks);
        }

        [Test]
        public void A_position_outside_the_chart_plays_the_next_clip_whatever_the_rule()
        {
            var picker = Silent(PaletteWhereNothingFits(), WhenNoClipFits.PlayClosestToChord);
            Assert.AreEqual(0, picker.Next(ChordMoment.Outside));
            Assert.AreEqual(1, picker.Next(ChordMoment.Outside));
        }

        [Test]
        public void One_clip_plays_every_time_under_both_rules()
        {
            var palette = new ChordPalette(ChordFixtures.Segments(), new[] { PitchClassSet.Of(new[] { 2, 8, 11 }) });
            foreach (var rule in new[] { WhenNoClipFits.PlayClosestToChord, WhenNoClipFits.PlayNextClip })
            {
                var picker = Silent(palette, rule);
                Assert.AreEqual(0, picker.Next(OverC));
                Assert.AreEqual(0, picker.Next(ChordMoment.Outside));
                Assert.AreEqual(0, picker.Next(OverC));
            }
        }

        [Test]
        public void Without_any_clip_there_is_nothing_to_play()
        {
            var palette = new ChordPalette(ChordFixtures.Segments(), new PitchClassSet[0]);
            Assert.AreEqual(PianoPicker.NoClip, Silent(palette, WhenNoClipFits.PlayClosestToChord).Next(OverC));
            Assert.AreEqual(PianoPicker.NoClip, Silent(palette, WhenNoClipFits.PlayNextClip).Next(ChordMoment.Outside));
        }

        [Test]
        public void With_clips_every_moment_of_the_chart_and_outside_it_gives_a_clip()
        {
            foreach (var rule in new[] { WhenNoClipFits.PlayClosestToChord, WhenNoClipFits.PlayNextClip })
            {
                var picker = Silent(ChordFixtures.Palette(), rule);
                for (var segment = -1; segment < 8; segment++)
                    Assert.AreNotEqual(PianoPicker.NoClip, picker.Next(segment < 0 ? ChordMoment.Outside : new ChordMoment(segment, 0, true, true)));
            }
        }

        [Test]
        public void Without_a_fitting_clip_the_same_clip_is_never_played_twice_in_a_row()
        {
            foreach (var rule in new[] { WhenNoClipFits.PlayClosestToChord, WhenNoClipFits.PlayNextClip })
            {
                var picker = Silent(ChordFixtures.Palette(), rule);
                var previous = picker.Next(OverC);
                for (var count = 0; count < 100; count++)
                {
                    var next = picker.Next(count % 3 == 0 ? ChordMoment.Outside : OverC);
                    Assert.AreNotEqual(previous, next);
                    previous = next;
                }
            }
        }

        [Test]
        public void A_chord_aware_chain_over_the_chart_never_repeats_a_clip_and_never_stays_silent()
        {
            var palette = ChordFixtures.Palette();
            var picker = new GuaranteedClipPicker(new ChordAwarePicker(new UnityRandomSource(), palette, 3f), palette, WhenNoClipFits.PlayClosestToChord);
            var previous = PianoPicker.NoClip;
            for (var count = 0; count < 300; count++)
            {
                var next = picker.Next(new ChordMoment(count % 8, count % 4, count % 2 == 0, false));
                Assert.AreNotEqual(PianoPicker.NoClip, next);
                Assert.AreNotEqual(previous, next);
                previous = next;
            }
        }
    }
}
