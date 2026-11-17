using System.Linq;
using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Tests
{
    public class LevelDefinitionTests
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
        public void SpriteLookupUsesASizeTolerance()
        {
            var level = make.Level();
            var sprite = make.Sprite("wall_2_25x1");
            make.AddStyle(level, wall, (Shift, false, sprite));

            Assert.AreEqual(sprite, level.FindSprite(wall, new Vector2(2.249f, 1.001f)));
            Assert.IsNull(level.FindSprite(wall, new Vector2(2.3f, 1f)));
        }

        [Test]
        public void WholeKeySpritesAreSeparateFromRectangleSprites()
        {
            var level = make.Level();
            var whole = make.Sprite("wall_enter_1_5x2");
            make.AddStyle(level, wall, (Enter, true, whole));

            Assert.AreEqual(whole, level.FindSprite(wall, Enter, wholeKey: true));
            Assert.IsNull(level.FindSprite(wall, Enter, wholeKey: false));
        }

        [Test]
        public void MissingArtComesFromTheTemplate()
        {
            var template = make.Level("Template");
            var templateLetter = make.Sprite("t_1x1");
            var templateShift = make.Sprite("t_2_25x1");
            make.AddStyle(template, wall, (Letter, false, templateLetter), (Shift, false, templateShift));
            make.SetBackground(template, make.Sprite("background"));

            var level = make.Level("Level");
            var ownLetter = make.Sprite("own_1x1");
            make.AddStyle(level, wall, (Letter, false, ownLetter));
            make.SetTemplate(level, template);

            Assert.AreEqual(ownLetter, level.FindSprite(wall, Letter), "own sprite wins");
            Assert.AreEqual(templateShift, level.FindSprite(wall, Shift), "a shape the level lacks comes from the template");
            Assert.AreEqual(template.Background, level.Background);
            Assert.AreSame(level.TileStyles[0], level.FindStyle(wall));
        }

        [Test]
        public void TemplateLoopsDoNotHang()
        {
            var a = make.Level("A");
            var b = make.Level("B");
            make.SetTemplate(a, b);
            make.SetTemplate(b, a);

            Assert.IsNull(a.FindSprite(wall, Letter));
            Assert.IsTrue(a.HasTemplateLoop());
            Assert.IsTrue(LevelValidator.Validate(a).Any(issue => issue.Severity == MessageType.Error));
        }

        [Test]
        public void DisplayNameFallsBackToTheAssetName()
        {
            Assert.AreEqual("Level 3", make.Level("Level 3").DisplayName);
        }

        [Test]
        public void ValidatorFlagsDuplicateStylesAndUnbakedShape()
        {
            var level = make.Level();
            make.AddStyle(level, wall, (Letter, false, make.Sprite("a")));
            make.AddStyle(level, wall, (Letter, false, make.Sprite("b")));

            var messages = LevelValidator.Validate(level).Select(issue => issue.Message).ToList();

            Assert.IsTrue(messages.Any(message => message.Contains("2 styles")));
            Assert.IsTrue(messages.Any(message => message.Contains("No shape yet")));
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
        public void SpriteNamesWithoutASizeAreSkipped()
        {
            Assert.IsFalse(KeyShapePresets.TryParseSpriteName("tile_wall", out _, out _));
        }
    }
}
