namespace DetectiveDave.OverworldPlay
{
    public static class LetterText
    {
        public const string Tail = "\n\nYours sincerely,\nDetective Dave";

        public static string Head(string client) => "Dear " + client + ",\n\n";

        public static string Compose(string client, string message) => Head(client) + message + Tail;
    }
}
