namespace DetectiveDave.EditorTools
{
    static class MusicSetupPaths
    {
        public const string MusicFolder = "Assets/Audio/Music";
        public const string PianoFolder = MusicFolder + "/Piano";
        public const string SingleFolder = PianoFolder + "/Single";
        public const string StagingFolder = "Assets/Staging";
        public const string LoopPath = MusicFolder + "/JazzLoop.wav";
        public const string SettingsPath = MusicFolder + "/Music Settings.config.asset";
        public const string ChartPath = MusicFolder + "/Chord Chart.config.asset";
        public const string ChartFilePath = MusicFolder + "/chord_chart.json";

        public static bool IsSingleNotePath(string assetPath) => assetPath.StartsWith(SingleFolder + "/", System.StringComparison.Ordinal);
    }
}
