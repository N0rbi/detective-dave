using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// One flicker: a short timeline of brightness keys. Between keys the brightness moves in a straight line; before the first key
    /// it holds the first value, after the last key it holds the last value. A pattern without keys leaves the light unchanged.
    /// </summary>
    [Serializable]
    public sealed class FlickerPattern
    {
        public const float UnchangedBrightness = 1f;

        [Tooltip("A name for the designer. Not shown to players.")]
        [SerializeField] string name = "Flicker";

        [Min(0.05f)]
        [Tooltip("How long the flicker lasts, in seconds.")]
        [SerializeField] float duration = 0.4f;

        [Tooltip("The brightness keys over time. Edit them on the timeline.")]
        [SerializeField] List<FlickerPoint> points = new List<FlickerPoint>();

        [Min(0f)]
        [Tooltip("How likely this pattern is picked compared with the others. 0 never picks it.")]
        [SerializeField] float weight = 1f;

        public FlickerPattern() { }

        public FlickerPattern(string name, float duration, IEnumerable<FlickerPoint> points, float weight = 1f)
        {
            this.name = name;
            this.duration = duration;
            this.points = new List<FlickerPoint>(points);
            this.weight = weight;
        }

        public string Name => name;

        public float Duration => duration;

        public float Weight => weight;

        public IReadOnlyList<FlickerPoint> Points => points;

        /// <summary>The brightness (0..1) at <paramref name="time"/> seconds into the pattern.</summary>
        public float Evaluate(float time)
        {
            if (points == null || points.Count == 0) return UnchangedBrightness;
            bool hasBefore = false, hasAfter = false;
            FlickerPoint before = default, after = default;
            foreach (var point in points)
            {
                if (point.Time <= time && (!hasBefore || point.Time >= before.Time))
                {
                    before = point;
                    hasBefore = true;
                }
                else if (point.Time > time && (!hasAfter || point.Time < after.Time))
                {
                    after = point;
                    hasAfter = true;
                }
            }
            if (!hasBefore) return after.Brightness;
            if (!hasAfter) return before.Brightness;
            float span = after.Time - before.Time;
            return span <= 0f ? after.Brightness : Mathf.Lerp(before.Brightness, after.Brightness, (time - before.Time) / span);
        }
    }
}
