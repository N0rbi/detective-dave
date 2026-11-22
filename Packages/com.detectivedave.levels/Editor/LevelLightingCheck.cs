using System.Collections.Generic;
using UnityEditor;

namespace DetectiveDave.Levels.Editor
{
    internal sealed class LevelLightingCheck : ILevelCheck
    {
        public void Check(Level level, List<LevelIssue> issues)
        {
            foreach (var problem in LightingProblems.Find(level.Lighting))
                issues.Add(new LevelIssue(problem.IsError ? MessageType.Error : MessageType.Warning, problem.Message, level, FixLabel(problem), FixFor(level, problem)));
        }

        static string FixLabel(LightingProblem problem)
        {
            switch (problem.Kind)
            {
                case LightingProblemKind.FlickerWithoutPatterns: return "Add a Blink pattern";
                case LightingProblemKind.WaitRangeReversed: return "Swap the waits";
                default: return null;
            }
        }

        static System.Action FixFor(Level level, LightingProblem problem)
        {
            switch (problem.Kind)
            {
                case LightingProblemKind.FlickerWithoutPatterns: return () => AddBlink(level);
                case LightingProblemKind.WaitRangeReversed: return () => SwapWaits(level);
                default: return null;
            }
        }

        static void AddBlink(Level level)
        {
            var serialized = new SerializedObject(level);
            FlickerPresets.Append(serialized.FindProperty("lighting.patterns"), FlickerPresets.All[0]);
            serialized.ApplyModifiedProperties();
        }

        static void SwapWaits(Level level)
        {
            var serialized = new SerializedObject(level);
            var min = serialized.FindProperty("lighting.secondsBetweenFlickersMin");
            var max = serialized.FindProperty("lighting.secondsBetweenFlickersMax");
            (min.floatValue, max.floatValue) = (max.floatValue, min.floatValue);
            serialized.ApplyModifiedProperties();
        }
    }
}
