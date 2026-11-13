using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// Conversions between the three spaces of the keyboard grid:
    /// key units (origin top-left, y down), the grid object's local space (origin at the center, y up) and the screen.
    /// </summary>
    public static class GridUtils
    {
        /// <summary>Key units to world units, so that the layout fits inside <paramref name="area"/>.</summary>
        public static float FitScale(KeyboardLayout layout, Vector2 area) =>
            Mathf.Min(area.x / layout.unitsWide, area.y / layout.unitsHigh);

        public static Vector2 ToLocal(KeyboardLayout layout, float scale, Vector2 keyUnits)
        {
            var fromCenter = keyUnits - layout.Size * 0.5f;
            return new Vector2(fromCenter.x, -fromCenter.y) * scale;
        }

        public static Vector2 ToKeyUnits(KeyboardLayout layout, float scale, Vector2 local)
        {
            var fromCenter = new Vector2(local.x, -local.y) / scale;
            return fromCenter + layout.Size * 0.5f;
        }

        /// <summary>The drawn size of a key rectangle, shrunk by <paramref name="gap"/> (key units) so neighbours do not touch.</summary>
        public static Vector2 CellSize(KeyRect rect, float scale, float gap) =>
            (rect.Size - Vector2.one * gap) * scale;

        /// <summary>A screen position in the local space of the grid object. Needs an orthographic camera.</summary>
        public static Vector2 ScreenToLocal(Camera camera, Transform grid, Vector2 screenPosition)
        {
            float depth = Mathf.Abs(camera.transform.position.z - grid.position.z);
            var world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
            return grid.InverseTransformPoint(world);
        }

        /// <summary>The cell whose rectangles contain the point, or null.</summary>
        public static KeyCell FindCellAt(IEnumerable<KeyCell> cells, Vector2 keyUnits) =>
            cells.FirstOrDefault(cell => cell.Def.rects.Any(rect => rect.Contains(keyUnits)));
    }
}
