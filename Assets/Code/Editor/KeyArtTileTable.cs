using System.Collections.Generic;

namespace DetectiveDave.EditorTools
{
    static class KeyArtTileTable
    {
        public const int PipeStripHeightPixels = 172;
        public const string UnassignedStagedName = "keyboard_stealth_pipe.png";

        public static readonly IReadOnlyList<KeyArtTileRow> Boxes = new[]
        {
            new KeyArtTileRow("Wall", "keyboard_stealth_brick.png", "Assets/Key Art/Wall", "wall_box.png", 0),
            new KeyArtTileRow("WallGutter", "keyboard_stealth_climb.png", "Assets/Key Art/WallGutter", "wallgutter_box.png", PipeStripHeightPixels),
            new KeyArtTileRow("Gap", "keyboard_stealth_hole.png", "Assets/Key Art/Gap", "gap_box.png", 0),
        };
    }
}
