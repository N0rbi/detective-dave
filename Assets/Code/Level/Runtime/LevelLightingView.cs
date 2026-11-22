using System;
using System.Collections.Generic;
using DetectiveDave.Levels;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DetectiveDave.LevelPlay
{
    [RequireComponent(typeof(LevelLoader))]
    public sealed class LevelLightingView : MonoBehaviour
    {
        [Tooltip("Light every 2D light in the scene with the level's lighting. Off: only the lights in Target Lights.")]
        [SerializeField] bool useAllSceneLights = true;

        [Tooltip("The lights the level's lighting applies to when Use All Scene Lights is off.")]
        [SerializeField] Light2D[] targetLights = Array.Empty<Light2D>();

        readonly List<LightSetup> lights = new List<LightSetup>();
        LevelLoader loader;
        FlickerScheduler scheduler;
        LevelLightingSettings settings;
        float appliedBrightness = float.NaN;

        void Awake() => loader = GetComponent<LevelLoader>();

        void OnEnable() => loader.LevelChanged += OnLevelChanged;

        void OnDisable()
        {
            loader.LevelChanged -= OnLevelChanged;
            RestoreLights();
        }

        void Start() => OnLevelChanged();

        void Update()
        {
            if (scheduler == null) return;
            ApplyBrightness(scheduler.Tick(Time.deltaTime));
        }

        void OnLevelChanged()
        {
            var level = loader.Current;
            if (!level)
            {
                RestoreLights();
                return;
            }
            CaptureLights();
            settings = level.Lighting;
            scheduler = new FlickerScheduler(settings, () => UnityEngine.Random.value);
            appliedBrightness = float.NaN;
            ApplyBrightness(FlickerPattern.UnchangedBrightness);
        }

        void CaptureLights()
        {
            if (lights.Count > 0) return;
            var found = useAllSceneLights ? FindObjectsByType<Light2D>(FindObjectsSortMode.None) : targetLights;
            foreach (var light in found)
                if (light) lights.Add(new LightSetup(light, light.intensity, light.color));
        }

        void ApplyBrightness(float brightness)
        {
            if (Mathf.Approximately(brightness, appliedBrightness)) return;
            appliedBrightness = brightness;
            foreach (var setup in lights)
            {
                if (!setup.Light) continue;
                setup.Light.intensity = LightingMath.LightIntensity(setup.Intensity, settings, brightness);
                setup.Light.color = LightingMath.LightColor(setup.Color, settings);
            }
        }

        void RestoreLights()
        {
            scheduler = null;
            foreach (var setup in lights)
            {
                if (!setup.Light) continue;
                setup.Light.intensity = setup.Intensity;
                setup.Light.color = setup.Color;
            }
            lights.Clear();
            appliedBrightness = float.NaN;
        }

        readonly struct LightSetup
        {
            public Light2D Light { get; }
            public float Intensity { get; }
            public Color Color { get; }

            public LightSetup(Light2D light, float intensity, Color color)
            {
                Light = light;
                Intensity = intensity;
                Color = color;
            }
        }
    }
}
