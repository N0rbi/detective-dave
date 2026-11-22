namespace DetectiveDave.OverworldPlay
{
    public readonly struct Glyph
    {
        public Glyph(char character, FontSlot slot, string colorHex, int row, int column)
        {
            Character = character;
            Slot = slot;
            ColorHex = colorHex;
            Row = row;
            Column = column;
        }

        public char Character { get; }

        public FontSlot Slot { get; }

        public string ColorHex { get; }

        public int Row { get; }

        public int Column { get; }
    }
}
