using System;

namespace DetectiveDave.OverworldPlay
{
    public interface IBriefingScreen
    {
        void Open(string caseId, Action<LetterOutcome> finished);

        void OpenReport(string caseId, Action finished);

        void OpenReview(string caseId, Action finished);

        void Refuse(string message, Action finished);

        void Close(Action done);
    }
}
