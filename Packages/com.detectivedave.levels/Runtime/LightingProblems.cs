using System.Collections.Generic;

namespace DetectiveDave.Levels
{
    /// <summary>Finds what is wrong with a level's lighting, worded for designers.</summary>
    public static class LightingProblems
    {
        public static List<LightingProblem> Find(LevelLightingSettings settings)
        {
            var problems = new List<LightingProblem>();
            if (settings == null) return problems;
            if (settings.Intensity <= 0f)
                problems.Add(new LightingProblem(LightingProblemKind.LightsOff, false, "Lighting intensity is 0: the lights are off in this level."));
            if (!settings.Flicker) return problems;
            if (settings.SecondsBetweenFlickersMin > settings.SecondsBetweenFlickersMax)
                problems.Add(new LightingProblem(LightingProblemKind.WaitRangeReversed, true, "The shortest wait between flickers is longer than the longest wait. Swap them."));
            if (settings.Patterns.Count == 0)
            {
                problems.Add(new LightingProblem(LightingProblemKind.FlickerWithoutPatterns, true, "Flicker is on but there is no flicker pattern. Add one under Lighting."));
                return problems;
            }
            if (FlickerPicker.TotalWeight(settings.Patterns) <= 0f)
                problems.Add(new LightingProblem(LightingProblemKind.NoPatternCanBePicked, true, "Every flicker pattern has weight 0, so none can be picked. Give one a weight above 0."));
            for (int index = 0; index < settings.Patterns.Count; index++) AddPatternProblems(problems, settings.Patterns[index], index);
            return problems;
        }

        static void AddPatternProblems(List<LightingProblem> problems, FlickerPattern pattern, int index)
        {
            string label = pattern == null ? $"Flicker pattern {index + 1}" : $"Flicker pattern '{pattern.Name}'";
            if (pattern == null) return;
            if (pattern.Duration <= 0f)
                problems.Add(new LightingProblem(LightingProblemKind.PatternWithoutDuration, true, $"{label} has no duration. Give it a duration above 0.", index));
            if (pattern.Points.Count == 0)
                problems.Add(new LightingProblem(LightingProblemKind.PatternWithoutPoints, false, $"{label} has no points, so it does not change the light. Click on its timeline to add points.", index));
            foreach (var point in pattern.Points)
                if (point.Time > pattern.Duration)
                {
                    problems.Add(new LightingProblem(LightingProblemKind.PointAfterTheEnd, false, $"{label} has a point after its end. It is never reached: make the duration longer or move the point.", index));
                    break;
                }
        }
    }
}
