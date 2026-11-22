using System;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>One key of a flicker timeline: at <see cref="Time"/> seconds the light is at <see cref="Brightness"/> (0 dark, 1 full) of its level brightness.</summary>
    [Serializable]
    public struct FlickerPoint
    {
        [Min(0f)] public float Time;
        [Range(0f, 1f)] public float Brightness;

        public FlickerPoint(float time, float brightness)
        {
            Time = time;
            Brightness = brightness;
        }
    }
}
