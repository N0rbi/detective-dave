using System.Collections.Generic;

namespace DetectiveDave.Levels
{
    /// <summary>Picks one flicker pattern at random, more often the ones with a higher weight.</summary>
    public static class FlickerPicker
    {
        public const int NoPattern = -1;

        /// <summary>The index of the picked pattern, or <see cref="NoPattern"/> when none can be picked. <paramref name="random01"/> is a number from 0 up to 1.</summary>
        public static int Pick(IReadOnlyList<FlickerPattern> patterns, float random01)
        {
            float total = TotalWeight(patterns);
            if (total <= 0f) return NoPattern;
            float target = random01 * total;
            int lastPickable = NoPattern;
            for (int index = 0; index < patterns.Count; index++)
            {
                float weight = WeightOf(patterns[index]);
                if (weight <= 0f) continue;
                lastPickable = index;
                if (target < weight) return index;
                target -= weight;
            }
            return lastPickable;
        }

        public static float TotalWeight(IReadOnlyList<FlickerPattern> patterns)
        {
            float total = 0f;
            if (patterns == null) return total;
            foreach (var pattern in patterns) total += WeightOf(pattern);
            return total;
        }

        static float WeightOf(FlickerPattern pattern) => pattern == null ? 0f : pattern.Weight;
    }
}
