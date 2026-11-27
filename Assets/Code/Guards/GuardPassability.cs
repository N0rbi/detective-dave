using DetectiveDave.PlayerPlay;
using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    public sealed class GuardPassability
    {
        readonly IPlayerLevel level;

        public GuardPassability(IPlayerLevel level)
        {
            this.level = level;
        }

        public bool CanStandOn(Key key)
        {
            if (!level.HasLevel) return false;
            if (level.TryGetEnds(out var start, out _) && key == start) return false;
            return level.KindOf(key) == CellKind.Floor;
        }
    }
}
