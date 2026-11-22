using System;
using System.Collections.Generic;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// Decides how bright the lights are over time: full brightness, then after a random wait one random pattern plays, then the wait starts again.
    /// Call <see cref="Tick"/> once per frame; it returns the brightness multiplier (0..1) for that frame.
    /// </summary>
    public sealed class FlickerScheduler
    {
        readonly bool enabled;
        readonly float waitMin;
        readonly float waitMax;
        readonly IReadOnlyList<FlickerPattern> patterns;
        readonly Func<float> random01;
        FlickerPattern playing;
        float waitLeft;
        float playedSeconds;

        public FlickerScheduler(LevelLightingSettings settings, Func<float> random01)
            : this(settings.Flicker, settings.SecondsBetweenFlickersMin, settings.SecondsBetweenFlickersMax, settings.Patterns, random01) { }

        public FlickerScheduler(bool enabled, float waitMin, float waitMax, IReadOnlyList<FlickerPattern> patterns, Func<float> random01)
        {
            this.enabled = enabled;
            this.waitMin = Math.Min(waitMin, waitMax);
            this.waitMax = Math.Max(waitMin, waitMax);
            this.patterns = patterns;
            this.random01 = random01;
            waitLeft = NextWait();
        }

        public bool IsFlickering => playing != null;

        public float WaitLeft => waitLeft;

        public float Tick(float deltaSeconds)
        {
            if (!enabled || FlickerPicker.TotalWeight(patterns) <= 0f) return FlickerPattern.UnchangedBrightness;
            if (playing == null) return Wait(deltaSeconds);
            return Play(deltaSeconds);
        }

        float Wait(float deltaSeconds)
        {
            waitLeft -= deltaSeconds;
            if (waitLeft > 0f) return FlickerPattern.UnchangedBrightness;
            float overshoot = -waitLeft;
            int index = FlickerPicker.Pick(patterns, random01());
            if (index == FlickerPicker.NoPattern)
            {
                waitLeft = NextWait();
                return FlickerPattern.UnchangedBrightness;
            }
            playing = patterns[index];
            playedSeconds = 0f;
            return Play(overshoot);
        }

        float Play(float deltaSeconds)
        {
            playedSeconds += deltaSeconds;
            if (playedSeconds >= playing.Duration)
            {
                playing = null;
                waitLeft = NextWait();
                return FlickerPattern.UnchangedBrightness;
            }
            return playing.Evaluate(playedSeconds);
        }

        float NextWait() => waitMin + (waitMax - waitMin) * random01();
    }
}
