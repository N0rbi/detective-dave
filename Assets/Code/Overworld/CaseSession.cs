using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class CaseSession
    {
        public static CaseFile Current { get; private set; }

        public static string SolvedCaseId { get; private set; }

        public static bool IsActive => Current;

        public static void Begin(CaseFile caseFile)
        {
            Current = caseFile;
            SolvedCaseId = null;
        }

        public static void MarkLevelSolved()
        {
            if (Current) SolvedCaseId = Current.Id;
        }

        public static bool TryTakeSolved(out string caseId)
        {
            caseId = SolvedCaseId;
            SolvedCaseId = null;
            Current = null;
            return !string.IsNullOrEmpty(caseId);
        }

        public static void Clear()
        {
            Current = null;
            SolvedCaseId = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Clear();
    }
}
