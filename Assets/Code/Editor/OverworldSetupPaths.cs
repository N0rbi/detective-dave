namespace DetectiveDave.EditorTools
{
    static class OverworldSetupPaths
    {
        public const string OverworldFolder = "Assets/Overworld";
        public const string CasesFolder = "Assets/Cases";
        public const string GlassPath = PlayerSetupPaths.DecorationsFolder + "/stealth_goal_glass.png";
        public const string FolderPath = PlayerSetupPaths.DecorationsFolder + "/stealth_goal_folder.png";
        public const string SolvedPath = PlayerSetupPaths.GoalIconPath;
        public const string SettingsPath = OverworldFolder + "/Overworld Settings.config.asset";
        public const string MagnifierSettingsPath = PlayerSetupPaths.PlayerFolder + "/Magnifier Movement Settings.config.asset";
        public const string SampleCasePath = CasesFolder + "/Sample Case.case.asset";
        public const string SampleDeskPath = OverworldFolder + "/Sample Desk.prefab";
        public const string ScenePath = "Assets/Scenes/Overworld.unity";
        public const string LevelPath = "Assets/Levels/Level 01.prefab";

        public static readonly string[] SpritePaths = { GlassPath, FolderPath, SolvedPath };
    }
}
