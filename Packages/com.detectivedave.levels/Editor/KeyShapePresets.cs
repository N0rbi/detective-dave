using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// The key shapes the shipped keyboard layouts use (US, UK, DE, HU, FR, JP main block): every plain key is one of seven widths
    /// and one unit high, and the ISO/JIS Enter is an L of two rectangles inside a 1.5 x 2 box.
    /// Used to fill new styles and to check that a level has art for every key. A new layout with a new key size goes here.
    /// </summary>
    internal static class KeyShapePresets
    {
        internal readonly struct Shape
        {
            public readonly string Label;
            public readonly Vector2 Size;
            public readonly bool WholeKey;

            public Shape(string label, Vector2 size, bool wholeKey)
            {
                Label = label;
                Size = size;
                WholeKey = wholeKey;
            }
        }

        public static readonly IReadOnlyList<Shape> Standard = new[]
        {
            new Shape("1 x 1 (letters, digits)", new Vector2(1f, 1f), false),
            new Shape("1.25 x 1", new Vector2(1.25f, 1f), false),
            new Shape("1.5 x 1 (Tab)", new Vector2(1.5f, 1f), false),
            new Shape("1.75 x 1 (Caps Lock)", new Vector2(1.75f, 1f), false),
            new Shape("2 x 1 (Backspace)", new Vector2(2f, 1f), false),
            new Shape("2.25 x 1 (Shift, ANSI Enter)", new Vector2(2.25f, 1f), false),
            new Shape("2.75 x 1 (right Shift)", new Vector2(2.75f, 1f), false),
            new Shape("Enter, L-shaped (whole key)", new Vector2(1.5f, 2f), true),
        };

        // Sprite names like "wall_1_25x1", "gap_2x1" or "goal_enter_1_5x2": "_" stands for the decimal point.
        static readonly Regex SizeInName = new Regex(@"(?:^|_)(?<whole>enter_)?(?<w>\d+(?:_\d+)?)x(?<h>\d+(?:_\d+)?)$", RegexOptions.IgnoreCase);

        /// <summary>Reads the shape from a sprite name such as "wall_1_25x1" or "wall_enter_1_5x2". False when the name has no size.</summary>
        public static bool TryParseSpriteName(string spriteName, out Vector2 size, out bool wholeKey)
        {
            size = default;
            wholeKey = false;
            var match = SizeInName.Match(spriteName ?? string.Empty);
            if (!match.Success) return false;
            size = new Vector2(ParseUnits(match.Groups["w"].Value), ParseUnits(match.Groups["h"].Value));
            wholeKey = match.Groups["whole"].Success;
            return size.x > 0f && size.y > 0f;
        }

        /// <summary>The preset label for a shape, or a plain "w x h" when it is not a standard one.</summary>
        public static string LabelFor(Vector2 size, bool wholeKey)
        {
            foreach (var shape in Standard)
                if (new KeyShapeSprite(null, shape.Size, shape.WholeKey, null).Matches(size, wholeKey)) return shape.Label;
            var text = $"{size.x.ToString(CultureInfo.InvariantCulture)} x {size.y.ToString(CultureInfo.InvariantCulture)}";
            return wholeKey ? text + " (whole key)" : text;
        }

        static float ParseUnits(string text) =>
            float.TryParse(text.Replace('_', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0f;
    }
}
