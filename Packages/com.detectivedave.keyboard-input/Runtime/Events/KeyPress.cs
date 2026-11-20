using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>One physical key going down, as reported by <see cref="IKeyEventSource.RawKeyPressed"/>. Immutable.</summary>
    public sealed class KeyPress
    {
        /// <summary>The physical key.</summary>
        public Key Key { get; }

        /// <summary>What the OS keyboard layout says the key types (Input System <c>displayName</c>), e.g. "ö" for <c>Key.Digit0</c> on a Hungarian layout.</summary>
        public string DisplayName { get; }

        /// <summary>The layout's entry for this key, or null when the layout does not have it.</summary>
        public KeyDef Def { get; }

        /// <summary>Does the layout have this key?</summary>
        public bool IsMapped => Def != null;

        internal KeyPress(Key key, string displayName, KeyDef def)
        {
            Key = key;
            DisplayName = displayName;
            Def = def;
        }
    }
}
