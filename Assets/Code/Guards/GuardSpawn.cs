using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    public readonly struct GuardSpawn
    {
        public IReadOnlyList<Key> Route { get; }

        public GuardBrainSettings Settings { get; }

        public GuardSpawn(IReadOnlyList<Key> route, GuardBrainSettings settings)
        {
            Route = route;
            Settings = settings;
        }
    }
}
