using System.Collections.Generic;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DetectiveDave.EditorTools
{
    [InitializeOnLoad]
    static class LightingScenePreview
    {
        static readonly Dictionary<Light2D, LightLook> originals = new Dictionary<Light2D, LightLook>();

        static LightingScenePreview()
        {
            LightingPreviewHost.Apply = Apply;
            LightingPreviewHost.Restore = Restore;
            EditorSceneManager.sceneSaving += StopBeforeSaving;
        }

        static void StopBeforeSaving(UnityEngine.SceneManagement.Scene scene, string path) => LightingPreview.Stop();

        static void Apply(LevelLightingSettings settings, float flickerBrightness)
        {
            if (originals.Count == 0) Capture();
            foreach (var pair in originals)
            {
                if (!pair.Key) continue;
                pair.Key.intensity = LightingMath.LightIntensity(pair.Value.Intensity, settings, flickerBrightness);
                pair.Key.color = LightingMath.LightColor(pair.Value.Color, settings);
            }
        }

        static void Restore()
        {
            foreach (var pair in originals)
            {
                if (!pair.Key) continue;
                pair.Key.intensity = pair.Value.Intensity;
                pair.Key.color = pair.Value.Color;
            }
            originals.Clear();
        }

        static void Capture()
        {
            foreach (var light in Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None))
                if (!EditorUtility.IsPersistent(light)) originals[light] = new LightLook(light.intensity, light.color);
        }

        readonly struct LightLook
        {
            public float Intensity { get; }
            public Color Color { get; }

            public LightLook(float intensity, Color color)
            {
                Intensity = intensity;
                Color = color;
            }
        }
    }
}
