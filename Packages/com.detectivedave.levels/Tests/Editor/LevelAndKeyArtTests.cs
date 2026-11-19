using System.Linq;
using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Tests
{
    public class LevelAndKeyArtTests
    {
        static readonly Vector2 Letter = Vector2.one;
        static readonly Vector2 Shift = new Vector2(2.25f, 1f);
        static readonly Vector2 Enter = new Vector2(1.5f, 2f);

        LevelTestUtility make;
        TileType wall;

        [SetUp]
        public void SetUp()
        {
            make = new LevelTestUtility();
            wall = make.Type("Wall", walkable: false);
        }

        [TearDown]
        public void TearDown() => make.Dispose();

        [Test]
        public void DisplayNameFallsBackToTheFileNameWithoutClone()
        {
            Assert.AreEqual("Level 03", make.Level("Level 03").DisplayName);
            Assert.AreEqual("Level 03", make.Level("Level 03(Clone)").DisplayName);
        }

        [Test]
        public void BackgroundFallsBackToTheKeyArtDefault()
        {
            var art = make.KeyArt();
            var fallback = make.Sprite("default background");
            make.SetDefaultBackground(art, fallback);
            var level = make.Level(keyArt: art);

            Assert.AreEqual(fallback, level.Background);
            var own = make.Sprite("own background");
            level.BackgroundRenderer.sprite = own;
            Assert.AreEqual(own, level.Background);
        }

        [Test]
        public void SpriteLookupUsesASizeTolerance()
        {
            var art = make.KeyArt();
            var sprite = make.Sprite("wall_2_25x1");
            make.AddStyle(art, wall, (Shift, false, sprite));

            Assert.AreEqual(sprite, art.FindSprite(wall, new Vector2(2.249f, 1.001f)));
            Assert.IsNull(art.FindSprite(wall, new Vector2(2.3f, 1f)));
        }

        [Test]
        public void WholeKeySpritesAreSeparateFromRectangleSprites()
        {
            var art = make.KeyArt();
            var whole = make.Sprite("wall_enter_1_5x2");
            make.AddStyle(art, wall, (Enter, true, whole));

            Assert.AreEqual(whole, art.FindSprite(wall, Enter, wholeKey: true));
            Assert.IsNull(art.FindSprite(wall, Enter));
        }

        [Test]
        public void KeyArtCheckFlagsDuplicateStyles()
        {
            var art = make.KeyArt();
            make.AddStyle(art, wall, (Letter, false, make.Sprite("a")));
            make.AddStyle(art, wall, (Letter, false, make.Sprite("b")));

            Assert.IsTrue(LevelValidator.Validate(art).Any(issue => issue.Message.Contains("2 styles")));
            Assert.AreSame(art.TileStyles[0], art.FindStyle(wall), "the first style wins");
        }

        [TestCase("wall_1_25x1", 1.25f, 1f, false)]
        [TestCase("gap_2x1", 2f, 1f, false)]
        [TestCase("goal_enter_1_5x2", 1.5f, 2f, true)]
        public void SpriteNamesGiveTheirShape(string spriteName, float width, float height, bool wholeKey)
        {
            Assert.IsTrue(KeyShapePresets.TryParseSpriteName(spriteName, out var size, out var whole));
            Assert.AreEqual(new Vector2(width, height), size);
            Assert.AreEqual(wholeKey, whole);
        }

        [Test]
        public void SpriteNamesWithoutASizeAreSkipped() => Assert.IsFalse(KeyShapePresets.TryParseSpriteName("tile_wall", out _, out _));
    }
}
