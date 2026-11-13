using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>One key of a layout file. Field names match the JSON.</summary>
    [Serializable]
    public class KeyDef
    {
        public string code;          // Input System Key name, e.g. "Digit1"
        public string label;         // the one legend shown on the key
        public KeyRect[] rects;      // one rectangle, two for the L-shaped Enter
        public Vector2 center;       // where the player stands, key units
        public string[] alsoCodes;   // other keys that also light this one (optional)
        public bool toggle;          // stateful key (Caps Lock): lights briefly on every state change instead of while held
        public bool inert;           // never lights itself (still drawn, and may light another key via alsoCodes)
        public bool unverified;      // Unity key code not yet confirmed on hardware
        public string note;

        [NonSerialized] bool resolved;
        [NonSerialized] Key? inputKey;
        [NonSerialized] Key[] alsoKeys;

        /// <summary>The Input System key this entry stands for, or null when <see cref="code"/> is not a Key name.</summary>
        public Key? InputKey
        {
            get
            {
                if (!resolved)
                {
                    resolved = true;
                    inputKey = ParseKey(code);
                }
                return inputKey;
            }
        }

        /// <summary>The keys listed in <see cref="alsoCodes"/>, unknown names skipped.</summary>
        public Key[] AlsoKeys => alsoKeys ??= (alsoCodes ?? Array.Empty<string>())
            .Select(ParseKey)
            .Where(key => key.HasValue)
            .Select(key => key.Value)
            .ToArray();

        static Key? ParseKey(string name) => Enum.TryParse<Key>(name, out var key) ? (Key?)key : null;
    }
}
