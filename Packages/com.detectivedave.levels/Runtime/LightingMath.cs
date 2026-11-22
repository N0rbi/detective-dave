using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>The numbers that end up on a light: its own setup, the level's lighting and the current flicker brightness combined.</summary>
    public static class LightingMath
    {
        public static float LightIntensity(float ownIntensity, LevelLightingSettings settings, float flickerBrightness) =>
            ownIntensity * settings.Intensity * Mathf.Clamp01(flickerBrightness);

        public static Color LightColor(Color ownColor, LevelLightingSettings settings) => ownColor * settings.ColorTint;
    }
}
