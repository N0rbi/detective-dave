namespace DetectiveDave.CheatPlay
{
    public sealed class ResetPlayerCheat : IGameCheat
    {
        readonly ICheatEnvironment environment;
        readonly string message;

        public ResetPlayerCheat(ICheatEnvironment environment, string message)
        {
            this.environment = environment;
            this.message = message;
        }

        public void Run()
        {
            environment.ForgetSolvedCases();
            environment.ClearCaseSession();
            environment.ShowMessage(message);
            environment.LoadOverworld();
        }
    }
}
