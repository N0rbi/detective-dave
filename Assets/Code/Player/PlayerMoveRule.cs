using DetectiveDave.KeyboardGrid;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public sealed class PlayerMoveRule
    {
        public MoveOutcome Decide(IKeyboardBoard board, IPlayerLevel level, Key goal, Key current, Key pressed)
        {
            if (current == pressed) return MoveOutcome.Ignored;
            if (!board.IsNeighbour(current, pressed)) return MoveOutcome.Slip;
            if (pressed == goal) return MoveOutcome.ReachGoal;
            return OutcomeOfEntering(level.KindOf(pressed));
        }

        static MoveOutcome OutcomeOfEntering(CellKind kind)
        {
            switch (kind)
            {
                case CellKind.Wall: return MoveOutcome.BumpWall;
                case CellKind.Gap: return MoveOutcome.FallIntoGap;
                case CellKind.Gutter: return MoveOutcome.HangOnGutter;
                default: return MoveOutcome.Step;
            }
        }
    }
}
