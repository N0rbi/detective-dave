using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public sealed class InOrderChordPicker : IPianoPicker
    {
        readonly SequentialPianoPicker sequence;
        readonly ChordPalette palette;

        public InOrderChordPicker(SequentialPianoPicker sequence, ChordPalette palette)
        {
            this.sequence = sequence;
            this.palette = palette;
        }

        public int Next(ChordMoment moment)
        {
            var allowed = palette.Allowed(moment.SegmentIndex);
            return sequence.Next(clip => allowed.Contains(clip));
        }
    }
}
