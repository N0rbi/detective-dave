using System;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class RestartSequence
    {
        readonly IScreenWipe wipe;
        readonly IntroSequence intro;
        readonly Action resetWorld;
        int generation;

        public RestartSequence(IScreenWipe wipe, IntroSequence intro, Action resetWorld)
        {
            this.wipe = wipe;
            this.intro = intro;
            this.resetWorld = resetWorld;
        }

        public bool IsRunning { get; private set; }

        public void Begin(Vector2 centre)
        {
            if (IsRunning) return;
            IsRunning = true;
            wipe.Out(centre, Guard(ResetWorldAndWipeIn));
        }

        public void Cancel()
        {
            if (!IsRunning) return;
            generation++;
            IsRunning = false;
            intro.Cancel();
        }

        void ResetWorldAndWipeIn()
        {
            resetWorld();
            intro.Begin(Guard(Finish));
        }

        void Finish() => IsRunning = false;

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
