using System;
using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    public sealed class GoalSequence
    {
        readonly IKeyboardBoard board;
        readonly IPlayerLevel level;
        readonly EndDecorationsDirector decorations;
        readonly IEvidencePresenter evidence;
        readonly ICameraZoom zoom;
        readonly ZoomFocus focus;
        readonly IScreenWipe wipe;
        readonly ILevelAdvance advance;
        readonly IntroSequence intro;
        readonly float handRiseInKeys;
        int generation;
        Vector2 goalWorld;

        public GoalSequence(IKeyboardBoard board, IPlayerLevel level, EndDecorationsDirector decorations, IEvidencePresenter evidence, ICameraZoom zoom, ZoomFocus focus, IScreenWipe wipe, ILevelAdvance advance, IntroSequence intro, float handRiseInKeys)
        {
            this.board = board;
            this.level = level;
            this.decorations = decorations;
            this.evidence = evidence;
            this.zoom = zoom;
            this.focus = focus;
            this.wipe = wipe;
            this.advance = advance;
            this.intro = intro;
            this.handRiseInKeys = handRiseInKeys;
        }

        public bool IsRunning { get; private set; }

        public bool IsAdvancing { get; private set; }

        public void Begin()
        {
            if (IsRunning || !TryFindGoal(out goalWorld)) return;
            IsRunning = true;
            decorations.ShowBurstOnGoal();
            evidence.Present(HandAbove(goalWorld), Guard(ZoomOnPlayer));
        }

        public void HandleLevelChanged()
        {
            if (!IsAdvancing) Cancel();
        }

        public void Cancel()
        {
            generation++;
            IsRunning = false;
            evidence.Hide();
            intro.Cancel();
        }

        void ZoomOnPlayer()
        {
            evidence.Emphasise();
            zoom.ZoomIn(focus.On(goalWorld), Guard(WipeOut));
        }

        void WipeOut() => wipe.Out(goalWorld, Guard(AdvanceLevel));

        void AdvanceLevel()
        {
            evidence.Hide();
            LoadNextLevel();
            intro.Begin(Guard(Finish));
        }

        void LoadNextLevel()
        {
            IsAdvancing = true;
            try { advance.Advance(); }
            finally { IsAdvancing = false; }
        }

        void Finish() => IsRunning = false;

        Vector2 HandAbove(Vector2 goal) => goal + Vector2.up * (handRiseInKeys * board.KeyWorldSize);

        bool TryFindGoal(out Vector2 goal)
        {
            goal = default;
            return level.TryGetEnds(out _, out Key goalKey) && board.TryGetPosition(goalKey, out goal);
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
