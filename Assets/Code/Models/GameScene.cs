namespace DetectiveDave
{
    /// <summary>
    /// Every scene the game loads from code. The member name is the scene file name without ".unity".
    /// The numbers are fixed because scenes serialize this enum as a number: never renumber, only add.
    /// </summary>
    public enum GameScene
    {
        KeyboardSelect = 0,
        KeyboardGrid = 1,
        Game = 2,           // was KeyboardLevel: the scene that plays levels
        Overworld = 3,      // the desk with the case files
    }
}
