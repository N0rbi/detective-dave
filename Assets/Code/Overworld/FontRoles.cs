using System;

namespace DetectiveDave.OverworldPlay
{
    public static class FontRoles
    {
        public static FontSlot SlotOf(TextRole role)
        {
            switch (role)
            {
                case TextRole.ClientMessage:
                    return FontSlot.Request;
                case TextRole.Placeholder:
                    return FontSlot.Placeholder;
                case TextRole.Findings:
                    return FontSlot.Findings;
                case TextRole.Typed:
                    return FontSlot.Typed;
                case TextRole.Notice:
                    return FontSlot.Notice;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, "No font slot for this role");
            }
        }

        public static FontSlot[] Fill(int length, TextRole role)
        {
            var slots = new FontSlot[length];
            Array.Fill(slots, SlotOf(role));
            return slots;
        }

        public static void FillRange(FontSlot[] slots, int start, int count, TextRole role)
        {
            if (count > 0) Array.Fill(slots, SlotOf(role), start, count);
        }
    }
}
