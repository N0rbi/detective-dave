namespace DetectiveDave.EditorTools
{
    enum KeyArtSlotState
    {
        Missing,
        NeedsMove,
        InPlace,
        Conflict,
    }

    static class KeyArtSlot
    {
        public static KeyArtSlotState Classify(bool stagedExists, bool targetExists)
        {
            if (stagedExists && targetExists) return KeyArtSlotState.Conflict;
            if (stagedExists) return KeyArtSlotState.NeedsMove;
            return targetExists ? KeyArtSlotState.InPlace : KeyArtSlotState.Missing;
        }
    }
}
