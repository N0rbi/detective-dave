using System;

namespace DetectiveDave.PlayerPlay
{
    public sealed class WalkCycle
    {
        readonly int frameCount;
        int steps;

        public WalkCycle(int frameCount)
        {
            if (frameCount < 2) throw new ArgumentOutOfRangeException(nameof(frameCount));
            this.frameCount = frameCount;
        }

        public int Frame => FrameAfter(steps);

        public void Advance() => steps++;

        public void Reset() => steps = 0;

        int FrameAfter(int stepCount)
        {
            int period = 2 * (frameCount - 1);
            int position = stepCount % period;
            return position < frameCount ? position : period - position;
        }
    }
}
