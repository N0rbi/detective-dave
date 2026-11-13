using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>One key going down on the keyboard.</summary>
    public class KeyPress
    {
        public Key Key { get; }
        public string DisplayName { get; }   // what the OS says the key types
        public KeyCell Cell { get; }         // null when the locked layout has no such key

        public bool IsMapped => Cell != null;

        public KeyPress(Key key, string displayName, KeyCell cell)
        {
            Key = key;
            DisplayName = displayName;
            Cell = cell;
        }
    }
}
