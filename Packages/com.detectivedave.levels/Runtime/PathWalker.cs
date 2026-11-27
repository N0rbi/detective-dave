using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// Walks along a list of waypoints (for example a <see cref="LevelTerrain.FindPath"/> result) at a given speed.
    /// Plain data and math: the caller moves whatever it wants to with <see cref="Position"/>.
    /// </summary>
    public sealed class PathWalker
    {
        readonly IReadOnlyList<Vector2> path;
        int next = 1;

        /// <summary>Where the walker is now.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>True once the last waypoint is reached.</summary>
        public bool Done => next >= path.Count;

        /// <summary>A walker standing on the first waypoint. <paramref name="path"/> must have at least one point.</summary>
        public PathWalker(IReadOnlyList<Vector2> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path.Count == 0) throw new ArgumentException("A path needs at least one point.", nameof(path));
            this.path = path;
            Position = path[0];
        }

        /// <summary>Moves <paramref name="distance"/> along the path, through as many waypoints as that covers.</summary>
        // Every pass either reaches the next waypoint or uses up the step, so the loop always ends.
        public void Advance(float distance)
        {
            while (distance > 0f && !Done)
            {
                var toNext = path[next] - Position;
                float remaining = toNext.magnitude;
                if (remaining <= distance)
                {
                    Position = path[next];
                    distance -= remaining;
                    next++;
                }
                else
                {
                    Position += toNext / remaining * distance;
                    distance = 0f;
                }
            }
        }
    }
}
