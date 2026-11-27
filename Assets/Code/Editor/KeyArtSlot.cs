namespace DetectiveDave.EditorTools
{
    enum KeyArtSlotState
    {
        Missing,
        NeedsMove,
        InPlace,
        NeedsReplace,
    }

    static class KeyArtSlot
    {
        public static KeyArtSlotState Classify(bool stagedExists, bool targetExists)
        {
            if (stagedExists && targetExists) return KeyArtSlotState.NeedsReplace;
            if (stagedExists) return KeyArtSlotState.NeedsMove;
            return targetExists ? KeyArtSlotState.InPlace : KeyArtSlotState.Missing;
        }
    }
}
