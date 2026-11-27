using System.Collections.Generic;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PianoDebounceReactionTests
    {
        const float Window = 0.6f;
        const float BaseVolume = 0.8f;
        const float BlockedShare = 0.5f;

        sealed class FixedRandom : IRandomSource
        {
            public float Range(float min, float max) => (min + max) / 2f;

            public int Index(int count) => 0;
        }

        sealed class ScriptedPicker : IPianoPicker
        {
            readonly Queue<int> clips;

            public ScriptedPicker(params int[] clips) => this.clips = new Queue<int>(clips);

            public int Calls { get; private set; }

            public ChordMoment LastMoment { get; private set; }

            public int Next(ChordMoment moment)
            {
                Calls++;
                LastMoment = moment;
                return clips.Count > 0 ? clips.Dequeue() : PianoPicker.NoClip;
            }
        }

        sealed class RecordingVoices : IPianoVoices
        {
            public readonly List<(NoteKind Kind, int Clip, float Volume)> Played = new List<(NoteKind, int, float)>();

            public void Play(NoteKind kind, int clipIndex, float volume) => Played.Add((kind, clipIndex, volume));
        }

        RecordingVoices voices;
        List<string> warnings;
        ChordMoment moment;
        float now;

        [SetUp]
        public void SetUp()
        {
            voices = new RecordingVoices();
            warnings = new List<string>();
            moment = new ChordMoment(3, 2, true, false);
            now = 0f;
        }

        PianoReaction Build(IPianoPicker chords, IPianoPicker singles, bool singlesExtend = true, float minSecondsBetween = 0f) =>
            new PianoReaction(chords, singles, new DebounceGate(Window, singlesExtend), new PianoNoteVolume(new FixedRandom(), BaseVolume, 0.2f, BlockedShare), new ClickGate(minSecondsBetween), voices, () => now, () => moment, warnings.Add);

        NoteKind[] Kinds() => voices.Played.ConvertAll(note => note.Kind).ToArray();

        [Test]
        public void The_first_move_plays_a_chord_clip()
        {
            Build(new ScriptedPicker(4), new ScriptedPicker(1)).OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual((NoteKind.Chord, 4), (voices.Played[0].Kind, voices.Played[0].Clip));
        }

        [Test]
        public void A_move_right_after_plays_a_single_note()
        {
            var reaction = Build(new ScriptedPicker(4), new ScriptedPicker(7));
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.2f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual((NoteKind.Single, 7), (voices.Played[1].Kind, voices.Played[1].Clip));
        }

        [Test]
        public void A_move_after_a_pause_plays_a_chord_clip_again()
        {
            var reaction = Build(new ScriptedPicker(4, 5), new ScriptedPicker(7));
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.2f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 1.5f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            CollectionAssert.AreEqual(new[] { NoteKind.Chord, NoteKind.Single, NoteKind.Chord }, Kinds());
            Assert.AreEqual(5, voices.Played[2].Clip);
        }

        [Test]
        public void A_continuous_run_of_moves_stays_single_notes()
        {
            var reaction = Build(new ScriptedPicker(4), new ScriptedPicker(1, 2, 3, 4));
            foreach (var time in new[] { 0f, 0.4f, 0.8f, 1.2f, 1.6f })
            {
                now = time;
                reaction.OnMoveAttempted(MoveAttempt.Moved);
            }
            CollectionAssert.AreEqual(new[] { NoteKind.Chord, NoteKind.Single, NoteKind.Single, NoteKind.Single, NoteKind.Single }, Kinds());
        }

        [Test]
        public void A_blocked_move_counts_for_the_window_and_plays_a_quiet_single_note()
        {
            var reaction = Build(new ScriptedPicker(4), new ScriptedPicker(7));
            reaction.OnMoveAttempted(MoveAttempt.Blocked);
            now = 0.3f;
            reaction.OnMoveAttempted(MoveAttempt.Blocked);
            Assert.AreEqual(NoteKind.Single, voices.Played[1].Kind);
            Assert.AreEqual(BaseVolume * BlockedShare, voices.Played[1].Volume, 0.0001f);
        }

        [Test]
        public void A_blocked_first_move_plays_a_quiet_chord_clip()
        {
            Build(new ScriptedPicker(4), new ScriptedPicker(7)).OnMoveAttempted(MoveAttempt.Blocked);
            Assert.AreEqual(BaseVolume * BlockedShare, voices.Played[0].Volume, 0.0001f);
        }

        [Test]
        public void A_single_note_gets_the_chord_moment_of_the_press()
        {
            var singles = new ScriptedPicker(7);
            var reaction = Build(new ScriptedPicker(4), singles);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.2f;
            moment = new ChordMoment(6, 1, false, false);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(6, singles.LastMoment.SegmentIndex);
        }

        [Test]
        public void Without_single_note_clips_a_chord_clip_plays_instead()
        {
            var reaction = Build(new ScriptedPicker(4, 5), new ScriptedPicker());
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.2f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            CollectionAssert.AreEqual(new[] { NoteKind.Chord, NoteKind.Chord }, Kinds());
            Assert.AreEqual(5, voices.Played[1].Clip);
        }

        [Test]
        public void The_missing_single_notes_are_warned_about_once()
        {
            var reaction = Build(new ScriptedPicker(4, 5, 6, 7), new ScriptedPicker());
            foreach (var time in new[] { 0f, 0.1f, 0.2f, 0.3f })
            {
                now = time;
                reaction.OnMoveAttempted(MoveAttempt.Moved);
            }
            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains("Single Note Clips", warnings[0]);
        }

        [Test]
        public void No_warning_is_given_while_every_note_can_be_played_as_wanted()
        {
            var reaction = Build(new ScriptedPicker(4), new ScriptedPicker(1, 2));
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.1f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.IsEmpty(warnings);
        }

        [Test]
        public void A_move_skipped_by_the_minimum_time_does_not_restart_the_window()
        {
            var reaction = Build(new ScriptedPicker(4, 5), new ScriptedPicker(7), true, 0.5f);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.3f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.65f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            CollectionAssert.AreEqual(new[] { NoteKind.Chord, NoteKind.Chord }, Kinds());
        }

        [Test]
        public void A_silent_pick_does_not_restart_the_window()
        {
            var reaction = Build(new ScriptedPicker(PianoPicker.NoClip, 5), new ScriptedPicker(7));
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            now = 0.2f;
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(NoteKind.Chord, voices.Played[0].Kind);
            Assert.AreEqual(1, voices.Played.Count);
        }

        [Test]
        public void With_singles_that_do_not_extend_the_window_a_chord_returns_after_the_window_while_moving()
        {
            var reaction = Build(new ScriptedPicker(4, 5), new ScriptedPicker(1, 2, 3), false);
            foreach (var time in new[] { 0f, 0.3f, 0.5f, 0.7f })
            {
                now = time;
                reaction.OnMoveAttempted(MoveAttempt.Moved);
            }
            CollectionAssert.AreEqual(new[] { NoteKind.Chord, NoteKind.Single, NoteKind.Single, NoteKind.Chord }, Kinds());
        }

        [Test]
        public void The_warning_picker_names_the_kind_of_clip()
        {
            var palette = new ChordPalette(ChordFixtures.Segments(), new List<PitchClassSet>());
            var warned = new List<string>();
            var picker = new WarnOnSilentChordPicker(new ScriptedPicker(), palette, warned.Add, "single note clip");
            picker.Next(new ChordMoment(0, 0, true, true));
            StringAssert.Contains("No single note clip fits", warned[0]);
        }

        [Test]
        public void The_warning_picker_says_piano_clip_by_default()
        {
            var palette = new ChordPalette(ChordFixtures.Segments(), new List<PitchClassSet>());
            var warned = new List<string>();
            new WarnOnSilentChordPicker(new ScriptedPicker(), palette, warned.Add).Next(new ChordMoment(0, 0, true, true));
            StringAssert.Contains("No piano clip fits", warned[0]);
        }
    }
}
