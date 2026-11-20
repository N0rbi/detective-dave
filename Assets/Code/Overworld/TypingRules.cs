namespace DetectiveDave.OverworldPlay
{
    public readonly struct TypingRules
    {
        public bool AutoTypeFreeCharacters { get; }

        public TypingRules(bool autoTypeFreeCharacters)
        {
            AutoTypeFreeCharacters = autoTypeFreeCharacters;
        }

        public static TypingRules Default => new TypingRules(true);

        public static bool IsFreeCharacter(char character) => character == '\n' || character == '\r' || char.IsPunctuation(character) || char.IsSymbol(character);

        public bool IsAutoTyped(char character) => AutoTypeFreeCharacters && IsFreeCharacter(character);
    }
}
