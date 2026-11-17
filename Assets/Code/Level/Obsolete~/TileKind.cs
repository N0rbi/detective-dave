namespace DetectiveDave.Level
{
    /// <summary>What a map tile is. The numbers are stored in the tile assets: never renumber.</summary>
    public enum TileKind
    {
        None = 0,         // nothing painted here
        Walkable = 1,
        Gap = 2,
        Wall = 3,
        WallGutter = 4,   // a wall with a gutter the player can climb
        Start = 5,
        Goal = 6,
    }
}
