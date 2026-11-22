using System;

namespace DetectiveDave.MusicPlay
{
    public sealed class SequentialPianoPicker
    {
        readonly int clipCount;
        int position;

        public SequentialPianoPicker(int clipCount) => this.clipCount = clipCount;

        public int Next() => Next(_ => true);

        public int Next(Func<int, bool> isAllowed)
        {
            for (var offset = 0; offset < clipCount; offset++)
            {
                var candidate = (position + offset) % clipCount;
                if (!isAllowed(candidate)) continue;
                position = (candidate + 1) % clipCount;
                return candidate;
            }
            return PianoPicker.NoClip;
        }
    }
}
