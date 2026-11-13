using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>A pickable layout on the selection screen, resolved from a <see cref="LayoutOption"/>.</summary>
    public class LayoutChoice
    {
        public KeyboardLayout Layout { get; }
        public Key Key { get; }                  // the number-row key that picks it
        public SpriteRenderer Glyph { get; }

        public LayoutChoice(KeyboardLayout layout, Key key, SpriteRenderer glyph)
        {
            Layout = layout;
            Key = key;
            Glyph = glyph;
        }
    }
}
