using System;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DeskCases
    {
        readonly DeskKeyMap map;
        readonly Func<string, CaseState> stateOf;

        public DeskCases(DeskKeyMap map, Func<string, CaseState> stateOf)
        {
            this.map = map;
            this.stateOf = stateOf;
        }

        public bool TryGetCase(Key key, out string caseId) => map.TryGetCase(key, out caseId);

        public bool TryGetState(Key key, out CaseState state)
        {
            state = default;
            if (!map.TryGetCase(key, out var caseId)) return false;
            state = stateOf(caseId);
            return true;
        }
    }
}
