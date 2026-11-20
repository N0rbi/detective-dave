using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class LevelCellClassifierTests
    {
        TileType gap;
        TileType gutter;
        TileType unmappedWalkable;
        TileType unmappedBlocked;

        [SetUp]
        public void SetUp()
        {
            gap = NewType(false);
            gutter = NewType(false);
            unmappedWalkable = NewType(true);
            unmappedBlocked = NewType(false);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var type in new[] { gap, gutter, unmappedWalkable, unmappedBlocked }) Object.DestroyImmediate(type);
        }

        [Test]
        public void EmptyCellIsWall() => Assert.AreEqual(CellKind.Wall, Classify(default));

        [Test]
        public void MappedTypesKeepTheirKind()
        {
            Assert.AreEqual(CellKind.Gap, Classify(Tile(gap)));
            Assert.AreEqual(CellKind.Gutter, Classify(Tile(gutter)));
        }

        [Test]
        public void UnmappedWalkableIsFloor() => Assert.AreEqual(CellKind.Floor, Classify(Tile(unmappedWalkable)));

        [Test]
        public void UnmappedUnwalkableIsWall() => Assert.AreEqual(CellKind.Wall, Classify(Tile(unmappedBlocked)));

        CellKind Classify(MapTile tile) => LevelCellClassifier.Classify(tile, gap, gutter);

        static MapTile Tile(TileType type) => new MapTile(type, Vector2Int.zero);

        static TileType NewType(bool walkable)
        {
            var type = ScriptableObject.CreateInstance<TileType>();
            var serialized = new SerializedObject(type);
            serialized.FindProperty("walkable").boolValue = walkable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return type;
        }
    }
}
