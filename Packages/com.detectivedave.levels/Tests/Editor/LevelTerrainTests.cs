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
