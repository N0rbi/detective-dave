using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// A physical keyboard layout: the size of the board and where every key is. Loaded from JSON with <see cref="Parse"/>;
    /// read-only afterwards. The serialized field names are the JSON schema (see the package documentation).
    /// </summary>
    [Serializable]
    public sealed class KeyboardLayout
    {
        [SerializeField] string id;
        [SerializeField] string name;
        [SerializeField] string form;          // ansi, iso or jis
        [SerializeField] int digit;            // number-row key that picks this layout in a layout picker
        [SerializeField] float unitsWide;
        [SerializeField] float unitsHigh;
        [SerializeField] KeyDef[] keys;

        /// <summary>Stable identifier, e.g. "hu".</summary>
        public string Id => id;

        /// <summary>Human readable name.</summary>
        public string Name => name;

        /// <summary>Physical form factor: "ansi", "iso" or "jis".</summary>
        public string Form => form;

        /// <summary>The number-row digit that picks this layout in a layout picker (1 = <c>Key.Digit1</c>).</summary>
        public int Digit => digit;

        /// <summary>Board width in key units.</summary>
        public float UnitsWide => unitsWide;

        /// <summary>Board height in key units.</summary>
        public float UnitsHigh => unitsHigh;

        /// <summary>Board size in key units.</summary>
        public Vector2 Size => new Vector2(unitsWide, unitsHigh);

        /// <summary>Every entry of the layout file, including ones whose code is not a valid key name.</summary>
        public IReadOnlyList<KeyDef> Keys => keys ?? Array.Empty<KeyDef>();

        /// <summary>The keys that have a valid Input System name, one entry per key (the first wins when a code is listed twice), in file order.</summary>
        public IEnumerable<KeyDef> UniqueKeys() => Keys
            .Where(def => def.InputKey.HasValue)
            .GroupBy(def => def.InputKey.Value)
            .Select(group => group.First());

        /// <summary>Reads a layout from a JSON text asset.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">The text is empty or not a layout.</exception>
        public static KeyboardLayout Parse(TextAsset json)
        {
            if (!json) throw new ArgumentNullException(nameof(json));
            return Parse(json.text);
        }

        /// <summary>Reads a layout from its JSON text.</summary>
        /// <exception cref="ArgumentException">The text is empty or not a layout.</exception>
        public static KeyboardLayout Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Layout JSON is empty.", nameof(json));
            var layout = JsonUtility.FromJson<KeyboardLayout>(json);
            if (layout == null) throw new ArgumentException("Layout JSON is not an object.", nameof(json));
            return layout;
        }
    }
}
