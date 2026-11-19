using DetectiveDave.EditorTools;
using DetectiveDave.KeyboardInput;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Tests
{
    /// <summary>
    /// The Key brush against a hand-made 15 x 4 layout (scale 1: one key unit is one board unit, a key is 4 x 4 cells of 0.25).
    /// Board space here is the Grid (no Level parent). Key positions in board space: x - 7.5, 2 - y.
    /// </summary>
    public class KeyBrushTests
    {
        const string LayoutJson = @"{
  ""id"": ""brush-test"", ""name"": ""Brush Test"", ""form"": ""iso"", ""digit"": 9, ""unitsWide"": 15, ""unitsHigh"": 4,
  ""keys"": [
    { ""code"": ""KeyA"", ""label"": ""A"", ""rects"": [ { ""x"": 0, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 0.5, ""y"": 0.5 } },
    { ""code"": ""KeyS"", ""label"": ""S"", ""rects"": [ { ""x"": 2, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 2.5, ""y"": 0.5 } },
    { ""code"": ""Enter"", ""label"": ""Enter"", ""rects"": [ { ""x"": 13.5, ""y"": 1, ""w"": 1.5, ""h"": 1 }, { ""x"": 13.75, ""y"": 2, ""w"": 1.25, ""h"": 1 } ],
      ""center"": { ""x"": 14.375, ""y"": 2 } },
    { ""code"": ""Slash"", ""label"": ""/"", ""rects"": [ { ""x"": 14, ""y"": 3, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 14.5, ""y"": 3.5 } }
  ]
}";

        GameObject root;
        Grid grid;
        Tilemap tilemap;
        KeyBrush brush;
        Tile tile;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Grid", typeof(Grid));
            grid = root.GetComponent<Grid>();
            grid.cellSize = new Vector3(0.25f, 0.25f, 0f);
            var shape = new GameObject("Shape", typeof(Tilemap));
            shape.transform.SetParent(root.transform, false);
            tilemap = shape.GetComponent<Tilemap>();
            tile = ScriptableObject.CreateInstance<Tile>();
            brush = ScriptableObject.CreateInstance<KeyBrush>();
            brush.LayoutOverride = KeyboardLayout.Parse(LayoutJson);
            brush.Init(Vector3Int.one);
            brush.SetTile(Vector3Int.zero, tile);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(tile);
            Object.DestroyImmediate(brush);
        }

        int PaintedCount()
        {
            tilemap.CompressBounds();
            int count = 0;
            foreach (var cell in tilemap.cellBounds.allPositionsWithin)
                if (tilemap.GetTile(cell)) count++;
            return count;
        }

        // Key A covers board x -7.5 .. -6.5, y 1 .. 2: cells x -30 .. -27, y 4 .. 7.
        [Test]
        public void Paint_FillsTheWholeKey()
        {
            brush.Paint(grid, tilemap.gameObject, new Vector3Int(-29, 5, 0));

            Assert.That(PaintedCount(), Is.EqualTo(16));
            Assert.That(tilemap.GetTile(new Vector3Int(-30, 4, 0)), Is.SameAs(tile), "lower-left cell");
            Assert.That(tilemap.GetTile(new Vector3Int(-27, 7, 0)), Is.SameAs(tile), "upper-right cell");
            Assert.That(tilemap.GetTile(new Vector3Int(-26, 5, 0)), Is.Null, "the cell right of the key");
            Assert.That(tilemap.GetTile(new Vector3Int(-29, 3, 0)), Is.Null, "the cell below the key");
        }

        [Test]
        public void Paint_LShapedEnter_FillsBothRectangles()
        {
            // Enter: x 6 .. 7.5, y 0 .. 1 (6 x 4 cells) and x 6.25 .. 7.5, y -1 .. 0 (5 x 4 cells). Click the lower part.
            brush.Paint(grid, tilemap.gameObject, new Vector3Int(27, -2, 0));

            Assert.That(PaintedCount(), Is.EqualTo(44));
            Assert.That(tilemap.GetTile(new Vector3Int(24, 3, 0)), Is.SameAs(tile), "upper part, left column");
            Assert.That(tilemap.GetTile(new Vector3Int(24, -1, 0)), Is.Null, "left of the lower part");
            Assert.That(tilemap.GetTile(new Vector3Int(25, -4, 0)), Is.SameAs(tile), "lower part, bottom-left cell");
        }

        [Test]
        public void Paint_EdgeKey_StaysInsideTheBoard()
        {
            // Slash, bottom-right corner key: x 6.5 .. 7.5, y -2 .. -1.
            brush.Paint(grid, tilemap.gameObject, new Vector3Int(29, -5, 0));

            Assert.That(PaintedCount(), Is.EqualTo(16));
            Assert.That(tilemap.GetTile(new Vector3Int(29, -8, 0)), Is.SameAs(tile));
            Assert.That(tilemap.GetTile(new Vector3Int(30, -8, 0)), Is.Null, "outside the board");
        }

        [Test]
        public void Paint_BetweenKeys_DoesNothing()
        {
            brush.Paint(grid, tilemap.gameObject, new Vector3Int(-25, 5, 0));     // board x -6.25: between A and S
            Assert.That(PaintedCount(), Is.Zero);
        }

        [Test]
        public void Erase_ClearsTheWholeKey()
        {
            brush.Paint(grid, tilemap.gameObject, new Vector3Int(-29, 5, 0));
            brush.Paint(grid, tilemap.gameObject, new Vector3Int(-21, 5, 0));    // S
            brush.Erase(grid, tilemap.gameObject, new Vector3Int(-27, 7, 0));    // any cell of A

            Assert.That(PaintedCount(), Is.EqualTo(16), "only S is left");
            Assert.That(tilemap.GetTile(new Vector3Int(-29, 5, 0)), Is.Null);
        }

        [Test]
        public void BoxFill_PaintsEveryKeyTheBoxTouches()
        {
            brush.BoxFill(grid, tilemap.gameObject, new BoundsInt(new Vector3Int(-28, 5, 0), new Vector3Int(8, 1, 1)));    // from A across to S
            Assert.That(PaintedCount(), Is.EqualTo(32));
        }

        [Test]
        public void KeyUnder_FindsTheKeyAndNothingBetweenKeys()
        {
            Assert.That(brush.KeyUnder(tilemap, new Vector3Int(-29, 5, 0)).Label, Is.EqualTo("A"));
            Assert.That(brush.KeyUnder(tilemap, new Vector3Int(-25, 5, 0)), Is.Null);
        }
    }
}
