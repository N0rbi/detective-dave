using System;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// The sprite for one key shape. A key shape is a size in key units (1 x 1 is a letter key, 2.25 x 1 a left Shift).
    /// Keys made of several rectangles (the L-shaped ISO Enter) can have one sprite over the whole key: tick <c>Whole Key</c>
    /// and give the size of the box around the key (1.5 x 2 for the Enter).
    /// </summary>
    [Serializable]
    public sealed class KeyShapeSprite
    {
        /// <summary>How far apart two sizes may be, in key units, and still count as the same shape.</summary>
        public const float SizeTolerance = 0.01f;

        [Tooltip("A name for this row of the list, e.g. \"Shift\". Only for the inspector.")]
        [SerializeField] string label;

        [Tooltip("The key rectangle this sprite is for, in key units (width x height). For a Whole Key sprite: the box around the whole key.")]
        [SerializeField] Vector2 size = Vector2.one;

        [Tooltip("Tick for keys made of several rectangles (the L-shaped Enter): this one sprite is stretched over the whole key. " +
                 "Without one, each rectangle of such a key uses the plain sprite for its own size.")]
        [SerializeField] bool wholeKey;

        [Tooltip("Stretched to fill the key (or the rectangle). Grayscale art is tinted by the style's Tint.")]
        [SerializeField] Sprite sprite;

        /// <summary>For serialization.</summary>
        public KeyShapeSprite() { }

        /// <summary>A shape entry; see the field tooltips for the meaning of each argument.</summary>
        public KeyShapeSprite(string label, Vector2 size, bool wholeKey, Sprite sprite)
        {
            this.label = label;
            this.size = size;
            this.wholeKey = wholeKey;
            this.sprite = sprite;
        }

        /// <summary>The inspector label.</summary>
        public string Label => label ?? string.Empty;

        /// <summary>The key (or rectangle) size in key units.</summary>
        public Vector2 Size => size;

        /// <summary>True when the sprite covers a whole multi-rectangle key.</summary>
        public bool WholeKey => wholeKey;

        /// <summary>The sprite, or null when the art is not there yet.</summary>
        public Sprite Sprite => sprite;

        /// <summary>True when this entry is for a shape of <paramref name="shapeSize"/>, within <see cref="SizeTolerance"/>.</summary>
        public bool Matches(Vector2 shapeSize, bool isWholeKey) =>
            wholeKey == isWholeKey &&
            Mathf.Abs(size.x - shapeSize.x) < SizeTolerance &&
            Mathf.Abs(size.y - shapeSize.y) < SizeTolerance;
    }
}
