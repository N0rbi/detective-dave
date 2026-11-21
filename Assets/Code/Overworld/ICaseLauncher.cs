namespace DetectiveDave.OverworldPlay
{
    public interface ICaseLauncher
    {
        bool CanLaunch(string caseId);

        void Launch(string caseId);
    }
}
