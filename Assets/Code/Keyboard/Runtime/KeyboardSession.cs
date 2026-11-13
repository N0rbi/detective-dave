using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>The layout the player picked. Set once, kept until the game quits, never saved.</summary>
    public static class KeyboardSession
    {
        public static KeyboardLayout Layout { get; private set; }
        public static bool IsLocked => Layout != null;

        /// <summary>Locks the layout. Returns false when one is already locked.</summary>
        public static bool Lock(KeyboardLayout layout)
        {
            if (IsLocked) return false;
            Layout = layout;
            return true;
        }

        // Keeps Play Mode honest when domain reload is turned off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Layout = null;
    }
}
