using System;
using System.Collections.Generic;

namespace DetectiveDave.MusicPlay
{
    public sealed class WarnOnSilentChordPicker : IPianoPicker
    {
        readonly IPianoPicker inner;
        readonly ChordPalette palette;
        readonly Action<string> warn;
        readonly HashSet<int> warned = new HashSet<int>();

        public WarnOnSilentChordPicker(IPianoPicker inner, ChordPalette palette, Action<string> warn)
        {
            this.inner = inner;
            this.palette = palette;
            this.warn = warn;
        }

        public int Next(ChordMoment moment)
        {
            var clip = inner.Next(moment);
            if (clip == PianoPicker.NoClip && warned.Add(moment.SegmentIndex)) warn(MessageFor(moment));
            return clip;
        }

        string MessageFor(ChordMoment moment) => moment.IsInsideChart
            ? $"No piano clip fits the scale of {palette.ChordName(moment.SegmentIndex)} (chord number {moment.SegmentIndex + 1}), so When No Clip Fits chooses the clip played over it. Add a clip whose notes are in its scale, or widen the scale, on the Chord Chart."
            : "The loop position is outside every chord of the Chord Chart, so the next clip in name order plays there. Fix the chord segments on the Chord Chart.";
    }
}
