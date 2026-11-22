using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DebounceGateTests
    {
        const float Window = 0.6f;

        static DebounceGate Gate(bool singlesExtend = true, float window = Window) => new DebounceGate(window, singlesExtend);

        static NoteKind Play(DebounceGate gate, float now)
        {
            var kind = gate.KindAt(now);
            gate.NotePlayed(kind, now);
            return kind;
        }

        [Test]
        public void The_first_note_is_a_chord()
        {
            Assert.AreEqual(NoteKind.Chord, Gate().KindAt(0f));
        }

        [Test]
        public void A_note_inside_the_window_is_a_single_note()
        {
            var gate = Gate();
            Play(gate, 0f);
            Assert.AreEqual(NoteKind.Single, gate.KindAt(0.3f));
        }

        [Test]
        public void A_note_after_the_window_is_a_chord_again()
        {
            var gate = Gate();
            Play(gate, 0f);
            Assert.AreEqual(NoteKind.Chord, gate.KindAt(0.9f));
        }

        [Test]
        public void A_note_exactly_at_the_end_of_the_window_is_a_chord()
        {
            var gate = Gate();
            Play(gate, 1f);
            Assert.AreEqual(NoteKind.Chord, gate.KindAt(1f + Window));
        }

        [Test]
        public void A_note_just_before_the_end_of_the_window_is_a_single_note()
        {
            var gate = Gate();
            Play(gate, 1f);
            Assert.AreEqual(NoteKind.Single, gate.KindAt(1f + Window - 0.01f));
        }

        [Test]
        public void Asking_does_not_start_the_window()
        {
            var gate = Gate();
            gate.KindAt(0f);
            Assert.AreEqual(NoteKind.Chord, gate.KindAt(0.1f));
        }

        [Test]
        public void Every_single_note_restarts_the_window_when_singles_extend_it()
        {
            var gate = Gate();
            Play(gate, 0f);
            Assert.AreEqual(NoteKind.Single, Play(gate, 0.4f));
            Assert.AreEqual(NoteKind.Single, Play(gate, 0.9f));
            Assert.AreEqual(NoteKind.Chord, Play(gate, 1.6f));
        }

        [Test]
        public void The_window_counts_from_the_last_chord_when_singles_do_not_extend_it()
        {
            var gate = Gate(false);
            Play(gate, 0f);
            Assert.AreEqual(NoteKind.Single, Play(gate, 0.4f));
            Assert.AreEqual(NoteKind.Chord, Play(gate, 0.7f));
        }

        [Test]
        public void A_window_of_zero_makes_every_note_a_chord()
        {
            var gate = Gate(true, 0f);
            Assert.AreEqual(NoteKind.Chord, Play(gate, 0f));
            Assert.AreEqual(NoteKind.Chord, Play(gate, 0f));
            Assert.AreEqual(NoteKind.Chord, Play(gate, 0.1f));
        }

        [Test]
        public void A_negative_window_acts_like_zero()
        {
            var gate = Gate(true, -1f);
            Play(gate, 0f);
            Assert.AreEqual(NoteKind.Chord, gate.KindAt(0f));
        }
    }
}
