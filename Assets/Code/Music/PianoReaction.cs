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
        readonly INoteTiming timing;

        public PianoReaction(IPianoPicker picker, PianoNoteVolume volume, ClickGate gate, IPianoVoices voices, Func<float> clock, INoteTiming timing)
        {
            this.picker = picker;
            this.volume = volume;
            this.gate = gate;
            this.voices = voices;
            this.clock = clock;
            this.timing = timing;
        }

        public void OnMoveAttempted(MoveAttempt attempt)
        {
            if (!gate.TryPass(clock())) return;
            if (!timing.TryPlan(out var delaySeconds, out var moment)) return;
            var clipIndex = picker.Next(moment);
            if (clipIndex == PianoPicker.NoClip) return;
            voices.Play(clipIndex, volume.For(attempt), delaySeconds);
        }
    }
}
