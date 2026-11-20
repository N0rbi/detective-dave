namespace DetectiveDave.EditorTools
{
    static class PlayerSetupPaths
    {
        public const string PlayerFolder = "Assets/Player";
        public const string ArtFolder = PlayerFolder + "/Art";
        public const string WalkFramePattern = ArtFolder + "/stealth_detective_walk_cycle_{0}.png";
        public const string ClimbFramePattern = ArtFolder + "/stealth_detective_climb_cycle_{0}.png";
        public const string DecorationsFolder = "Assets/Decorations";
        public const string StartGratePath = DecorationsFolder + "/stealth_start_grate.png";
        public const string GoalIconPath = DecorationsFolder + "/stealth_goal_both.png";
        public const string GoalBurstPath = DecorationsFolder + "/stealth_goal_decoration.png";
        public const string SettingsPath = PlayerFolder + "/Player Movement Settings.asset";
        public const string PrefabPath = PlayerFolder + "/Player.prefab";
        public const string ScenePath = "Assets/Scenes/Game.unity";
        public const string TilePattern = "Assets/Tiles/{0}.asset";
        public const int FrameCount = 3;

        public static readonly string[] DecorationPaths = { StartGratePath, GoalIconPath, GoalBurstPath };

        public static readonly string[] TileNames = { "Gap", "WallGutter" };
    }
}
