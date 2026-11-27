using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class CompletionLatch
    {
        readonly Action allDone;
        int remaining;

        public CompletionLatch(int expected, Action allDone)
        {
            if (expected < 1) throw new ArgumentOutOfRangeException(nameof(expected), expected, "A latch waits for at least one completion");
            remaining = expected;
            this.allDone = allDone;
        }

        public bool IsOpen => remaining == 0;

        public void Complete()
        {
            if (remaining == 0) return;
            remaining--;
            if (remaining == 0) allDone();
        }
    }
}
