namespace DetectiveDave.CheatPlay
{
    public interface ICheatEnvironment
    {
        void ForgetSolvedCases();

        void ClearCaseSession();

        void LoadOverworld();

        void ShowMessage(string text);
    }
}
