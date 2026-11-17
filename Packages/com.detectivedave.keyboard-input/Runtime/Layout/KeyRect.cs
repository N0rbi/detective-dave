using System;
using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// A rectangle of a key, in key units: origin at the top-left of the board, y pointing down. Part of a <see cref="KeyDef"/>.
    /// Read-only once loaded (the public parameterless constructor exists for Unity serialization only); the serialized field names (<c>x</c>, <c>y</c>, <c>w</c>, <c>h</c>) are the JSON schema.
    /// </summary>
    [Serializable]
    public sealed class KeyRect
    {
        const float BorderTolerance = 1e-4f;    // float slack when comparing edges

        [SerializeField] float x, y, w, h;

        /// <summary>Left edge, key units from the left of the board.</summary>
        public float X => x;

        /// <summary>Top edge, key units from the top of the board (y points down).</summary>
        public float Y => y;

        /// <summary>Width in key units.</summary>
        public float Width => w;

        /// <summary>Height in key units.</summary>
        public float Height => h;

        /// <summary>The middle of the rectangle, key units (y down).</summary>
        public Vector2 Center => new Vector2(x + w * 0.5f, y + h * 0.5f);

        /// <summary>Width and height in key units.</summary>
        public Vector2 Size => new Vector2(w, h);

        /// <summary>The length of the border this rectangle shares with another one, 0 when they only touch at a corner or not at all.</summary>
        public float SharedBorder(KeyRect other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            float overlapX = Mathf.Min(x + w, other.x + other.w) - Mathf.Max(x, other.x);
            float overlapY = Mathf.Min(y + h, other.y + other.h) - Mathf.Max(y, other.y);
            if (Mathf.Abs(overlapX) <= BorderTolerance && overlapY > BorderTolerance) return overlapY;   // side by side
            if (Mathf.Abs(overlapY) <= BorderTolerance && overlapX > BorderTolerance) return overlapX;   // one above the other
            return 0f;
        }

        /// <summary>Do the two rectangles share a border of any length? Touching only at a corner does not count.</summary>
        public bool SharesBorderWith(KeyRect other) => SharedBorder(other) > 0f;
    }
}
