using System;
using System.Collections.Generic;
using DetectiveDave.GuardPlay;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DeskRoute
    {
        static readonly IReadOnlyList<Key> NoPath = new Key[0];

        readonly KeyPathfinder pathfinder;
        readonly DeskKeyMap map;
        readonly IReadOnlyList<string> caseIdsInDeskOrder;
        readonly Func<string, CaseState> stateOf;

        public DeskRoute(KeyPathfinder pathfinder, DeskKeyMap map, IReadOnlyList<string> caseIdsInDeskOrder, Func<string, CaseState> stateOf)
        {
            this.pathfinder = pathfinder;
            this.map = map;
            this.caseIdsInDeskOrder = caseIdsInDeskOrder;
            this.stateOf = stateOf;
        }

        public IReadOnlyList<Key> ToNearestOpenCase(Key from)
        {
            var shortest = NoPath;
            foreach (var caseId in caseIdsInDeskOrder)
            {
                if (stateOf(caseId) != CaseState.Open) continue;
                var path = ShortestPathToCase(from, caseId);
                if (path.Count > 0 && (shortest.Count == 0 || path.Count < shortest.Count)) shortest = path;
            }
            return shortest;
        }

        IReadOnlyList<Key> ShortestPathToCase(Key from, string caseId)
        {
            var shortest = NoPath;
            foreach (var key in map.KeysOf(caseId))
            {
                var path = pathfinder.FindPath(from, key);
                if (path.Count > 0 && (shortest.Count == 0 || path.Count < shortest.Count)) shortest = path;
            }
            return shortest;
        }
    }
}
