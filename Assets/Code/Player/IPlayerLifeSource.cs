using System;

namespace DetectiveDave.PlayerPlay
{
    public interface IPlayerLifeSource
    {
        event Action Died;

        event Action Respawned;
    }
}
