using System.Collections.Generic;
using UnityEditor;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>Ready-made flicker patterns a designer starts from, then adjusts on the timeline.</summary>
    internal static class FlickerPresets
    {
        public static readonly IReadOnlyList<FlickerPattern> All = new[]
        {
            new FlickerPattern("Blink", 0.4f, new[] { P(0f, 1f), P(0.08f, 0.15f), P(0.16f, 1f), P(0.24f, 0.3f), P(0.4f, 1f) }),
            new FlickerPattern("Stutter", 0.7f, new[] { P(0f, 1f), P(0.05f, 0f), P(0.1f, 1f), P(0.2f, 0f), P(0.25f, 0.8f), P(0.35f, 0f), P(0.5f, 1f) }),
            new FlickerPattern("Dim and recover", 1.2f, new[] { P(0f, 1f), P(0.3f, 0.25f), P(0.8f, 0.25f), P(1.2f, 1f) }),
            new FlickerPattern("Short out", 0.9f, new[] { P(0f, 1f), P(0.1f, 0.5f), P(0.15f, 1f), P(0.3f, 0f), P(0.75f, 0f), P(0.9f, 1f) }),
            new FlickerPattern("Buzz", 0.5f, new[] { P(0f, 1f), P(0.05f, 0.7f), P(0.1f, 1f), P(0.15f, 0.7f), P(0.2f, 1f), P(0.25f, 0.7f), P(0.3f, 1f) })
        };

        public static void Append(SerializedProperty patterns, FlickerPattern preset)
        {
            int index = patterns.arraySize;
            patterns.arraySize = index + 1;
            var created = patterns.GetArrayElementAtIndex(index);
            created.FindPropertyRelative("name").stringValue = preset.Name;
            created.FindPropertyRelative("duration").floatValue = preset.Duration;
            created.FindPropertyRelative("weight").floatValue = 1f;
            ReplacePoints(created.FindPropertyRelative("points"), preset);
        }

        public static void ReplacePoints(SerializedProperty points, FlickerPattern preset)
        {
            points.arraySize = preset.Points.Count;
            for (int index = 0; index < preset.Points.Count; index++)
            {
                var point = points.GetArrayElementAtIndex(index);
                point.FindPropertyRelative("Time").floatValue = preset.Points[index].Time;
                point.FindPropertyRelative("Brightness").floatValue = preset.Points[index].Brightness;
            }
        }

        static FlickerPoint P(float time, float brightness) => new FlickerPoint(time, brightness);
    }
}
