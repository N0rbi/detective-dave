using System.Collections.Generic;
using System.Linq;
using DetectiveDave.OverworldPlay;

namespace DetectiveDave.EditorTools
{
    static class CaseChainInputs
    {
        public static CaseNode[] NodesOn(DeskLayout desk)
        {
            var onDesk = new HashSet<CaseFile>(DeskDesignInputs.AllMarkers(desk).Select(marker => marker.CaseFile).Where(file => file));
            var everyCase = new List<CaseFile>();
            var pending = new Stack<CaseFile>(onDesk);
            var seen = new HashSet<CaseFile>();
            while (pending.Count > 0)
            {
                var file = pending.Pop();
                if (!seen.Add(file)) continue;
                everyCase.Add(file);
                foreach (var required in file.Requires.Where(required => required)) pending.Push(required);
            }
            return everyCase.Select(file => new CaseNode(file.Id, file.RequiredIds, onDesk.Contains(file), file.HasLevel)).ToArray();
        }

        public static bool HasNoLevel(CaseMarker marker) => marker.CaseFile && !marker.CaseFile.HasLevel;
    }
}
