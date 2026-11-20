using System.Collections.Generic;

namespace DetectiveDave.OverworldPlay
{
    public sealed class StoryTyper
    {
        readonly PrefixTyper typer;
        readonly int[] unitEnds;

        public StoryTyper(string text, TypingRules rules, RevealUnit unit)
        {
            Text = text ?? string.Empty;
            typer = new PrefixTyper(new[] { Text }, rules);
            unitEnds = UnitEnds(Text, unit);
        }

        public string Text { get; }

        public int TypedCount => typer.TypedCount;

        public bool IsComplete => typer.IsComplete;

        public int RevealedCount
        {
            get
            {
                int revealed = 0;
                foreach (var end in unitEnds)
                {
                    if (end > typer.TypedCount) break;
                    revealed = end;
                }
                return revealed;
            }
        }

        public bool TryType(char character) => typer.TryType(character);

        public bool Backspace() => typer.Backspace();

        static int[] UnitEnds(string text, RevealUnit unit)
        {
            var ends = new List<int>();
            for (int position = 0; position < text.Length; position++)
            {
                if (TryGetUnitEnd(text, position, unit, out var end)) ends.Add(end);
            }
            if (text.Length > 0 && (ends.Count == 0 || ends[ends.Count - 1] != text.Length)) ends.Add(text.Length);
            return ends.ToArray();
        }

        static bool TryGetUnitEnd(string text, int position, RevealUnit unit, out int end)
        {
            end = position + 1;
            switch (unit)
            {
                case RevealUnit.Character: return true;
                case RevealUnit.Word: return char.IsWhiteSpace(text[position]) && !IsWhiteSpaceAt(text, position + 1);
                default: return EndsSentence(text, position, out end);
            }
        }

        static bool EndsSentence(string text, int position, out int end)
        {
            end = position + 1;
            if (!IsSentenceTerminator(text[position]) || (position + 1 < text.Length && !IsWhiteSpaceAt(text, position + 1))) return false;
            while (IsWhiteSpaceAt(text, end)) end++;
            return true;
        }

        static bool IsSentenceTerminator(char character) => character == '.' || character == '!' || character == '?';

        static bool IsWhiteSpaceAt(string text, int position) => position < text.Length && char.IsWhiteSpace(text[position]);
    }
}
