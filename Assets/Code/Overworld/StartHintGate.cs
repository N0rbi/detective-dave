using DetectiveDave.KeyboardGrid;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class StartHintGate
    {
        readonly IKeyboardBoard board;
        Key startKey;
        bool armed;

        public StartHintGate(IKeyboardBoard board) => this.board = board;

        public void Arm(Key start)
        {
            startKey = start;
            armed = true;
        }

        public void Disarm() => armed = false;

        public bool IsWrongStartKey(Key pressed) => armed && pressed != startKey && !board.IsNeighbour(startKey, pressed);
    }
}
