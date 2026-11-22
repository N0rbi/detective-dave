using DetectiveDave.OverworldPlay;

namespace DetectiveDave.MusicPlay
{
    public static class PianoSequences
    {
        public static IPianoSequence For(PianoOrder order, IRandomSource random, int clipCount) =>
            order == PianoOrder.InOrder ? (IPianoSequence)new SequentialPianoPicker(clipCount) : new PianoPicker(random, clipCount);
    }
}
