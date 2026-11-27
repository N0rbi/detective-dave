using System;
using DetectiveDave.PlayerPlay;

namespace DetectiveDave.MusicPlay
{
    public sealed class HookGatedReaction : IMusicReaction
    {
        readonly IMusicReaction inner;
        readonly Func<bool> allowed;

        public HookGatedReaction(IMusicReaction inner, Func<bool> allowed)
        {
            this.inner = inner;
            this.allowed = allowed;
        }

        public void OnMoveAttempted(MoveAttempt attempt)
        {
            if (allowed()) inner.OnMoveAttempted(attempt);
        }
    }
}
