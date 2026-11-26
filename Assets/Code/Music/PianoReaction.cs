using System;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;

namespace DetectiveDave.MusicPlay
{
    public sealed class PianoReaction : IMusicReaction
    {
        const string NoSinglesWarning = "Music Settings has no Single Note Clips, so a move right after another move plays a chord clip. Export the single piano notes as \"Piano Single - <Note><Octave>.wav\" into Assets/Staging and run Window > Detective Dave > Set Up Music.";

        const string NoMisclickWarning = "A wrong key could not play a dissonant note, because there is no Chord Chart with chords and clip notes (or the loop is outside every chord). It plays a normal piano note instead. Run Window > Detective Dave > Set Up Chord Chart.";

        readonly IPianoPicker chordPicker;
        readonly IPianoPicker singlePicker;
        readonly DebounceGate debounce;
        readonly PianoNoteVolume volume;
        readonly ClickGate gate;
        readonly IPianoVoices voices;
        readonly Func<float> clock;
        readonly Func<ChordMoment> momentNow;
        readonly Action<string> warn;
        readonly IMisclickPicker misclick;
        bool warnedAboutSingles;
        bool warnedAboutMisclick;

        public PianoReaction(IPianoPicker chordPicker, IPianoPicker singlePicker, DebounceGate debounce, PianoNoteVolume volume, ClickGate gate, IPianoVoices voices, Func<float> clock, Func<ChordMoment> momentNow, Action<string> warn) : this(chordPicker, singlePicker, debounce, volume, gate, voices, clock, momentNow, warn, null)
        {
        }

        public PianoReaction(IPianoPicker chordPicker, IPianoPicker singlePicker, DebounceGate debounce, PianoNoteVolume volume, ClickGate gate, IPianoVoices voices, Func<float> clock, Func<ChordMoment> momentNow, Action<string> warn, IMisclickPicker misclick)
        {
            this.misclick = misclick;
            this.chordPicker = chordPicker;
            this.singlePicker = singlePicker;
            this.debounce = debounce;
            this.volume = volume;
            this.gate = gate;
            this.voices = voices;
            this.clock = clock;
            this.momentNow = momentNow;
            this.warn = warn;
        }

        public void OnMoveAttempted(MoveAttempt attempt)
        {
            var now = clock();
            if (!gate.TryPass(now)) return;
            if (attempt == MoveAttempt.WrongKey && misclick != null && PlayMisclick(attempt)) return;
            var note = Pick(debounce.KindAt(now), momentNow());
            if (note.IsSilent) return;
            voices.Play(note.Kind, note.ClipIndex, volume.For(attempt));
            debounce.NotePlayed(note.Kind, now);
        }

        bool PlayMisclick(MoveAttempt attempt)
        {
            var note = misclick.Next(momentNow());
            if (note.IsSilent)
            {
                WarnAboutMisclick();
                return false;
            }
            voices.Play(note.Kind, note.ClipIndex, volume.For(attempt));
            return true;
        }

        void WarnAboutMisclick()
        {
            if (warnedAboutMisclick) return;
            warnedAboutMisclick = true;
            warn(NoMisclickWarning);
        }

        PickedNote Pick(NoteKind wanted, ChordMoment moment)
        {
            if (wanted == NoteKind.Chord) return new PickedNote(NoteKind.Chord, chordPicker.Next(moment));
            var single = singlePicker.Next(moment);
            if (single != PianoPicker.NoClip) return new PickedNote(NoteKind.Single, single);
            WarnAboutMissingSingles();
            return new PickedNote(NoteKind.Chord, chordPicker.Next(moment));
        }

        void WarnAboutMissingSingles()
        {
            if (warnedAboutSingles) return;
            warnedAboutSingles = true;
            warn(NoSinglesWarning);
        }
    }
}
