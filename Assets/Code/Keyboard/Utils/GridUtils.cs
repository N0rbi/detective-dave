using System.Collections.Generic;
using DetectiveDave.KeyboardInput;
using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Conversions between the two spaces of the keyboard grid:
    /// key units (origin top-left, y down) and the grid object's local space (origin at the center, y up), plus sizing.
    /// </summary>
    public static class GridUtils
    {
        /// <summary>Key units to world units, so that the layout fits inside <paramref name="area"/>.</summary>
        public static float FitScale(KeyboardLayout layout, Vector2 area) =>
            Mathf.Min(area.x / layout.UnitsWide, area.y / layout.UnitsHigh);

        public static Vector2 ToLocal(KeyboardLayout layout, float scale, Vector2 keyUnits)
        {
            var fromCenter = keyUnits - layout.Size * 0.5f;
            return new Vector2(fromCenter.x, -fromCenter.y) * scale;
        }

        /// <summary>The scale that stretches a sprite of <paramref name="spriteSize"/> (world units) to <paramref name="target"/>.</summary>
        public static Vector3 ScaleToFit(Vector2 target, Vector2 spriteSize) =>
            new Vector3(target.x / spriteSize.x, target.y / spriteSize.y, 1f);

        const float JoinTolerance = 1e-4f;

        /// <summary>The box around all rectangles of a key, shrunk by <paramref name="gap"/> like a single rectangle, in key units.</summary>
        public static Rect KeyBounds(IReadOnlyList<KeyRect> rects, float gap)
        {
            float left = float.MaxValue, top = float.MaxValue, right = float.MinValue, bottom = float.MinValue;
            foreach (var rect in rects)
            {
                left = Mathf.Min(left, rect.X);
                top = Mathf.Min(top, rect.Y);
                right = Mathf.Max(right, rect.X + rect.Width);
                bottom = Mathf.Max(bottom, rect.Y + rect.Height);
            }
            float half = gap * 0.5f;
            return new Rect(left + half, top + half, right - left - gap, bottom - top - gap);
        }

        /// <summary>
        /// The drawn rectangle of one part of a key, in key units: shrunk by <paramref name="gap"/> so neighbouring keys do not touch,
        /// except where it joins another part of the same key. There the part that fits inside the other one reaches across the gap,
        /// so a two-part key (the L-shaped Enter) is drawn as one piece.
        /// </summary>
        public static Rect PartRect(IReadOnlyList<KeyRect> rects, int index, float gap)
        {
            var r = rects[index];
            float half = gap * 0.5f;
            float left = r.X + half, right = r.X + r.Width - half, top = r.Y + half, bottom = r.Y + r.Height - half;
            for (int other = 0; other < rects.Count; other++)
            {
                var o = rects[other];
                if (other == index || !r.SharesBorderWith(o)) continue;
                bool stacked = Mathf.Abs(r.Y + r.Height - o.Y) <= JoinTolerance || Mathf.Abs(o.Y + o.Height - r.Y) <= JoinTolerance;   // one above the other
                float from = stacked ? r.X : r.Y, to = from + (stacked ? r.Width : r.Height);
                float otherFrom = stacked ? o.X : o.Y, otherTo = otherFrom + (stacked ? o.Width : o.Height);
                bool inside = from >= otherFrom - JoinTolerance && to <= otherTo + JoinTolerance;
                bool sameSpan = Mathf.Abs(from - otherFrom) <= JoinTolerance && Mathf.Abs(to - otherTo) <= JoinTolerance;
                if (!inside || (sameSpan && index > other)) continue;
                if (stacked) { if (o.Y > r.Y) bottom += gap; else top -= gap; }
                else { if (o.X > r.X) right += gap; else left -= gap; }
            }
            return new Rect(left, top, right - left, bottom - top);
        }
    }
}
