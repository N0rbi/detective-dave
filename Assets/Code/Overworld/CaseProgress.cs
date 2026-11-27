using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.OverworldPlay
{
    public sealed class CaseProgress
    {
        readonly IProgressStore store;
        readonly HashSet<string> solved;

        public CaseProgress(IProgressStore store)
        {
            this.store = store;
            solved = new HashSet<string>(store.LoadSolved());
        }

        public bool IsSolved(string caseId) => solved.Contains(caseId);

        public CaseState StateOf(string caseId, IReadOnlyList<string> requiredIds)
        {
            if (IsSolved(caseId)) return CaseState.Solved;
            return requiredIds.All(IsSolved) ? CaseState.Open : CaseState.Locked;
        }

        public void MarkSolved(string caseId)
        {
            if (!solved.Add(caseId)) return;
            Save();
        }

        public void Reset()
        {
            solved.Clear();
            Save();
        }

        void Save() => store.SaveSolved(solved.ToArray());
    }
}
