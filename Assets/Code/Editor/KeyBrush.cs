using System.Collections.Generic;
using DetectiveDave.KeyboardInput;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// The default brush of the Tile Palette ("Key"): a click paints (or erases) the whole key under the cursor, for the preview layout
    /// picked in the Levels window. Every cell whose centre lies inside the key gets the picked tile, so a key always reads as one kind.
    /// Clicks between keys do nothing. Box fill paints every key the box touches. Undo works per stroke.
    /// The level tools select it through the public <c>GridPaintingState.brushes</c> / <c>GridPaintingState.gridBrush</c>.
    /// </summary>
    [CustomGridBrush(false, false, true, "Key")]
    public sealed class KeyBrush : GridBrush
    {
        /// <summary>Tests set a layout here; null uses the preview layout.</summary>
        internal KeyboardLayout LayoutOverride { get; set; }

        /// <summary>The layout the brush paints keys of.</summary>
        public KeyboardLayout Layout => LayoutOverride ?? KeyboardLayouts.Preview;

        /// <summary>The tile a click paints: the first picked cell. Null when nothing is picked.</summary>
        public TileBase PickedTile => cells != null && cells.Length > 0 ? cells[0].tile : null;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position) =>
            SetKeys(brushTarget, new[] { position }, PickedTile, "Paint Key");

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position) =>
            SetKeys(brushTarget, new[] { position }, null, "Erase Key");

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position) =>
            SetKeys(brushTarget, Positions(position), PickedTile, "Fill Keys");

        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position) =>
            SetKeys(brushTarget, Positions(position), null, "Erase Keys");

        /// <summary>The cells of the key under <paramref name="position"/> on <paramref name="tilemap"/>; empty between keys.</summary>
        public List<Vector3Int> KeyCells(Tilemap tilemap, Vector3Int position)
        {
            var key = KeyUnder(tilemap, position);
            return key == null ? new List<Vector3Int>() : KeyGeometry.Cells(tilemap, KeyGeometry.BoardSpaceOf(tilemap), Layout, key);
        }

        /// <summary>The key under a cell, or null between keys (or without layouts).</summary>
        public KeyDef KeyUnder(Tilemap tilemap, Vector3Int position)
        {
            var layout = Layout;
            if (layout == null || !tilemap) return null;
            var boardSpace = KeyGeometry.BoardSpaceOf(tilemap);
            return KeyGeometry.KeyAt(layout, boardSpace.InverseTransformPoint(tilemap.GetCellCenterWorld(position)));
        }

        void SetKeys(GameObject brushTarget, IEnumerable<Vector3Int> positions, TileBase tile, string undoName)
        {
            var tilemap = brushTarget ? brushTarget.GetComponent<Tilemap>() : null;
            if (!tilemap) return;
            if (Layout == null)
            {
                Debug.LogWarning("Key brush: no keyboard layouts found (Project Settings > Detective Dave > Levels > Layouts Folder).");
                return;
            }
            var done = new HashSet<KeyDef>();
            var cellsToSet = new List<Vector3Int>();
            foreach (var position in positions)
            {
                var key = KeyUnder(tilemap, position);
                if (key == null || !done.Add(key)) continue;
                cellsToSet.AddRange(KeyGeometry.Cells(tilemap, KeyGeometry.BoardSpaceOf(tilemap), Layout, key));
            }
            if (cellsToSet.Count == 0) return;
            Undo.RegisterCompleteObjectUndo(tilemap, undoName);
            foreach (var cell in cellsToSet) tilemap.SetTile(cell, tile);
        }

        static IEnumerable<Vector3Int> Positions(BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin) yield return position;
        }
    }

    /// <summary>Shows the whole key a click would paint, and outlines the key under the cursor.</summary>
    [CustomEditor(typeof(KeyBrush))]
    public sealed class KeyBrushEditor : GridBrushEditor
    {
        static readonly Color OutlineColor = new Color(1f, 0.85f, 0.2f, 1f);

        Tilemap previewed;

        KeyBrush Brush => (KeyBrush)target;

        public override void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            var tilemap = brushTarget ? brushTarget.GetComponent<Tilemap>() : null;
            var tile = Brush.PickedTile;
            if (!tilemap || !tile)
            {
                base.PaintPreview(gridLayout, brushTarget, position);
                return;
            }
            ClearPreview();
            previewed = tilemap;
            foreach (var cell in Brush.KeyCells(tilemap, position)) tilemap.SetEditorPreviewTile(cell, tile);
        }

        public override void ClearPreview()
        {
            if (previewed) previewed.ClearAllEditorPreviewTiles();
            previewed = null;
            base.ClearPreview();
        }

        public override void OnPaintSceneGUI(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
        {
            base.OnPaintSceneGUI(gridLayout, brushTarget, position, tool, executing);
            var tilemap = brushTarget ? brushTarget.GetComponent<Tilemap>() : null;
            var layout = Brush.Layout;
            if (!tilemap || layout == null) return;
            var key = Brush.KeyUnder(tilemap, position.min);
            if (key == null) return;
            var boardSpace = KeyGeometry.BoardSpaceOf(tilemap);
            using (new Handles.DrawingScope(OutlineColor, boardSpace.localToWorldMatrix))
                foreach (var rect in KeyGeometry.Rects(layout, key))
                    Handles.DrawSolidRectangleWithOutline(rect, new Color(1f, 1f, 1f, 0.05f), OutlineColor);
        }
    }
}
