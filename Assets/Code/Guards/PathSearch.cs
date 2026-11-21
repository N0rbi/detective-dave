using System;
using System.Collections.Generic;

namespace DetectiveDave.GuardPlay
{
    public static class PathSearch
    {
        public static List<T> Find<T>(
            T start,
            T goal,
            Func<T, IEnumerable<T>> neighboursOf,
            Func<T, bool> passable,
            Func<T, float> estimateToGoal,
            IComparer<T> tieBreak)
        {
            var empty = new List<T>();
            var equality = EqualityComparer<T>.Default;
            if (!passable(start) || !passable(goal)) return empty;

            var cost = new Dictionary<T, float> { [start] = 0f };
            var cameFrom = new Dictionary<T, T>();
            var open = new List<T> { start };
            var closed = new HashSet<T>();

            while (open.Count > 0)
            {
                var current = TakeBest(open, cost, estimateToGoal, tieBreak);
                if (equality.Equals(current, goal)) return Rebuild(cameFrom, start, goal, equality);
                closed.Add(current);

                foreach (var next in neighboursOf(current))
                {
                    if (closed.Contains(next) || !passable(next)) continue;
                    float newCost = cost[current] + 1f;
                    if (cost.TryGetValue(next, out var known) && known <= newCost) continue;
                    cost[next] = newCost;
                    cameFrom[next] = current;
                    if (!open.Contains(next)) open.Add(next);
                }
            }
            return empty;
        }

        static T TakeBest<T>(List<T> open, Dictionary<T, float> cost, Func<T, float> estimateToGoal, IComparer<T> tieBreak)
        {
            int best = 0;
            for (int i = 1; i < open.Count; i++)
            {
                float candidate = cost[open[i]] + estimateToGoal(open[i]);
                float current = cost[open[best]] + estimateToGoal(open[best]);
                if (candidate < current || (candidate == current && tieBreak.Compare(open[i], open[best]) < 0)) best = i;
            }
            var taken = open[best];
            open.RemoveAt(best);
            return taken;
        }

        static List<T> Rebuild<T>(Dictionary<T, T> cameFrom, T start, T goal, EqualityComparer<T> equality)
        {
            var path = new List<T> { goal };
            for (var step = goal; !equality.Equals(step, start); step = cameFrom[step]) path.Add(cameFrom[step]);
            path.Reverse();
            return path;
        }
    }
}
