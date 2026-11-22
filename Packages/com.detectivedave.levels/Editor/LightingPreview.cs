using System;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// Plays a flicker in Edit Mode: one pattern once, or the level's flicker the way the game plays it (random waits, random patterns).
    /// Drives the inspector's lamp and, through <see cref="LightingPreviewHost"/>, the lights of the open scenes. Stops by itself on Play Mode and scene save.
    /// </summary>
    public static class LightingPreview
    {
        const int NoPattern = -1;
        static LevelLightingSettings settings;
        static int patternIndex = NoPattern;
        static FlickerScheduler scheduler;
        static double lastTime;
        static float elapsed;

        public static event Action Changed;

        public static bool IsPlaying { get; private set; }

        public static float Multiplier { get; private set; } = 1f;

        public static bool IsSinglePattern => IsPlaying && patternIndex != NoPattern;

        public static bool IsPlayingPattern(int index) => IsPlaying && patternIndex == index;

        public static float Elapsed => elapsed;

        public static void PlayPattern(LevelLightingSettings forSettings, int index)
        {
            Begin(forSettings);
            patternIndex = index;
            scheduler = null;
        }

        public static void PlayLikeTheGame(LevelLightingSettings forSettings)
        {
            Begin(forSettings);
            patternIndex = NoPattern;
            scheduler = new FlickerScheduler(forSettings.Flicker, forSettings.SecondsBetweenFlickersMin, forSettings.SecondsBetweenFlickersMax, forSettings.Patterns, () => UnityEngine.Random.value);
        }

        public static void Stop()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            Multiplier = 1f;
            patternIndex = NoPattern;
            scheduler = null;
            EditorApplication.update -= Update;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload -= Stop;
            LightingPreviewHost.Restore?.Invoke();
            Changed?.Invoke();
        }

        static void Begin(LevelLightingSettings forSettings)
        {
            Stop();
            settings = forSettings;
            elapsed = 0f;
            Multiplier = 1f;
            lastTime = EditorApplication.timeSinceStartup;
            IsPlaying = true;
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
        }

        static bool TryEvaluatePattern()
        {
            if (patternIndex >= settings.Patterns.Count || elapsed >= settings.Patterns[patternIndex].Duration)
            {
                Stop();
                return false;
            }
            Multiplier = settings.Patterns[patternIndex].Evaluate(elapsed);
            return true;
        }

        static void OnPlayMode(PlayModeStateChange change) => Stop();

        static void Update()
        {
            double now = EditorApplication.timeSinceStartup;
            float delta = (float)(now - lastTime);
            lastTime = now;
            elapsed += delta;
            if (scheduler != null) Multiplier = scheduler.Tick(delta);
            else if (!TryEvaluatePattern()) return;
            LightingPreviewHost.Apply?.Invoke(settings, Multiplier);
            Changed?.Invoke();
        }
    }
}
