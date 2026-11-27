using System.Collections.Generic;
using DetectiveDave.OverworldPlay;

namespace DetectiveDave.EditorTools
{
    public static class DeskPreviewState
    {
        public static CaseState Of(DeskPreviewMode mode, string caseId, IReadOnlyList<string> requiredIds, CaseProgress progress)
        {
            switch (mode)
            {
                case DeskPreviewMode.Locked: return CaseState.Locked;
                case DeskPreviewMode.Solved: return CaseState.Solved;
                case DeskPreviewMode.ByProgressFile: return progress.StateOf(caseId, requiredIds);
                default: return CaseState.Open;
            }
        }
    }
}
