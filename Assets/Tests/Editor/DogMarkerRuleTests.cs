using DetectiveDave.EditorTools;
using DetectiveDave.Levels;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class DogMarkerRuleTests
    {
        TileType gap;
        TileType floor;

        [SetUp]
        public void CreateTiles()
        {
            gap = ScriptableObject.CreateInstance<TileType>();
            floor = ScriptableObject.CreateInstance<TileType>();
        }

        [TearDown]
        public void DestroyTiles()
        {
            Object.DestroyImmediate(gap);
            Object.DestroyImmediate(floor);
        }

        [Test]
        public void A_gap_key_is_marked_while_markers_are_on()
        {
            Assert.IsTrue(DogMarkerRule.Marks(true, gap, gap));
        }

        [Test]
        public void No_key_is_marked_while_markers_are_off()
        {
            Assert.IsFalse(DogMarkerRule.Marks(false, gap, gap));
        }

        [Test]
        public void A_floor_key_and_an_empty_key_are_not_marked()
        {
            Assert.IsFalse(DogMarkerRule.Marks(true, floor, gap));
            Assert.IsFalse(DogMarkerRule.Marks(true, null, gap));
        }
    }
}
