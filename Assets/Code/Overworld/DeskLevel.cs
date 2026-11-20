using System;
using DetectiveDave.PlayerPlay;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DeskLevel : IPlayerLevel
    {
        public event Action LevelChanged;

        public bool HasLevel => true;

        public bool TryGetEnds(out Key start, out Key goal)
        {
            start = Key.LeftShift;
            goal = Key.None;
            return true;
        }

        public CellKind KindOf(Key key) => CellKind.Floor;

        public void Refresh() => LevelChanged?.Invoke();
    }
}
