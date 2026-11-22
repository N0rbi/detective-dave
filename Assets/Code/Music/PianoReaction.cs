using System;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;

namespace DetectiveDave.MusicPlay
{
    public sealed class PianoReaction : IMusicReaction
    {
        readonly IPianoPicker picker;
        readonly PianoNoteVolume volume;
        readonly ClickGate gate;
        readonly IPianoVoices voices;
        readonly Func<float> clock;
        readonly Func<ChordMoment> momentNow;

        public PianoReaction(IPianoPicker picker, PianoNoteVolume volume, ClickGate gate, IPianoVoices voices, Func<float> clock, Func<ChordMoment> momentNow)
        {
            this.picker = picker;
            this.volume = volume;
            this.gate = gate;
            this.voices = voices;
            this.clock = clock;
            this.momentNow = momentNow;
        }

        public void OnMoveAttempted(MoveAttempt attempt)
        {
            if (!gate.TryPass(clock())) return;
            var clipIndex = picker.Next(momentNow());
            if (clipIndex == PianoPicker.NoClip) return;
            voices.Play(clipIndex, volume.For(attempt));
        }
    }
}
