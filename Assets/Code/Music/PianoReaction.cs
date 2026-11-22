using System;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;

namespace DetectiveDave.MusicPlay
{
    public sealed class PianoReaction : IMusicReaction
    {
        readonly PianoPicker picker;
        readonly PianoNoteVolume volume;
        readonly ClickGate gate;
        readonly IPianoVoices voices;
        readonly Func<float> clock;

        public PianoReaction(PianoPicker picker, PianoNoteVolume volume, ClickGate gate, IPianoVoices voices, Func<float> clock)
        {
            this.picker = picker;
            this.volume = volume;
            this.gate = gate;
            this.voices = voices;
            this.clock = clock;
        }

        public void OnMoveAttempted(MoveAttempt attempt)
        {
            if (!gate.TryPass(clock())) return;
            var clipIndex = picker.Next();
            if (clipIndex == PianoPicker.NoClip) return;
            voices.Play(clipIndex, volume.For(attempt));
        }
    }
}
