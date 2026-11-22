namespace DetectiveDave.MusicPlay
{
    public sealed class RandomChordPicker : IPianoPicker
    {
        readonly PianoPicker picker;
        readonly ChordPalette palette;

        public RandomChordPicker(PianoPicker picker, ChordPalette palette)
        {
            this.picker = picker;
            this.palette = palette;
        }

        public int Next(ChordMoment moment) => picker.Next(palette.Allowed(moment.SegmentIndex));
    }
}
