using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    public static class GuardProblemText
    {
        public static string Of(GuardProblemKind kind, Key start, Key end)
        {
            switch (kind)
            {
                case GuardProblemKind.StartNotPlaced: return "START is not placed. Click a key in the Scene view.";
                case GuardProblemKind.EndNotPlaced: return "END is not placed. Shift+click a key in the Scene view.";
                case GuardProblemKind.StartNotOnKeyboard: return $"START key {start} does not exist on this keyboard.";
                case GuardProblemKind.EndNotOnKeyboard: return $"END key {end} does not exist on this keyboard.";
                case GuardProblemKind.StartOnBlockedKey: return "START is on a wall, gap, gutter or empty key. Guards only stand on floor.";
                case GuardProblemKind.EndOnBlockedKey: return "END is on a wall, gap, gutter or empty key. Guards only stand on floor.";
                case GuardProblemKind.StartOnPlayerShift: return "START is on the key the player starts from.";
                default: return "There is no way over floor keys from START to END. The guard will stand still.";
            }
        }
    }
}
