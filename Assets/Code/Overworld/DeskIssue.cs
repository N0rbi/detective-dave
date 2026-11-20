using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public readonly struct DeskIssue
    {
        public DeskIssueKind Kind { get; }
        public string CaseId { get; }
        public string OtherCaseId { get; }
        public Key Key { get; }

        public DeskIssue(DeskIssueKind kind, string caseId, string otherCaseId, Key key)
        {
            Kind = kind;
            CaseId = caseId;
            OtherCaseId = otherCaseId;
            Key = key;
        }
    }
}
