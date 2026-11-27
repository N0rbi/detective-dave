using DetectiveDave.KeyboardGrid;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class ZoomFocus
    {
        readonly IKeyboardBoard board;
        readonly float handRiseInKeys;
        readonly float handShare;

        public ZoomFocus(IKeyboardBoard board, float handRiseInKeys, float handShare)
        {
            this.board = board;
            this.handRiseInKeys = handRiseInKeys;
            this.handShare = handShare;
        }

        public Vector2 On(Vector2 player) => player + Vector2.up * (handRiseInKeys * board.KeyWorldSize * handShare);
    }
}
