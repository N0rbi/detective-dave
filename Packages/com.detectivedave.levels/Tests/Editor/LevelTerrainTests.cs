using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Levels.Tests
{
    public class LevelTerrainTests
    {
        LevelTestUtility make;
        TileType floor, wall;

        [SetUp]
        public void SetUp()
        {
            make = new LevelTestUtility();
            floor = make.Type("Floor");
            wall = make.Type("Wall", walkable: false);
        }

        [TearDown]
        public void TearDown() => make.Dispose();

        // A 5 x 3 field of floor at one unit per cell, origin (0, 0), with a wall column at x = 2 except at the top row.
        LevelTerrain Field() => new LevelTerrain(new RectInt(0, 0, 5, 3), Vector2.zero, Vector2.one,
            cell => cell.x == 2 && cell.y < 2 ? wall : floor);

        [Test]
        public void EmptyTerrainAnswersEmpty()
        {
            var terrain = new LevelTerrain();
            Assert.IsTrue(terrain.IsEmpty);
            Assert.IsTrue(terrain.GetTile(Vector2.zero).IsEmpty);
            Assert.IsEmpty(terrain.FindPath(Vector2.zero, Vector2.one));
        }

        [Test]
        public void CellsAndBoardPositionsRoundTrip()
        {
            var terrain = new LevelTerrain(new RectInt(-4, -2, 8, 4), new Vector2(-1f, -0.5f), new Vector2(0.25f, 0.25f), _ => floor);
            var cell = new Vector2Int(1, -1);
            Assert.AreEqual(cell, terrain.ToCell(terrain.CellCenter(cell)));
            Assert.AreEqual(new Vector2Int(-4, -2), terrain.ToCell(new Vector2(-1f, -0.5f)), "a border belongs to the cell above-right");
        }

        [Test]
        public void OutsideThePaintedAreaIsEmpty()
        {
            var terrain = Field();
            Assert.AreEqual(floor, terrain.GetTile(new Vector2(0.5f, 0.5f)).Type);
            Assert.IsTrue(terrain.GetTile(new Vector2(-0.5f, 0.5f)).IsEmpty);
            Assert.IsTrue(terrain.GetTile(new Vector2Int(5, 0)).IsEmpty);
        }

        [Test]
        public void TypesListsEachUsedTypeOnce()
        {
            CollectionAssert.AreEquivalent(new[] { floor, wall }, Field().Types);
        }

        [Test]
        public void PathGoesAroundWalls()
        {
            var path = Field().FindPath(new Vector2(0.5f, 0.5f), new Vector2(4.5f, 0.5f));
            Assert.IsNotEmpty(path);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), path[0]);
            Assert.AreEqual(new Vector2(4.5f, 0.5f), path[path.Count - 1]);
            foreach (var point in path) Assert.AreNotEqual(wall, Field().GetTile(point).Type);
        }

        [Test]
        public void CustomPassableRuleIsUsed()
        {
            var terrain = Field();
            Assert.IsEmpty(terrain.FindPath(new Vector2(0.5f, 0.5f), new Vector2(4.5f, 0.5f), tile => tile.Type == floor && tile.Cell.y < 2));
            Assert.IsNotEmpty(terrain.FindPath(new Vector2(0.5f, 0.5f), new Vector2(2.5f, 0.5f), tile => !tile.IsEmpty));
        }

        [Test]
        public void FromTilemapReadsTileTypesInBoardSpaceTrimmedToThePaintedCells()
        {
            var level = make.Level(gridPosition: new Vector3(1f, 0f, 0f));
            LevelTestUtility.Paint(level.Shape, floor, new RectInt(2, 0, 4, 4));

            var terrain = LevelTerrain.FromTilemap(level.Shape, level.BoardSpace);

            Assert.AreEqual(new RectInt(2, 0, 4, 4), terrain.CellBounds);
            Assert.AreEqual(16, terrain.PaintedCellCount);
            // Cell (2, 0) spans x 0.5..0.75 in grid space, so 1.5..1.75 on the board.
            var center = terrain.CellCenter(new Vector2Int(2, 0));
            Assert.AreEqual(1.625f, center.x, 1e-5f);
            Assert.AreEqual(0.125f, center.y, 1e-5f);
            Assert.AreEqual(floor, terrain.GetTile(new Vector2(1.6f, 0.1f)).Type);
            Assert.IsTrue(terrain.GetTile(new Vector2(1.4f, 0.1f)).IsEmpty);
        }

        [Test]
        public void FromTilemapSkipsTilesThatAreNotAKindOfKey()
        {
            var level = make.Level();
            LevelTestUtility.Paint(level.Shape, make.PlainTile(), new RectInt(0, 0, 2, 2));
            LevelTestUtility.Paint(level.Shape, wall, new RectInt(5, 0, 1, 1));

            var terrain = LevelTerrain.FromTilemap(level.Shape, level.BoardSpace);

            Assert.AreEqual(1, terrain.PaintedCellCount);
            CollectionAssert.AreEqual(new[] { wall }, terrain.Types);
            Assert.IsTrue(LevelTerrain.FromTilemap(make.Level("Empty").Shape, level.BoardSpace).IsEmpty);
        }

        [Test]
        public void BuildTerrainUsesTheParentAsBoardSpace()
        {
            var placed = make.Level();
            placed.transform.localPosition = new Vector3(3f, 0f, 0f);
            var alone = make.Level("Alone", underBoard: false);
            alone.transform.localPosition = new Vector3(3f, 0f, 0f);
            LevelTestUtility.Paint(placed.Shape, floor, new RectInt(0, 0, 1, 1));
            LevelTestUtility.Paint(alone.Shape, floor, new RectInt(0, 0, 1, 1));

            Assert.AreEqual(floor, placed.BuildTerrain().GetTile(new Vector2(3.1f, 0.1f)).Type, "under a board: the level's offset counts");
            Assert.AreEqual(floor, alone.BuildTerrain().GetTile(new Vector2(0.1f, 0.1f)).Type, "at the root: its own local space");
        }

        [Test]
        public void PathWalkerReachesTheEnd()
        {
            var walker = new PathWalker(new[] { Vector2.zero, Vector2.right, new Vector2(1f, 1f) });
            walker.Advance(1.5f);
            Assert.AreEqual(new Vector2(1f, 0.5f), walker.Position);
            Assert.IsFalse(walker.Done);
            walker.Advance(10f);
            Assert.IsTrue(walker.Done);
            Assert.AreEqual(new Vector2(1f, 1f), walker.Position);
        }
    }
}
