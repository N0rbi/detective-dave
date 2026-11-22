namespace DetectiveDave.OverworldPlay
{
    public static class CharacterMatch
    {
        public const char Whitespace = ' ';

        public static char Canonical(char character) => char.IsWhiteSpace(character) ? Whitespace : char.ToLowerInvariant(character);
    }
}
