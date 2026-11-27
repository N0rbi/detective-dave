using System.Collections.Generic;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class MisclickPickerTests
    {
        static readonly ChordSegment Am7 = new ChordSegment("Am7", 0f, 4f, new[] { 9, 0, 4, 7 }, new[] { 9, 11, 0, 2, 4, 7 });
        static readonly ChordSegment E7 = new ChordSegment("E7", 4f, 4f, new[] { 4, 8, 11, 2 }, new[] { 4, 5, 8, 9, 11, 0, 2 });

        static MisclickCandidate Chord(int index, params int[] notes) => new MisclickCandidate(NoteKind.Chord, index, $"chord {index}", PitchClassSet.Of(notes));

        static MisclickCandidate Single(int index, int note) => new MisclickCandidate(NoteKind.Single, index, $"single {index}", PitchClassSet.Of(new[] { note }));

        static MisclickPicker Build(bool normalize, params MisclickCandidate[] candidates) =>
            new MisclickPicker(new[] { Am7, E7 }, candidates, DissonanceWeights.Defaults, normalize);

        static ChordMoment In(int segment) => new ChordMoment(segment, 0, true, true);

        [Test]
        public void The_highest_scoring_clip_wins()
        {
            var picker = Build(false, Chord(0, 9, 0, 4), Chord(1, 2, 8, 11));
            var note = picker.Next(In(0));
            Assert.AreEqual(NoteKind.Chord, note.Kind);
            Assert.AreEqual(1, note.ClipIndex);
        }

        [Test]
        public void The_winner_depends_on_the_chord()
        {
            var picker = Build(false, Chord(0, 0, 4, 7), Chord(1, 2, 8, 11));
            Assert.AreEqual(1, picker.Next(In(0)).ClipIndex);
            Assert.AreEqual(0, picker.Next(In(1)).ClipIndex);
        }

        [Test]
        public void A_single_note_can_win()
        {
            var picker = Build(false, Chord(0, 9, 0, 4), Single(0, 8));
            var note = picker.Next(In(0));
            Assert.AreEqual(NoteKind.Single, note.Kind);
            Assert.AreEqual(0, note.ClipIndex);
        }

        [Test]
        public void A_chord_clip_beats_a_single_by_note_count_unless_normalised()
        {
            var candidates = new[] { Chord(0, 2, 8, 11), Single(0, 8) };
            Assert.AreEqual(NoteKind.Chord, Build(false, candidates).Next(In(0)).Kind);
            Assert.AreEqual(NoteKind.Single, Build(true, candidates).Next(In(0)).Kind);
        }

        [Test]
        public void Tied_clips_take_turns_in_order()
        {
            var picker = Build(false, Chord(0, 2, 8, 11), Chord(1, 0, 4), Chord(2, 2, 8, 11), Chord(3, 2, 8, 11));
            var picks = new[] { picker.Next(In(0)).ClipIndex, picker.Next(In(0)).ClipIndex, picker.Next(In(0)).ClipIndex, picker.Next(In(0)).ClipIndex };
            CollectionAssert.AreEqual(new[] { 0, 2, 3, 0 }, picks);
        }

        [Test]
        public void A_tie_never_repeats_the_previous_clip_while_another_is_tied()
        {
            var picker = Build(false, Chord(0, 2, 8, 11), Chord(1, 2, 8, 11));
            var previous = picker.Next(In(0)).ClipIndex;
            for (var i = 0; i < 6; i++)
            {
                var next = picker.Next(In(0)).ClipIndex;
                Assert.AreNotEqual(previous, next);
                previous = next;
            }
        }

        [Test]
        public void A_loop_position_outside_every_chord_is_silent() =>
            Assert.IsTrue(Build(false, Chord(0, 2, 8, 11)).Next(ChordMoment.Outside).IsSilent);

        [Test]
        public void Clips_without_notes_are_skipped()
        {
            var picker = Build(false, new MisclickCandidate(NoteKind.Chord, 0, "unmapped", default), Chord(1, 0, 4));
            Assert.AreEqual(1, picker.Next(In(0)).ClipIndex);
        }

        [Test]
        public void No_candidates_is_silent() => Assert.IsTrue(Build(false).Next(In(0)).IsSilent);

        [Test]
        public void Most_dissonant_reports_the_score()
        {
            var picker = Build(false, Chord(0, 2, 8, 11));
            picker.MostDissonant(Am7, out var score);
            Assert.AreEqual(1.2f, score, 0.0001f);
        }
    }

    public class PianoReactionMisclickTests
    {
        sealed class FixedRandom : IRandomSource
        {
            public float Range(float min, float max) => (min + max) / 2f;

            public int Index(int count) => 0;
        }

        sealed class ScriptedPicker : IPianoPicker
        {
            public int Calls;

            public int Next(ChordMoment moment)
            {
                Calls++;
                return 3;
            }
        }

        sealed class StubMisclick : IMisclickPicker
        {
            public PickedNote Result = new PickedNote(NoteKind.Single, 7);

            public PickedNote Next(ChordMoment moment) => Result;
        }

        sealed class Recorder : IPianoVoices
        {
            public readonly List<(NoteKind Kind, int Clip, float Volume)> Played = new List<(NoteKind, int, float)>();

            public void Play(NoteKind kind, int clipIndex, float volume) => Played.Add((kind, clipIndex, volume));
        }

        readonly Recorder voices = new Recorder();
        readonly List<string> warnings = new List<string>();
        readonly ScriptedPicker chordPicker = new ScriptedPicker();

        PianoReaction Build(IMisclickPicker misclick, float debounceSeconds = 0f) =>
            new PianoReaction(chordPicker, new ScriptedPicker(), new DebounceGate(debounceSeconds, true), new PianoNoteVolume(new FixedRandom(), 0.8f, 0.2f, 0.5f, 0.25f), new ClickGate(0f), voices, () => 0f, () => new ChordMoment(0, 0, true, true), warnings.Add, misclick);

        [Test]
        public void A_wrong_key_plays_the_misclick_note_at_the_misclick_share()
        {
            Build(new StubMisclick()).OnMoveAttempted(MoveAttempt.WrongKey);
            Assert.AreEqual((NoteKind.Single, 7, 0.2f), voices.Played[0]);
            Assert.AreEqual(0, chordPicker.Calls);
        }

        [Test]
        public void Other_attempts_do_not_use_the_misclick_picker()
        {
            Build(new StubMisclick()).OnMoveAttempted(MoveAttempt.Blocked);
            Build(new StubMisclick()).OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(2, chordPicker.Calls);
            Assert.AreEqual(0.4f, voices.Played[0].Volume, 0.0001f);
            Assert.AreEqual(0.8f, voices.Played[1].Volume, 0.0001f);
        }

        [Test]
        public void A_wrong_key_without_a_dissonant_clip_plays_a_normal_note_and_warns_once()
        {
            var reaction = Build(new StubMisclick { Result = new PickedNote(NoteKind.Chord, PianoPicker.NoClip) });
            reaction.OnMoveAttempted(MoveAttempt.WrongKey);
            reaction.OnMoveAttempted(MoveAttempt.WrongKey);
            Assert.AreEqual(2, voices.Played.Count);
            Assert.AreEqual(3, voices.Played[0].Clip);
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void With_the_setting_off_a_wrong_key_plays_a_normal_note_without_a_warning()
        {
            Build(null).OnMoveAttempted(MoveAttempt.WrongKey);
            Assert.AreEqual(3, voices.Played[0].Clip);
            Assert.AreEqual(0, warnings.Count);
        }

        [Test]
        public void A_misclick_does_not_start_the_debounce_or_move_the_sequence()
        {
            var reaction = Build(new StubMisclick(), 5f);
            reaction.OnMoveAttempted(MoveAttempt.WrongKey);
            reaction.OnMoveAttempted(MoveAttempt.Moved);
            Assert.AreEqual(NoteKind.Chord, voices.Played[1].Kind);
            Assert.AreEqual(1, chordPicker.Calls);
        }
    }
}
