using DetectiveDave.OverworldPlay;

namespace DetectiveDave.MusicPlay
{
    public sealed class PianoPicker : IPianoSequence
    {
        public const int NoClip = -1;

        readonly IRandomSource random;
        readonly int clipCount;
        int last = NoClip;

        public PianoPicker(IRandomSource random, int clipCount)
        {
            this.random = random;
            this.clipCount = clipCount;
        }

        public int Next()
        {
            if (clipCount == 0) return NoClip;
            if (clipCount == 1) return last = 0;
            if (last == NoClip) return last = random.Index(clipCount);
            var pick = random.Index(clipCount - 1);
            if (pick >= last) pick++;
            return last = pick;
        }
    }
}
