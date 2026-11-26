using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    public sealed class DeathHookController
    {
        readonly DeathHookSettings settings;
        readonly IDeathHookOutput output;
        readonly Func<float> clock;
        float amount;
        float delayLeft;
        float lastTime;
        bool hookStarted;

        public DeathHookController(DeathHookSettings settings, IDeathHookOutput output, Func<float> clock)
        {
            this.settings = settings;
            this.output = output;
            this.clock = clock;
        }

        public DeathHookState State { get; private set; }

        public float Amount => amount;

        public bool PianoAllowed => settings.PianoWhileHookPlays || State == DeathHookState.Idle || State == DeathHookState.FadingOut;

        public void PlayerDied()
        {
            if (!output.HasClip) return;
            if (State == DeathHookState.Idle || settings.RestartHookOnEveryDeath)
            {
                hookStarted = false;
                delayLeft = settings.StartDelaySeconds;
            }
            lastTime = clock();
            State = DeathHookState.FadingIn;
        }

        public void PlayerRespawned()
        {
            if (State == DeathHookState.Idle || State == DeathHookState.FadingOut) return;
            if (!hookStarted)
            {
                Stop();
                return;
            }
            lastTime = clock();
            State = DeathHookState.FadingOut;
        }

        public void Stop()
        {
            if (State == DeathHookState.Idle) return;
            output.StopHook();
            amount = 0f;
            hookStarted = false;
            State = DeathHookState.Idle;
            output.Apply(DeathHookMix.Neutral);
        }

        public void Tick()
        {
            if (State == DeathHookState.Idle) return;
            var now = clock();
            var seconds = Mathf.Max(0f, now - lastTime);
            lastTime = now;
            Advance(seconds);
            if (State != DeathHookState.Idle) output.Apply(CurrentMix());
        }

        void Advance(float seconds)
        {
            switch (State)
            {
                case DeathHookState.FadingIn:
                    FadeIn(seconds);
                    break;
                case DeathHookState.Playing:
                    if (settings.PlayHookOnce && !output.HookIsPlaying) State = DeathHookState.WaitingForRespawn;
                    break;
                case DeathHookState.FadingOut:
                    FadeOut(seconds);
                    break;
            }
        }

        void FadeIn(float seconds)
        {
            if (!hookStarted)
            {
                delayLeft -= seconds;
                if (delayLeft > 0f) return;
                seconds = -delayLeft;
                output.StartHook(!settings.PlayHookOnce);
                hookStarted = true;
            }
            amount = Step(amount, 1f, seconds, settings.FadeInSeconds);
            if (amount >= 1f) State = DeathHookState.Playing;
        }

        void FadeOut(float seconds)
        {
            amount = Step(amount, 0f, seconds, settings.FadeOutSeconds);
            if (amount > 0f) return;
            output.StopHook();
            hookStarted = false;
            State = DeathHookState.Idle;
            output.Apply(DeathHookMix.Neutral);
        }

        DeathHookMix CurrentMix() => new DeathHookMix(
            settings.HookVolume * amount,
            Mathf.Lerp(1f, settings.LoopVolumeWhileHookPlays, amount),
            Mathf.Lerp(1f, settings.CaseMusicVolumeWhileHookPlays, amount));

        static float Step(float value, float target, float seconds, float fadeSeconds)
        {
            if (fadeSeconds <= 0f) return target;
            return Mathf.MoveTowards(value, target, seconds / fadeSeconds);
        }
    }
}
