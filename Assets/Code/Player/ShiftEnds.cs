using DetectiveDave.Levels;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public static class ShiftEnds
    {
        public static void Of(StartSide side, out Key start, out Key goal)
        {
            start = side == StartSide.Left ? Key.LeftShift : Key.RightShift;
            goal = side == StartSide.Left ? Key.RightShift : Key.LeftShift;
        }
    }
}
