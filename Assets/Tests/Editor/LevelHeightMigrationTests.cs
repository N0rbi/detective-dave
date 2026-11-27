using DetectiveDave.EditorTools;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Tests
{
    public class LevelHeightMigrationTests
    {
        GameObject root;
        Tilemap tilemap;
        Tile tile;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Grid", typeof(Grid));
            root.GetComponent<Grid>().cellSize = new Vector3(0.25f, 0.25f, 0f);
            var shape = new GameObject("Shape", typeof(Tilemap));
            shape.transform.SetParent(root.transform, false);
            tilemap = shape.GetComponent<Tilemap>();
            tile = ScriptableObject.CreateInstance<Tile>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void DoublingStretchGivesEveryCellTwoRows()
        {
            tilemap.SetTile(new Vector3Int(3, 0, 0), tile);
            tilemap.SetTile(new Vector3Int(3, -1, 0), tile);

            LevelHeightMigration.StretchTilemap(tilemap, root.transform, 2f);

            foreach (var y in new[] { -2, -1, 0, 1 }) Assert.That(tilemap.GetTile(new Vector3Int(3, y, 0)), Is.SameAs(tile), $"row {y}");
            Assert.That(tilemap.GetTile(new Vector3Int(3, 2, 0)), Is.Null);
            Assert.That(tilemap.GetTile(new Vector3Int(3, -3, 0)), Is.Null);
            Assert.That(tilemap.GetTile(new Vector3Int(2, 0, 0)), Is.Null, "the width does not change");
        }

        [Test]
        public void StretchKeepsTheKindsApart()
        {
            var other = ScriptableObject.CreateInstance<Tile>();
            tilemap.SetTile(new Vector3Int(0, 0, 0), tile);
            tilemap.SetTile(new Vector3Int(0, 1, 0), other);

            LevelHeightMigration.StretchTilemap(tilemap, root.transform, 2f);

            Assert.That(tilemap.GetTile(new Vector3Int(0, 1, 0)), Is.SameAs(tile));
            Assert.That(tilemap.GetTile(new Vector3Int(0, 2, 0)), Is.SameAs(other));
            Assert.That(tilemap.GetTile(new Vector3Int(0, 3, 0)), Is.SameAs(other));
            Object.DestroyImmediate(other);
        }

        [Test]
        public void EmptyTilemapStaysEmpty()
        {
            LevelHeightMigration.StretchTilemap(tilemap, root.transform, 2f);
            Assert.That(tilemap.GetUsedTilesCount(), Is.Zero);
        }
    }
}
