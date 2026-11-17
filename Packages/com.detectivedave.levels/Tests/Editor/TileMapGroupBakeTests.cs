using System.Linq;
using System.Text.RegularExpressions;
using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DetectiveDave.Levels.Tests
{
    public class TileMapGroupBakeTests
    {
        LevelTestUtility make;
        TileType floor, wall, goal;

        [SetUp]
        public void SetUp()
        {
            make = new LevelTestUtility();
            floor = make.Type("Floor");
            wall = make.Type("Wall", walkable: false);
            goal = make.Type("Goal");
        }

        [TearDown]
        public void TearDown() => make.Dispose();

        [Test]
        public void FirstLayerWithATypedTileWins()
        {
            var group = make.Group(make.Level(), "Goal", "Ground");
            make.SetLayers(group, ("Goal", goal), ("Ground", floor));
            LevelTestUtility.Paint(make.Tilemap(group, "Ground"), make.PlainTile(), new RectInt(0, 0, 4, 4));
            LevelTestUtility.Paint(make.Tilemap(group, "Goal"), make.PlainTile(), new RectInt(0, 0, 1, 1));

            var terrain = LevelBaker.Bake(group);

            Assert.AreEqual(goal, terrain.GetTile(new Vector2Int(0, 0)).Type);
            Assert.AreEqual(floor, terrain.GetTile(new Vector2Int(1, 0)).Type);
        }

        [Test]
        public void UntypedTileFallsThroughToTheNextLayer()
        {
            var group = make.Group(make.Level(), "Decor", "Ground");
            make.SetLayers(group, ("Decor", null), ("Ground", null));
            LevelTestUtility.Paint(make.Tilemap(group, "Decor"), make.PlainTile(), new RectInt(0, 0, 1, 1));
            LevelTestUtility.Paint(make.Tilemap(group, "Ground"), make.Tile(wall), new RectInt(0, 0, 1, 1));

            Assert.AreEqual(wall, LevelBaker.Bake(group).GetTile(new Vector2Int(0, 0)).Type);
        }

        [Test]
        public void LayerTypeOverridesTheTilesOwnType()
        {
            var group = make.Group(make.Level(), "Walls");
            make.SetLayers(group, ("Walls", wall));
            LevelTestUtility.Paint(make.Tilemap(group, "Walls"), make.Tile(floor), new RectInt(0, 0, 1, 1));

            Assert.AreEqual(wall, LevelBaker.Bake(group).GetTile(new Vector2Int(0, 0)).Type);
        }

        [Test]
        public void NoLayersListedReadsEveryChildTilemap()
        {
            var group = make.Group(make.Level(), "A", "B");
            LevelTestUtility.Paint(make.Tilemap(group, "A"), make.Tile(goal), new RectInt(0, 0, 1, 1));
            LevelTestUtility.Paint(make.Tilemap(group, "B"), make.Tile(floor), new RectInt(0, 0, 2, 1));

            var terrain = LevelBaker.Bake(group);

            Assert.AreEqual(goal, terrain.GetTile(new Vector2Int(0, 0)).Type, "hierarchy order is priority order");
            Assert.AreEqual(floor, terrain.GetTile(new Vector2Int(1, 0)).Type);
        }

        [Test]
        public void TerrainIsInBoardSpaceAndTrimmedToThePaintedCells()
        {
            var group = make.Group(make.Level(), "Ground");
            group.transform.localPosition = new Vector3(1f, 0f, 0f);
            LevelTestUtility.Paint(make.Tilemap(group, "Ground"), make.Tile(floor), new RectInt(2, 0, 4, 4));

            var terrain = LevelBaker.Bake(group);

            Assert.AreEqual(new RectInt(2, 0, 4, 4), terrain.CellBounds);
            Assert.AreEqual(new Vector2(LevelTestUtility.CellSize, LevelTestUtility.CellSize), terrain.CellSize);
            // Cell (2, 0) spans x 0.5..0.75 in grid space, so 1.5..1.75 on the board.
            var center = terrain.CellCenter(new Vector2Int(2, 0));
            Assert.AreEqual(1.625f, center.x, 1e-5f);
            Assert.AreEqual(0.125f, center.y, 1e-5f);
            Assert.AreEqual(floor, terrain.GetTile(new Vector2(1.6f, 0.1f)).Type);
            Assert.IsTrue(terrain.GetTile(new Vector2(1.4f, 0.1f)).IsEmpty);
        }

        [Test]
        public void GroupsBakeIntoTheirOwnLevels()
        {
            var first = make.Group(make.Level("First"), "Ground");
            var second = make.Group(make.Level("Second"), "Ground");
            LevelTestUtility.Paint(make.Tilemap(first, "Ground"), make.Tile(floor), new RectInt(0, 0, 1, 1));
            LevelTestUtility.Paint(make.Tilemap(second, "Ground"), make.Tile(wall), new RectInt(0, 0, 1, 1));

            Assert.IsTrue(LevelBaker.BakeInto(first, recordUndo: false));
            Assert.IsTrue(LevelBaker.BakeInto(second, recordUndo: false));

            Assert.AreEqual(floor, first.Level.Terrain.GetTile(new Vector2Int(0, 0)).Type);
            Assert.AreEqual(wall, second.Level.Terrain.GetTile(new Vector2Int(0, 0)).Type);
        }

        [Test]
        public void TwoGroupsSharingALevelAreNotBaked()
        {
            var level = make.Level("Shared");
            var first = make.Group(level, "Ground");
            var second = make.Group(level, "Ground");
            var other = make.Group(make.Level("Own"), "Ground");

            LogAssert.Expect(LogType.Error, new Regex("Shared"));
            var safe = LevelBaker.GroupsSafeToBake(new[] { first, second, other }).ToList();

            CollectionAssert.AreEqual(new[] { other }, safe);
        }

        [Test]
        public void BakeReportsChangesOnlyWhenTheShapeChanged()
        {
            var group = make.Group(make.Level(), "Ground");
            var tilemap = make.Tilemap(group, "Ground");
            LevelTestUtility.Paint(tilemap, make.Tile(floor), new RectInt(0, 0, 2, 2));

            Assert.IsTrue(LevelBaker.BakeInto(group, recordUndo: false));
            Assert.IsTrue(LevelBaker.IsUpToDate(group));
            Assert.IsFalse(LevelBaker.BakeInto(group, recordUndo: false), "same shape: nothing to write");

            LevelTestUtility.Paint(tilemap, make.Tile(wall), new RectInt(0, 0, 1, 1));
            Assert.IsFalse(LevelBaker.IsUpToDate(group));
        }
    }
}
