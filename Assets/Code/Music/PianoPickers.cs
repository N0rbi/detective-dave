using DetectiveDave.OverworldPlay;

namespace DetectiveDave.MusicPlay
{
    public static class PianoPickers
    {
        public static IPianoPicker For(PianoOrder order, IRandomSource random, ChordPalette palette, float chordToneWeight)
        {
            switch (order)
            {
                case PianoOrder.InOrder: return new InOrderChordPicker(new SequentialPianoPicker(palette.ClipCount), palette);
                case PianoOrder.Random: return new RandomChordPicker(new PianoPicker(random, palette.ClipCount), palette);
                default: return new ChordAwarePicker(random, palette, chordToneWeight);
            }
        }
    }
}
