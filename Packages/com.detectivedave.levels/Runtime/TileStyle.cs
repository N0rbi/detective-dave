using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>How keys of one <see cref="TileType"/> look in a level: a tint and one sprite per key shape.</summary>
    [Serializable]
    public sealed class TileStyle
    {
        [Tooltip("The tile type these sprites are for.")]
        [SerializeField] TileType type;

        [Tooltip("Multiplied with the sprites (and used alone on keys that have no sprite). White keeps the art's own colours.")]
        [SerializeField] Color tint = Color.white;

        [Tooltip("One entry per key shape. Use \"Add Standard Key Shapes\" in the Key Art inspector to fill the shapes every keyboard layout uses.")]
        [SerializeField] KeyShapeSprite[] shapes = Array.Empty<KeyShapeSprite>();

        /// <summary>For serialization.</summary>
        public TileStyle() { }

        /// <summary>A style for <paramref name="type"/>; see the field tooltips for the meaning of each argument.</summary>
        public TileStyle(TileType type, Color tint, IEnumerable<KeyShapeSprite> shapes)
        {
            this.type = type;
            this.tint = tint;
            this.shapes = shapes == null ? Array.Empty<KeyShapeSprite>() : new List<KeyShapeSprite>(shapes).ToArray();
        }

        /// <summary>The type this style is for (null when not set yet).</summary>
        public TileType Type => type;

        /// <summary>The colour multiplied with the sprites.</summary>
        public Color Tint => tint;

        /// <summary>The shape entries in inspector order.</summary>
        public IReadOnlyList<KeyShapeSprite> Shapes => Array.AsReadOnly(shapes ?? Array.Empty<KeyShapeSprite>());

        /// <summary>The sprite of the first entry that matches the shape and has a sprite, or null.</summary>
        public Sprite FindSprite(Vector2 size, bool wholeKey)
        {
            foreach (var shape in shapes ?? Array.Empty<KeyShapeSprite>())
                if (shape != null && shape.Sprite && shape.Matches(size, wholeKey)) return shape.Sprite;
            return null;
        }
    }
}
