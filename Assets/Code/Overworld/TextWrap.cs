namespace DetectiveDave.OverworldPlay
{
    public static class TextWrap
    {
        public static string Wrap(string text, int maxLineLength)
        {
            if (string.IsNullOrEmpty(text) || maxLineLength < 1) return text ?? string.Empty;
            var characters = text.ToCharArray();
            int lineStart = 0;
            int lastSpace = -1;
            for (int position = 0; position < characters.Length; position++)
            {
                if (characters[position] == '\n')
                {
                    lineStart = position + 1;
                    lastSpace = -1;
                    continue;
                }
                if (characters[position] == ' ') lastSpace = position;
                if (position - lineStart + 1 <= maxLineLength || lastSpace < lineStart) continue;
                characters[lastSpace] = '\n';
                lineStart = lastSpace + 1;
                lastSpace = -1;
            }
            return new string(characters);
        }
    }
}
