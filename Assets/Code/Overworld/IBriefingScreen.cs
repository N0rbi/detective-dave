using System;

namespace DetectiveDave.OverworldPlay
{
    public interface IBriefingScreen
    {
        void Open(string caseId, Action<LetterOutcome> finished);

        void OpenFindings(string caseId, Action finished);

        void OpenReview(string caseId, Action finished);

        void Close(Action done);
    }
}
