using System.Linq;
using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Tests
{
    public class KeySpriteResolutionTests
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
        public void TheBoxDrawsEveryKeySizeSliced()
        {
            var art = make.KeyArt();
            var box = make.SpriteWithBorder("box", 8f);
            make.AddBoxStyle(art, wall, box);

            foreach (var size in new[] { Letter, Shift, new Vector2(6.25f, 1f), new Vector2(1f, 2f) })
            {
                var choice = art.Resolve(wall, size, wholeKey: false);
                Assert.AreEqual(KeySpriteKind.Sliced, choice.Kind, size.ToString());
                Assert.AreSame(box, choice.Sprite);
            }
        }

        [Test]
        public void TheEnterSpriteDrawsTheWholeEnterStretched()
        {
            var art = make.KeyArt();
            var enter = make.Sprite("enter");
            make.AddBoxStyle(art, wall, make.SpriteWithBorder("box", 8f), enter);

            var choice = art.Resolve(wall, Enter, wholeKey: true);

            Assert.AreEqual(KeySpriteKind.Stretched, choice.Kind);
            Assert.AreSame(enter, choice.Sprite);
        }

        [Test]
        public void WithoutAnEnterSpriteTheWholeKeyAsksForItsPartsInstead()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, make.SpriteWithBorder("box", 8f));

            Assert.AreEqual(KeySpriteKind.None, art.Resolve(wall, Enter, wholeKey: true).Kind);
            Assert.AreEqual(KeySpriteKind.Sliced, art.Resolve(wall, new Vector2(1.5f, 1f), wholeKey: false).Kind);
        }

        [Test]
        public void AShapeOverrideWinsOverTheBoxForItsSizeOnly()
        {
            var art = make.KeyArt();
            var overrideSprite = make.Sprite("wall_2_25x1");
            make.AddStyle(art, wall, (Shift, false, overrideSprite));
            var serialized = new SerializedObject(art);
            serialized.FindProperty("tileStyles").GetArrayElementAtIndex(0).FindPropertyRelative("box").objectReferenceValue = make.SpriteWithBorder("box", 8f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var shift = art.Resolve(wall, Shift, wholeKey: false);
            var letter = art.Resolve(wall, Letter, wholeKey: false);

            Assert.AreEqual(KeySpriteKind.Stretched, shift.Kind);
            Assert.AreSame(overrideSprite, shift.Sprite);
            Assert.AreEqual(KeySpriteKind.Sliced, letter.Kind);
        }

        [Test]
        public void AStyleWithOnlyShapeSpritesKeepsDrawingThemAsBefore()
        {
            var art = make.KeyArt();
            var sprite = make.Sprite("wall_1x1");
            make.AddStyle(art, wall, (Letter, false, sprite));

            var choice = art.Resolve(wall, Letter, wholeKey: false);

            Assert.AreEqual(KeySpriteKind.Stretched, choice.Kind);
            Assert.AreSame(sprite, choice.Sprite);
            Assert.AreEqual(KeySpriteKind.None, art.Resolve(wall, Shift, wholeKey: false).Kind);
        }

        [Test]
        public void ATypeWithoutAStyleDrawsNothing() =>
            Assert.AreEqual(KeySpriteKind.None, make.KeyArt().Resolve(wall, Letter, wholeKey: false).Kind);

        [Test]
        public void ShapeSpritesWithoutABoxCanMigrate()
        {
            var art = make.KeyArt();
            make.AddStyle(art, wall, (Letter, false, make.Sprite("a")));

            Assert.IsTrue(art.FindStyle(wall).CanMigrateToBox);
        }

        [Test]
        public void AStyleWithABoxDoesNotAskToMigrate()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, make.SpriteWithBorder("box", 8f));

            Assert.IsFalse(art.FindStyle(wall).CanMigrateToBox);
        }

        [Test]
        public void AnEmptyStyleDoesNotAskToMigrate()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, null);

            Assert.IsFalse(art.FindStyle(wall).CanMigrateToBox);
        }

        [Test]
        public void BorderThicknessStaysInItsRange()
        {
            var art = make.KeyArt();
            var serialized = new SerializedObject(art);
            serialized.FindProperty("borderThicknessInKeyUnits").floatValue = 5f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0.5f, art.BorderThicknessInKeyUnits);
        }

        [Test]
        public void NewKeyArtHasTheDefaultBorderThickness() => Assert.AreEqual(0.12f, make.KeyArt().BorderThicknessInKeyUnits, 1e-6f);

        [Test]
        public void ATypeWithNoBoxAndNoShapesIsWarned()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, null);

            var issues = LevelValidator.Validate(art);

            Assert.IsTrue(issues.Any(issue => issue.Severity == MessageType.Warning && issue.Message.Contains("no Box sprite")));
        }

        [Test]
        public void ABoxWithoutABorderIsWarnedAsNotSliced()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, make.SpriteWithBorder("flat_box", 0f), make.Sprite("enter"));

            var issues = LevelValidator.Validate(art);

            Assert.IsTrue(issues.Any(issue => issue.Severity == MessageType.Warning && issue.Message.Contains("no 9-slice border")));
        }

        [Test]
        public void ASlicedBoxAndAnEnterRaiseNoStyleIssue()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, make.SpriteWithBorder("box", 8f), make.Sprite("enter"));

            var issues = LevelValidator.Validate(art);

            Assert.IsFalse(issues.Any(issue => issue.Message.Contains("'Wall'") && (issue.Message.Contains("Box") || issue.Message.Contains("Enter"))));
        }

        [Test]
        public void ABoxWithoutAnEnterIsAnInfoNotAWarning()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, make.SpriteWithBorder("box", 8f));

            var enterIssues = LevelValidator.Validate(art).Where(issue => issue.Message.Contains("no Enter sprite")).ToList();

            Assert.AreEqual(1, enterIssues.Count);
            Assert.AreEqual(MessageType.Info, enterIssues[0].Severity);
        }

        [Test]
        public void ShapeSpritesWithoutABoxGetAMigrationInfo()
        {
            var art = make.KeyArt();
            make.AddStyle(art, wall, (Letter, false, make.Sprite("a")));

            var issues = LevelValidator.Validate(art);

            Assert.IsTrue(issues.Any(issue => issue.Severity == MessageType.Info && issue.Message.Contains("one sprite per size")));
        }

        [Test]
        public void AStandardShapeCoveredByTheBoxIsNotMissing()
        {
            var art = make.KeyArt();
            make.AddBoxStyle(art, wall, make.SpriteWithBorder("box", 8f));

            Assert.IsEmpty(LevelValidator.MissingStandardShapes(art, wall));
        }
    }
}
