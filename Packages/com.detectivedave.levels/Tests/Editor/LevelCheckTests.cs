using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Tests
{
    /// <summary>A game-style check found through TypeCache; it only throws for the level the test names, so it never disturbs real levels.</summary>
    internal sealed class ThrowingTestCheck : ILevelCheck
    {
        public const string LevelName = "Throws In Test 7f3a";

        public void Check(Level level, List<LevelIssue> issues)
        {
            if (level.name == LevelName) throw new InvalidOperationException("boom");
        }
    }

    public class LevelCheckTests
    {
        LevelTestUtility make;
        TileType floor;

        [SetUp]
        public void SetUp()
        {
            make = new LevelTestUtility();
            floor = make.Type("Floor");
        }

        [TearDown]
        public void TearDown() => make.Dispose();

        [Test]
        public void MissingKeyArtIsAnError()
        {
            var issues = LevelValidator.Validate(make.Level());
            Assert.IsTrue(issues.Any(issue => issue.Severity == MessageType.Error && issue.Message.StartsWith("No Key Art")));
        }

        [Test]
        public void MissingShapeOffersTheChildTilemap()
        {
            var level = make.Level(keyArt: make.KeyArt());
            var child = level.Shape;
            LevelTestUtility.Set(level, "shape", null);

            var issue = LevelValidator.Validate(level).Single(found => found.Message.StartsWith("No Shape tilemap"));
            issue.Fix();

            Assert.AreEqual(child, level.Shape);
        }

        [Test]
        public void TilesThatAreNotAKindOfKeyAreFlaggedAndErased()
        {
            var level = make.Level(keyArt: make.KeyArt());
            LevelTestUtility.Paint(level.Shape, make.PlainTile(), new RectInt(0, 0, 2, 1));
            LevelTestUtility.Paint(level.Shape, floor, new RectInt(3, 0, 1, 1));

            var issue = LevelValidator.Validate(level).Single(found => found.Message.Contains("not a kind of key"));
            StringAssert.StartsWith("2 painted cell", issue.Message);
            issue.Fix();

            Assert.IsNull(level.Shape.GetTile(new Vector3Int(0, 0, 0)));
            Assert.AreEqual(floor, level.Shape.GetTile(new Vector3Int(3, 0, 0)));
        }

        [Test]
        public void PaintedKindWithoutStyleIsFlagged()
        {
            var level = make.Level(keyArt: make.KeyArt("Art"));
            LevelTestUtility.Paint(level.Shape, floor, new RectInt(0, 0, 1, 1));

            Assert.IsTrue(LevelValidator.Validate(level).Any(issue => issue.Message.Contains("'Floor' is painted but 'Art' has no style")));
        }

        [Test]
        public void ACheckThatThrowsBecomesOneErrorAndTheOthersStillRun()
        {
            var level = make.Level(ThrowingTestCheck.LevelName);
            LogAssert.Expect(LogType.Exception, new Regex("boom"));

            var issues = LevelValidator.Validate(level);

            Assert.IsTrue(issues.Any(issue => issue.Severity == MessageType.Error && issue.Message.Contains(nameof(ThrowingTestCheck))));
            Assert.IsTrue(issues.Any(issue => issue.Message.StartsWith("No Key Art")), "the package checks still ran");
        }

        [Test]
        public void MergeKeepsTheHigherLayerAndLogsEachConflict()
        {
            var goal = make.Type("Goal");
            var level = make.Level();
            var grid = level.Shape.transform.parent;
            Tilemap Layer(string layerName)
            {
                var layer = new GameObject(layerName, typeof(Tilemap));
                layer.transform.SetParent(grid, false);
                return layer.GetComponent<Tilemap>();
            }
            var goals = Layer("Goals");
            var ground = Layer("Ground");
            LevelTestUtility.Paint(goals, make.PlainTile(), new RectInt(0, 0, 1, 1));
            LevelTestUtility.Paint(ground, make.PlainTile(), new RectInt(0, 0, 3, 1));
            var log = new List<string>();

            int conflicts = ShapeMerger.Merge(new[] { new ShapeMerger.Layer(goals, goal), new ShapeMerger.Layer(ground, floor) }, level.Shape, log.Add);

            Assert.AreEqual(1, conflicts);
            Assert.AreEqual(1, log.Count);
            StringAssert.Contains("kept Goal from 'Goals', dropped Floor from 'Ground'", log[0]);
            Assert.AreEqual(goal, level.Shape.GetTile(new Vector3Int(0, 0, 0)));
            Assert.AreEqual(floor, level.Shape.GetTile(new Vector3Int(2, 0, 0)));
        }
    }
}
