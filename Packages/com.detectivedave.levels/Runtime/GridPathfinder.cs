using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>A* over a grid of cells: 8 directions, no cutting corners past blocked cells.</summary>
    internal static class GridPathfinder
    {
        const float StraightCost = 1f;
        const float DiagonalCost = 1.41421356f;

        static readonly Vector2Int[] Steps =
        {
            Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        /// <summary>
        /// The cells from <paramref name="start"/> to <paramref name="goal"/>, both included, or an empty list when there is
        /// no way. Cells outside <paramref name="bounds"/> are never entered.
        /// </summary>
        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, RectInt bounds, Func<Vector2Int, bool> passable)
        {
            var path = new List<Vector2Int>();
            if (!bounds.Contains(start) || !bounds.Contains(goal) || !passable(start) || !passable(goal)) return path;

            var cost = new Dictionary<Vector2Int, float> { [start] = 0f };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var open = new List<Vector2Int> { start };
            var closed = new HashSet<Vector2Int>();

            while (open.Count > 0)
            {
                var current = TakeBest(open, cell => cost[cell] + Heuristic(cell, goal));
                if (current == goal) return Rebuild(cameFrom, start, goal);
                closed.Add(current);

                foreach (var step in Steps)
                {
                    var next = current + step;
                    if (closed.Contains(next) || !bounds.Contains(next) || !passable(next)) continue;
                    bool diagonal = step.x != 0 && step.y != 0;
                    if (diagonal && !(passable(current + new Vector2Int(step.x, 0)) && passable(current + new Vector2Int(0, step.y)))) continue;

                    float newCost = cost[current] + (diagonal ? DiagonalCost : StraightCost);
                    if (cost.TryGetValue(next, out var known) && known <= newCost) continue;
                    cost[next] = newCost;
                    cameFrom[next] = current;
                    if (!open.Contains(next)) open.Add(next);
                }
            }
            return path;
        }

        // Octile distance: exact when nothing is in the way.
        static float Heuristic(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(from.x - to.x);
            int dy = Mathf.Abs(from.y - to.y);
            return StraightCost * (Mathf.Max(dx, dy) - Mathf.Min(dx, dy)) + DiagonalCost * Mathf.Min(dx, dy);
        }

        // A plain scan is enough: a level is a few hundred cells.
        static Vector2Int TakeBest(List<Vector2Int> open, Func<Vector2Int, float> score)
        {
            int best = 0;
            for (int i = 1; i < open.Count; i++)
                if (score(open[i]) < score(open[best])) best = i;
            var cell = open[best];
            open.RemoveAt(best);
            return cell;
        }

        static List<Vector2Int> Rebuild(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int goal)
        {
            var path = new List<Vector2Int> { goal };
            for (var cell = goal; cell != start; cell = cameFrom[cell]) path.Add(cameFrom[cell]);
            path.Reverse();
            return path;
        }
    }
}
