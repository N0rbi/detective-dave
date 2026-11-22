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

        public static bool IsFreeCharacter(char character) => char.IsWhiteSpace(character) || IsMark(character);

        public bool IsAutoTyped(char character) => char.IsWhiteSpace(character) || (AutoTypeFreeCharacters && IsMark(character));

        static bool IsMark(char character) => char.IsPunctuation(character) || char.IsSymbol(character);
    }
}
