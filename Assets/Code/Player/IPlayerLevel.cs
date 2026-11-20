using System;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public interface IPlayerLevel
    {
        event Action LevelChanged;

        bool HasLevel { get; }

        bool TryGetEnds(out Key start, out Key goal);

        CellKind KindOf(Key key);
    }
}
