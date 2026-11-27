using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// How the 2D lights look while a level plays: their brightness and colour, and an optional flicker. The values are applied to
    /// the lights of the Game scene, relative to how each light is set up in the scene: an Intensity of 1 and a white Colour leave every light as it is.
    /// </summary>
    [Serializable]
    public sealed class LevelLightingSettings
    {
        public const float DefaultIntensity = 1f;
        public const float DefaultSecondsBetweenFlickersMin = 4f;
        public const float DefaultSecondsBetweenFlickersMax = 9f;

        [Min(0f)]
        [Tooltip("How bright the lights are in this level, as a multiple of how they are set up in the scene. 1 leaves them as they are, 0 turns them off.")]
        [SerializeField] float intensity = DefaultIntensity;

        [Tooltip("Multiplied with each light's own colour. White leaves the colours as they are.")]
        [SerializeField] Color colorTint = Color.white;

        [Tooltip("Turn the flicker on. The lights then flicker now and then, using one of the patterns below.")]
        [SerializeField] bool flicker;

        [Min(0.1f)]
        [Tooltip("The shortest wait between two flickers, in seconds.")]
        [SerializeField] float secondsBetweenFlickersMin = DefaultSecondsBetweenFlickersMin;

        [Min(0.1f)]
        [Tooltip("The longest wait between two flickers, in seconds. Each wait is a random time between the shortest and the longest.")]
        [SerializeField] float secondsBetweenFlickersMax = DefaultSecondsBetweenFlickersMax;

        [Tooltip("The flickers this level can use. Each time a flicker happens one is picked at random.")]
        [SerializeField] List<FlickerPattern> patterns = new List<FlickerPattern>();

        public float Intensity => intensity;

        public Color ColorTint => colorTint;

        public bool Flicker => flicker;

        public float SecondsBetweenFlickersMin => secondsBetweenFlickersMin;

        public float SecondsBetweenFlickersMax => secondsBetweenFlickersMax;

        public IReadOnlyList<FlickerPattern> Patterns => patterns;
    }
}
