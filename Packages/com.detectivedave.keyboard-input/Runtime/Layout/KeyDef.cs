using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// One key of a <see cref="KeyboardLayout"/>. Read-only; the serialized field names are the JSON schema.
    /// A key is identified by an Input System <see cref="Key"/>, which names a PHYSICAL position (US-layout naming),
    /// not the character the OS layout types: <see cref="Label"/> is what the player sees.
    /// </summary>
    [Serializable]
    public sealed class KeyDef
    {
        [SerializeField] string code;          // Input System Key name, e.g. "Digit1"
        [SerializeField] string label;         // the one legend shown on the key
        [SerializeField] KeyRect[] rects;      // one rectangle, two for the L-shaped Enter
        [SerializeField] Vector2 center;       // anchor point of the key, key units
        [SerializeField] string[] alsoCodes;   // other keys that also count as this one (optional)
        [SerializeField] bool inert;           // never goes down itself (still drawn, and may stand in for another key via alsoCodes)
        [SerializeField] bool unverified;      // Unity key code not yet confirmed on hardware
        [SerializeField] string note;

        [NonSerialized] bool resolved;
        [NonSerialized] Key? inputKey;
        [NonSerialized] Key[] alsoKeys;

        /// <summary>The Input System key name from the layout file, e.g. "Digit1". May not be a valid <see cref="Key"/> name, see <see cref="InputKey"/>.</summary>
        public string Code => code;

        /// <summary>The legend shown on the key in this layout.</summary>
        public string Label => label;

        /// <summary>The rectangles of the key, in key units: one, or two for an L-shaped Enter.</summary>
        public IReadOnlyList<KeyRect> Rects => rects ?? Array.Empty<KeyRect>();

        /// <summary>The anchor point of the key (for example where a character stands on it), in key units.</summary>
        public Vector2 Center => center;

        /// <summary>The raw <c>alsoCodes</c> names: other physical keys that count as this key. See <see cref="AlsoKeys"/>.</summary>
        public IReadOnlyList<string> AlsoCodes => alsoCodes ?? Array.Empty<string>();

        /// <summary>True when the key itself never goes down (it is still part of the board, and its <see cref="AlsoKeys"/> are ignored too).</summary>
        public bool IsInert => inert;

        /// <summary>True when the key code has not been confirmed on hardware yet.</summary>
        public bool IsUnverified => unverified;

        /// <summary>Free-form authoring note from the layout file.</summary>
        public string Note => note;

        /// <summary>The Input System key this entry stands for, or null when <see cref="Code"/> is not a <see cref="Key"/> name.</summary>
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

        /// <summary>The keys listed in <see cref="AlsoCodes"/>, unknown names skipped.</summary>
        public IReadOnlyList<Key> AlsoKeys => alsoKeys ??= (alsoCodes ?? Array.Empty<string>())
            .Select(ParseKey)
            .Where(key => key.HasValue)
            .Select(key => key.Value)
            .ToArray();

        // Names only: Enum.TryParse would also take numbers ("0" is Key.None, "999" is no key at all), and the keyboard has no control for those.
        static Key? ParseKey(string name)
        {
            if (string.IsNullOrEmpty(name) || !char.IsLetter(name[0])) return null;
            return Enum.TryParse(name, false, out Key key) && key != Key.None && Enum.IsDefined(typeof(Key), key) ? key : (Key?)null;
        }
    }
}
