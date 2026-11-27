using System;
using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public sealed class IntroSequence
    {
        const int ParallelParts = 2;

        readonly IKeyboardBoard board;
        readonly IPlayerLevel level;
        readonly IScreenWipe wipe;
        readonly ICameraZoom zoom;
        readonly ZoomFocus focus;
        int generation;
        int pendingParts;

        public IntroSequence(IKeyboardBoard board, IPlayerLevel level, IScreenWipe wipe, ICameraZoom zoom, ZoomFocus focus)
        {
            this.board = board;
            this.level = level;
            this.wipe = wipe;
            this.zoom = zoom;
            this.focus = focus;
        }

        public bool IsRunning { get; private set; }

        public void Begin(Action done = null)
        {
            generation++;
            if (!TryFindStart(out var start))
            {
                ClearScreenEffects();
                done?.Invoke();
                return;
            }
            IsRunning = true;
            pendingParts = ParallelParts;
            zoom.SnapZoomed(focus.On(start));
            wipe.In(start, Guard(() => FinishPart(done)));
            zoom.ZoomOut(Guard(() => FinishPart(done)));
        }

        public void Cancel()
        {
            generation++;
            ClearScreenEffects();
        }

        void ClearScreenEffects()
        {
            IsRunning = false;
            wipe.Cancel();
            zoom.Restore();
        }

        void FinishPart(Action done)
        {
            pendingParts--;
            if (pendingParts > 0) return;
            IsRunning = false;
            done?.Invoke();
        }

        bool TryFindStart(out Vector2 start)
        {
            start = default;
            return level.TryGetEnds(out Key startKey, out _) && board.TryGetPosition(startKey, out start);
        }

        Action Guard(Action action)
        {
            int token = generation;
            return () =>
            {
                if (token == generation) action();
            };
        }
    }
}
