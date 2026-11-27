namespace DetectiveDave.EditorTools
{
    public enum GuardProblemKind
    {
        StartNotPlaced,
        EndNotPlaced,
        StartNotOnKeyboard,
        EndNotOnKeyboard,
        StartOnBlockedKey,
        EndOnBlockedKey,
        StartOnPlayerShift,
        NoPathBetweenStartAndEnd,
    }
}
