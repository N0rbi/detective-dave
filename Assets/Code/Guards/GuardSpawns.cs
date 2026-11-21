using System.Collections.Generic;

namespace DetectiveDave.GuardPlay
{
    public static class GuardSpawns
    {
        public static List<GuardSpawn> Build(IEnumerable<GuardPatrolData> patrols, KeyPathfinder pathfinder, GuardPassability passability, float defaultPatrolSeconds, float defaultChaseSeconds)
        {
            var spawns = new List<GuardSpawn>();
            foreach (var patrol in patrols)
            {
                if (!passability.CanStandOn(patrol.StartKey)) continue;
                var route = RouteOf(patrol, pathfinder, passability);
                var settings = new GuardBrainSettings(
                    patrol.WaitBeforeMoveSeconds,
                    patrol.WaitAtEndSeconds,
                    patrol.PatrolSecondsPerKey > 0f ? patrol.PatrolSecondsPerKey : defaultPatrolSeconds,
                    patrol.ChaseSecondsPerKey > 0f ? patrol.ChaseSecondsPerKey : defaultChaseSeconds);
                spawns.Add(new GuardSpawn(route, settings));
            }
            return spawns;
        }

        static IReadOnlyList<UnityEngine.InputSystem.Key> RouteOf(GuardPatrolData patrol, KeyPathfinder pathfinder, GuardPassability passability)
        {
            var route = passability.CanStandOn(patrol.EndKey) ? pathfinder.FindPath(patrol.StartKey, patrol.EndKey) : null;
            return route != null && route.Count > 0 ? route : new[] { patrol.StartKey };
        }
    }
}
