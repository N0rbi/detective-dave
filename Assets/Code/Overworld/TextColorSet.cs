using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public readonly struct TextColorSet
    {
        public TextColorSet(Color description, Color findings, Color typed, Color placeholder, Color acceptPending, Color declinePending)
        {
            Description = description;
            Findings = findings;
            Typed = typed;
            Placeholder = placeholder;
            AcceptPending = acceptPending;
            DeclinePending = declinePending;
        }

        public Color Description { get; }

        public Color Findings { get; }

        public Color Typed { get; }

        public Color Placeholder { get; }

        public Color AcceptPending { get; }

        public Color DeclinePending { get; }

        public Color ForSlot(FontSlot slot)
        {
            switch (slot)
            {
                case FontSlot.Request:
                    return Description;
                case FontSlot.Findings:
                    return Findings;
                case FontSlot.Typed:
                    return Typed;
                case FontSlot.Placeholder:
                case FontSlot.Notice:
                    return Placeholder;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, "No colour for this font slot");
            }
        }
    }
}
