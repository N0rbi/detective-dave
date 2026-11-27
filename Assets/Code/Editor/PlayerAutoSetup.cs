namespace DetectiveDave.EditorTools
{
    static class PlayerAutoSetup
    {
        public static bool EnsureReady()
        {
            var error = PlayerSetup.ApplyOrError();
            if (error != null) PlayerSetup.ShowProblem("Play This Level", "The player cannot be set up:\n\n" + error);
            return error == null;
        }
    }
}
