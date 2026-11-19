using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// Where the keys of a layout are in board space (the keyboard object's local space, which is also a level root's local space),
    /// with the same math as <see cref="KeyboardGridView"/>: the layout is fitted into <see cref="LevelSettings.BoardSize"/>, centred.
    /// And which tilemap cells a key covers: a cell belongs to a key when its centre lies inside one of the key's rectangles.
    /// </summary>
    public static class KeyGeometry
    {
        /// <summary>The point a key reads its tile at: its centre nudged up and right, exactly as the game does.</summary>
        public static readonly Vector2 CenterNudge = new Vector2(0.01f, 0.01f);

        /// <summary>Key units to board units for <paramref name="layout"/>.</summary>
        public static float Scale(KeyboardLayout layout) => GridUtils.FitScale(layout, LevelSettings.instance.BoardSize);

        /// <summary>The key's centre in board space, before the nudge.</summary>
        public static Vector2 Center(KeyboardLayout layout, KeyDef key) => GridUtils.ToLocal(layout, Scale(layout), key.Center);

        /// <summary>The key's rectangles in board space (y up).</summary>
        public static Rect[] Rects(KeyboardLayout layout, KeyDef key)
        {
            float scale = Scale(layout);
            var result = new Rect[key.Rects.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var rect = key.Rects[i];
                var size = rect.Size * scale;
                result[i] = new Rect(GridUtils.ToLocal(layout, scale, rect.Center) - size * 0.5f, size);
            }
            return result;
        }

        /// <summary>The key with a rectangle containing <paramref name="board"/>, or null. Inert keys count: they are on the board.</summary>
        public static KeyDef KeyAt(KeyboardLayout layout, Vector2 board)
        {
            foreach (var key in layout.UniqueKeys())
                foreach (var rect in Rects(layout, key))
                    if (rect.Contains(board)) return key;
            return null;
        }

        /// <summary>The cells of <paramref name="tilemap"/> whose centres lie inside the key. <paramref name="boardSpace"/> is the level root (or keyboard).</summary>
        public static List<Vector3Int> Cells(Tilemap tilemap, Transform boardSpace, KeyboardLayout layout, KeyDef key)
        {
            var cells = new List<Vector3Int>();
            foreach (var rect in Rects(layout, key))
            {
                var a = tilemap.WorldToCell(boardSpace.TransformPoint(rect.min));
                var b = tilemap.WorldToCell(boardSpace.TransformPoint(rect.max));
                var min = Vector3Int.Min(a, b) - Vector3Int.one;
                var max = Vector3Int.Max(a, b) + Vector3Int.one;
                for (int x = min.x; x <= max.x; x++)
                    for (int y = min.y; y <= max.y; y++)
                    {
                        var cell = new Vector3Int(x, y, 0);
                        Vector2 center = boardSpace.InverseTransformPoint(tilemap.GetCellCenterWorld(cell));
                        if (rect.Contains(center) && !cells.Contains(cell)) cells.Add(cell);
                    }
            }
            return cells;
        }

        /// <summary>The cell a key reads its tile from (its nudged centre).</summary>
        public static Vector3Int ReadCell(Tilemap tilemap, Transform boardSpace, KeyboardLayout layout, KeyDef key) =>
            tilemap.WorldToCell(boardSpace.TransformPoint(Center(layout, key) + CenterNudge));

        /// <summary>The board space for a tilemap: its level root when it is part of a level, else its grid.</summary>
        public static Transform BoardSpaceOf(Tilemap tilemap)
        {
            var level = tilemap.GetComponentInParent<Levels.Level>(true);
            if (level) return level.BoardSpace;
            return tilemap.layoutGrid ? tilemap.layoutGrid.transform : tilemap.transform;
        }
    }
}
