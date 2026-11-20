using System;

namespace DetectiveDave.OverworldPlay
{
    public interface IBriefingScreen
    {
        void Open(string caseId, Action<LetterOutcome> finished);

        void Close(Action done);
    }
}
