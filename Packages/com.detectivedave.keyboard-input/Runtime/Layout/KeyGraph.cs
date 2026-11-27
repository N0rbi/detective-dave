using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// Which keys of a layout touch which. Two keys are neighbours when their rectangles share a border of any length,
    /// so a key off by a quarter of a unit still counts. Built from the layout rectangles alone; symmetric; immutable.
    /// </summary>
    public sealed class KeyGraph
    {
        static readonly IReadOnlyCollection<Key> NoKeys = Array.Empty<Key>();

        readonly Dictionary<Key, HashSet<Key>> neighbours;

        /// <summary>Works out the neighbours of every key of <paramref name="layout"/> (its <see cref="KeyboardLayout.UniqueKeys"/>, inert keys included).</summary>
        public KeyGraph(KeyboardLayout layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            var defs = layout.UniqueKeys().ToArray();
            neighbours = defs.ToDictionary(
                def => def.InputKey.Value,
                def => new HashSet<Key>(defs
                    .Where(other => other != def && Touches(def, other))
                    .Select(other => other.InputKey.Value)));
        }

        /// <summary>Do the two keys share a border? False for a key the layout does not have, and for a key and itself.</summary>
        public bool AreNeighbours(Key key, Key other) => neighbours.TryGetValue(key, out var set) && set.Contains(other);

        /// <summary>The neighbours of a key, empty for a key the layout does not have.</summary>
        public IReadOnlyCollection<Key> Of(Key key) => neighbours.TryGetValue(key, out var set) ? set : NoKeys;

        static bool Touches(KeyDef a, KeyDef b) => a.Rects.Any(rect => b.Rects.Any(rect.SharesBorderWith));
    }
}
