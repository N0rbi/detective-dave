namespace DetectiveDave.MusicPlay
{
    public sealed class SequentialPianoPicker : IPianoSequence
    {
        readonly int clipCount;
        int position;

        public SequentialPianoPicker(int clipCount) => this.clipCount = clipCount;

        public int Next()
        {
            if (clipCount == 0) return PianoPicker.NoClip;
            var pick = position;
            position = (position + 1) % clipCount;
            return pick;
        }
    }
}
