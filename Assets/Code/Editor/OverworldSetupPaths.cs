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

        public const string ArtFolder = OverworldFolder + "/Art";
        public const string AudioFolder = OverworldFolder + "/Audio";
        public const string FontFolder = OverworldFolder + "/Fonts";
        public const string BoardPath = ArtFolder + "/keyboard_stealth_board.png";
        public const string SmokingBodyPath = ArtFolder + "/stealth_detective_cigi_body.png";
        public const string SmokingArmPath = ArtFolder + "/stealth_detective_cigi_arm.png";
        public const string DescriptionPaperPath = ArtFolder + "/placeholder_paper_description.png";
        public const string LetterPaperPath = ArtFolder + "/placeholder_paper_letter.png";
        public const string RingPath = ArtFolder + "/placeholder_ring.png";
        public const string KeycapPath = ArtFolder + "/placeholder_keycap.png";
        public const string BackspacePath = AudioFolder + "/placeholder_typewriter_backspace.wav";
        public const string ReportFontPath = FontFolder + "/1942_report.ttf";
        public const string PlaceholderFontPath = FontFolder + "/jmh_typewriter.ttf";
        public const string TypedFontPath = FontFolder + "/moms_typewriter.ttf";
        public const string RequestFontPath = PlaceholderFontPath;
        public const string FindingsFontPath = PlaceholderFontPath;
        public const string NoticeFontPath = ReportFontPath;

        public static readonly string[] ClickPaths =
        {
            AudioFolder + "/placeholder_typewriter_click_1.wav",
            AudioFolder + "/placeholder_typewriter_click_2.wav",
            AudioFolder + "/placeholder_typewriter_click_3.wav",
        };

        public static readonly string[] PlaceholderPaths =
        {
            DescriptionPaperPath, LetterPaperPath, RingPath, KeycapPath, BackspacePath,
            ClickPaths[0], ClickPaths[1], ClickPaths[2],
        };

        public static readonly string[] SpritePaths = { GlassPath, FolderPath, SolvedPath };

        public static readonly string[] SceneArtPaths = { BoardPath, SmokingBodyPath, SmokingArmPath };

        public static readonly string[] FontPaths = { ReportFontPath, PlaceholderFontPath, TypedFontPath };
    }
}
