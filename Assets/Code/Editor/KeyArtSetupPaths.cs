namespace DetectiveDave.EditorTools
{
    static class KeyArtSetupPaths
    {
        public const string StagingFolder = "Assets/Staging/Tiles";
        public const string WalkableFolder = "Assets/Key Art/Walkable";
        public const string TilesFolder = "Assets/Tiles";
        public const string WalkableTilePath = TilesFolder + "/Walkable.asset";
        public const string StagedBoxName = "keyboard_stealth_platform.png";
        public const string StagedEnterName = "keyboard_stealth_platform_enter.png";
        public const string BoxName = "walkable_box.png";
        public const string EnterName = "walkable_enter.png";
        public const string StagedBoxPath = StagingFolder + "/" + StagedBoxName;
        public const string StagedEnterPath = StagingFolder + "/" + StagedEnterName;
        public const string BoxPath = WalkableFolder + "/" + BoxName;
        public const string EnterPath = WalkableFolder + "/" + EnterName;
        public const int PixelsPerUnit = 16;
        public const int MaxTextureSize = 1024;
    }
}
