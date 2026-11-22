using System;

namespace DetectiveDave.PlayerPlay
{
    public interface IMoveAttemptSource
    {
        event Action<MoveAttempt> MoveAttempted;
    }
}
