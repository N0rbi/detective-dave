namespace DetectiveDave.EditorTools
{
    sealed class KeyArtTileRow
    {
        public string TileName { get; }
        public string StagedName { get; }
        public string Folder { get; }
        public string TargetName { get; }
        public int TopBorderPixels { get; }

        public KeyArtTileRow(string tileName, string stagedName, string folder, string targetName, int topBorderPixels)
        {
            TileName = tileName;
            StagedName = stagedName;
            Folder = folder;
            TargetName = targetName;
            TopBorderPixels = topBorderPixels;
        }

        public string TilePath => KeyArtSetupPaths.TilesFolder + "/" + TileName + ".asset";
        public string StagedPath => KeyArtSetupPaths.StagingFolder + "/" + StagedName;
        public string TargetPath => Folder + "/" + TargetName;
    }
}
