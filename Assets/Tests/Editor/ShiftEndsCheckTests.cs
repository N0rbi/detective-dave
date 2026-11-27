using System.Collections.Generic;
using System.Linq;
using DetectiveDave.EditorTools;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Tests
{
    public class ShiftEndsCheckTests
    {
        const string LayoutJson = @"{
  ""id"": ""shift-test"", ""name"": ""Shift Test"", ""form"": ""iso"", ""digit"": 9, ""unitsWide"": 15, ""unitsHigh"": 4,
  ""keys"": [
    { ""code"": ""LeftShift"", ""label"": ""Shift"", ""rects"": [ { ""x"": 0, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 0.5, ""y"": 0.5 } },
    { ""code"": ""A"", ""label"": ""A"", ""rects"": [ { ""x"": 1, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 1.5, ""y"": 0.5 } },
    { ""code"": ""RightShift"", ""label"": ""Shift"", ""rects"": [ { ""x"": 2, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 2.5, ""y"": 0.5 } }
  ]
}";

        GameObject root;
        Level level;
        Tilemap shape;
        KeyboardLayout layout;
        TileType floor;
        TileType gap;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Level", typeof(Level));
            level = root.GetComponent<Level>();
            var grid = new GameObject("Grid", typeof(Grid));
            grid.GetComponent<Grid>().cellSize = new Vector3(0.25f, 0.25f, 0f);
            grid.transform.SetParent(root.transform, false);
            var tilemap = new GameObject("Shape", typeof(Tilemap));
            tilemap.transform.SetParent(grid.transform, false);
            shape = tilemap.GetComponent<Tilemap>();
            var serialized = new SerializedObject(level);
            serialized.FindProperty("shape").objectReferenceValue = shape;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            layout = KeyboardLayout.Parse(LayoutJson);
            floor = LevelSettings.instance.FloorType;
            gap = LevelSettings.instance.GapType;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void TileTypesAreConfigured()
        {
            Assert.IsNotNull(floor);
            Assert.IsNotNull(gap);
        }

        [Test]
        public void OpenPath_RaisesNothing()
        {
            PaintAll(floor, floor, floor);
            Assert.IsEmpty(Run());
        }

        [Test]
        public void GapOnAShift_WarnsThatTheGameIgnoresIt()
        {
            PaintAll(gap, floor, floor);
            Assert.IsTrue(Run().Any(issue => issue.Severity == MessageType.Warning && issue.Message.StartsWith("The Start key (Left Shift) is painted Gap.")));
        }

        [Test]
        public void UnpaintedShift_WarnsThatItIsDrawnEmpty()
        {
            PaintAll(floor, floor, null);
            Assert.IsTrue(Run().Any(issue => issue.Message == "The End key (Right Shift) is not painted, so it is drawn empty. Use Paint Floor in Prefab Mode."));
        }

        [Test]
        public void PaintFloor_LeavesTheShiftFloor()
        {
            PaintAll(gap, floor, floor);
            ShiftEndsCheck.PaintFloor(level, new[] { layout }, Key.LeftShift, floor);
            Assert.IsEmpty(Run());
        }

        [Test]
        public void GapBetweenTheShifts_IsAnError()
        {
            PaintAll(floor, gap, floor);
            Assert.IsTrue(Run().Any(issue => issue.Severity == MessageType.Error && issue.Message == "On the Shift Test keyboard there is no way from Left Shift (Start) to Right Shift (End): walls or gaps block it."));
        }

        List<LevelIssue> Run()
        {
            var issues = new List<LevelIssue>();
            new ShiftEndsCheck().Check(level, new[] { layout }, issues);
            return issues;
        }

        void PaintAll(TileBase left, TileBase middle, TileBase right)
        {
            Paint(Key.LeftShift, left);
            Paint(Key.A, middle);
            Paint(Key.RightShift, right);
        }

        void Paint(Key key, TileBase tile)
        {
            var def = layout.UniqueKeys().First(candidate => candidate.InputKey == key);
            foreach (var cell in KeyGeometry.Cells(shape, level.BoardSpace, layout, def)) shape.SetTile(cell, tile);
        }
    }
}
