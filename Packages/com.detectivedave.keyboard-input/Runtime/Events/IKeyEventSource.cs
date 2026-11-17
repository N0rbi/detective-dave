using System;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// The keyboard as a stream of key events. This is the whole contract between the input package and its consumers, and it is ONE-WAY:
    /// the source only produces events. It takes nothing back: no queries into game state, no callbacks that return values,
    /// no way for a listener to change what the source reports. Listeners keep whatever state they need themselves.
    /// <para>
    /// A key is an Input System <see cref="Key"/>, which names a physical position (US-layout naming), independent of the OS keyboard layout.
    /// Keys are independent: there are no chords and no modifiers. Within a frame the events of one key come in the order
    /// pressed, held, released; keys follow the order of the layout file, toggle keys (Caps Lock) last.
    /// </para>
    /// </summary>
    public interface IKeyEventSource
    {
        /// <summary>A key of the layout went down. Also raised for a tap shorter than a frame (then followed by <see cref="KeyReleased"/> in the same frame).</summary>
        event Action<Key> KeyPressed;

        /// <summary>Every frame a key of the layout is down, including the frame it went down.</summary>
        event Action<Key> KeyHeld;

        /// <summary>A key of the layout came up.</summary>
        event Action<Key> KeyReleased;

        /// <summary>
        /// Diagnostic stream: every physical key going down this frame, whether the layout has it or not, with the name the OS gives it.
        /// Raised after the layout events of the same frame. Not affected by aliases, inert keys or toggle latching.
        /// </summary>
        event Action<KeyPress> RawKeyPressed;
    }
}
