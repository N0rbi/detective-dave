namespace DetectiveDave.MusicPlay
{
    public sealed class GuaranteedClipPicker : IPianoPicker
    {
        readonly IPianoPicker inner;
        readonly ChordPalette palette;
        readonly WhenNoClipFits rule;
        readonly SequentialPianoPicker sequence;
        int last = PianoPicker.NoClip;

        public GuaranteedClipPicker(IPianoPicker inner, ChordPalette palette, WhenNoClipFits rule)
        {
            this.inner = inner;
            this.palette = palette;
            this.rule = rule;
            sequence = new SequentialPianoPicker(palette.ClipCount);
        }

        public int Next(ChordMoment moment)
        {
            var clip = inner.Next(moment);
            if (clip == PianoPicker.NoClip) clip = Fallback(moment);
            if (clip != PianoPicker.NoClip) last = clip;
            return clip;
        }

        int Fallback(ChordMoment moment) =>
            rule == WhenNoClipFits.PlayClosestToChord && moment.IsInsideChart ? palette.Closest(moment.SegmentIndex, last) : NextInOrder();

        int NextInOrder() => sequence.Next(clip => clip != last || palette.ClipCount == 1);
    }
}
