namespace DetectiveDave.OverworldPlay
{
    public readonly struct LetterPalette
    {
        public LetterPalette(string typedHex, string pendingHex, string acceptPendingHex, string declinePendingHex)
        {
            TypedHex = typedHex;
            PendingHex = pendingHex;
            AcceptPendingHex = acceptPendingHex;
            DeclinePendingHex = declinePendingHex;
        }

        public string TypedHex { get; }

        public string PendingHex { get; }

        public string AcceptPendingHex { get; }

        public string DeclinePendingHex { get; }
    }
}
