namespace DetectiveDave.EditorTools
{
    public enum CaseChainIssueKind
    {
        CircularRequires,
        RequiredCaseNotOnDesk,
        NoOpenCaseAtStart,
        DuplicateId,
        NoLevel
    }

    public readonly struct CaseChainIssue
    {
        public CaseChainIssueKind Kind { get; }
        public string CaseId { get; }
        public string OtherCaseId { get; }

        public CaseChainIssue(CaseChainIssueKind kind, string caseId, string otherCaseId)
        {
            Kind = kind;
            CaseId = caseId;
            OtherCaseId = otherCaseId;
        }
    }
}
