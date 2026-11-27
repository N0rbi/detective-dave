using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public sealed class EndDecorationsDirector
    {
        readonly IKeyboardBoard board;
        readonly IPlayerLevel level;
        readonly IEndDecorations decorations;
        readonly float burstRiseInKeys;

        public EndDecorationsDirector(IKeyboardBoard board, IPlayerLevel level, IEndDecorations decorations, float burstRiseInKeys)
        {
            this.board = board;
            this.level = level;
            this.decorations = decorations;
            this.burstRiseInKeys = burstRiseInKeys;
        }

        public void Refresh(bool playerIsActive)
        {
            decorations.HideAll();
            if (!playerIsActive || !TryFindEndPositions(out var start, out var goal)) return;
            decorations.ShowStart(start);
            decorations.ShowGoal(goal);
        }

        public void ShowBurstOnGoal()
        {
            if (!TryFindEndPositions(out _, out var goal)) return;
            decorations.ShowBurst(goal + Vector2.up * (burstRiseInKeys * board.KeyWorldSize));
        }

        public void Clear() => decorations.HideAll();

        bool TryFindEndPositions(out Vector2 start, out Vector2 goal)
        {
            start = default;
            goal = default;
            return level.TryGetEnds(out Key startKey, out Key goalKey) && board.TryGetPosition(startKey, out start) && board.TryGetPosition(goalKey, out goal);
        }
    }
}
