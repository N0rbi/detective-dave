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

        [Tooltip("The 9-sliced picture every single-rectangle key is drawn with, whatever its size. Corners and edges keep their thickness, only the middle stretches.")]
        [SerializeField] Sprite box;

        [Tooltip("One picture over the whole L-shaped Enter key, stretched to the key. Without it each rectangle of the Enter is drawn with the Box.")]
        [SerializeField] Sprite enter;

        [Tooltip("Optional. A picture for one exact key size, used instead of the Box for that size. Leave empty when the Box is enough.")]
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

        /// <summary>A style with a Box and an Enter sprite and no shape overrides.</summary>
        public TileStyle(TileType type, Color tint, Sprite box, Sprite enter)
        {
            this.type = type;
            this.tint = tint;
            this.box = box;
            this.enter = enter;
        }

        /// <summary>The 9-sliced sprite for every single-rectangle key, or null.</summary>
        public Sprite Box => box;

        /// <summary>The sprite over the whole L-shaped Enter, or null.</summary>
        public Sprite Enter => enter;

        /// <summary>True when the style still draws keys from per-size sprites and has no Box: it works as before and can move to Box + Enter.</summary>
        public bool CanMigrateToBox => !box && HasShapeSprites;

        /// <summary>True when at least one shape override has a sprite.</summary>
        public bool HasShapeSprites
        {
            get
            {
                foreach (var shape in shapes ?? Array.Empty<KeyShapeSprite>())
                    if (shape != null && shape.Sprite) return true;
                return false;
            }
        }

        /// <summary>The type this style is for (null when not set yet).</summary>
        public TileType Type => type;

        /// <summary>The colour multiplied with the sprites.</summary>
        public Color Tint => tint;

        /// <summary>The shape entries in inspector order.</summary>
        public IReadOnlyList<KeyShapeSprite> Shapes => Array.AsReadOnly(shapes ?? Array.Empty<KeyShapeSprite>());

        /// <summary>
        /// What to draw for a key rectangle of <paramref name="size"/> (key units): a shape override first, then the Enter for a whole key,
        /// then the Box. Kind None means nothing is set: the caller draws the plain square, or each rectangle of a whole key on its own.
        /// </summary>
        public KeySpriteChoice Resolve(Vector2 size, bool wholeKey)
        {
            var overridden = FindSprite(size, wholeKey);
            if (overridden) return new KeySpriteChoice(KeySpriteKind.Stretched, overridden);
            if (wholeKey) return enter ? new KeySpriteChoice(KeySpriteKind.Stretched, enter) : KeySpriteChoice.None;
            return box ? new KeySpriteChoice(KeySpriteKind.Sliced, box) : KeySpriteChoice.None;
        }

        /// <summary>The sprite of the first entry that matches the shape and has a sprite, or null.</summary>
        public Sprite FindSprite(Vector2 size, bool wholeKey)
        {
            foreach (var shape in shapes ?? Array.Empty<KeyShapeSprite>())
                if (shape != null && shape.Sprite && shape.Matches(size, wholeKey)) return shape.Sprite;
            return null;
        }
    }
}
