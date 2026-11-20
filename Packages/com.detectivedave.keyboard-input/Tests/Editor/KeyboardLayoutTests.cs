using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput.Tests
{
    public class KeyboardLayoutTests
    {
        [Test]
        public void Parse_ReadsEveryField()
        {
            var layout = TestLayouts.Load();

            Assert.That(layout.Id, Is.EqualTo("test"));
            Assert.That(layout.Name, Is.EqualTo("Test"));
            Assert.That(layout.Form, Is.EqualTo("iso"));
            Assert.That(layout.Digit, Is.EqualTo(3));
            Assert.That(layout.Size, Is.EqualTo(new Vector2(7f, 3f)));
            Assert.That(layout.Keys.Count, Is.EqualTo(9));

            var enter = layout.Keys[2];
            Assert.That(enter.Code, Is.EqualTo("Enter"));
            Assert.That(enter.InputKey, Is.EqualTo(Key.Enter));
            Assert.That(enter.Rects.Count, Is.EqualTo(2));
            Assert.That(enter.Rects[1].X, Is.EqualTo(3f));
            Assert.That(enter.Rects[1].Y, Is.EqualTo(1f));
            Assert.That(enter.Rects[0].Width, Is.EqualTo(2f));
            Assert.That(enter.Rects[0].Height, Is.EqualTo(1f));
            Assert.That(enter.Center, Is.EqualTo(new Vector2(3.5f, 0.5f)));
            Assert.That(enter.AlsoCodes, Is.EqualTo(new[] { "NumpadEnter", "NotAKey" }));

            var inert = layout.Keys[4];
            Assert.That(inert.IsInert, Is.True);
            Assert.That(inert.IsUnverified, Is.True);
            Assert.That(inert.Note, Is.EqualTo("ISO only"));
        }

        [Test]
        public void AlsoKeys_SkipsUnknownNames()
        {
            var enter = TestLayouts.Load().Keys[2];
            Assert.That(enter.AlsoKeys, Is.EqualTo(new[] { Key.NumpadEnter }));
        }

        [Test]
        public void InputKey_IsNullForAnUnknownName()
        {
            Assert.That(TestLayouts.Load().Keys[8].InputKey, Is.Null);
        }

        [Test]
        public void UniqueKeys_DropsInvalidCodesAndKeepsTheFirstOfADuplicate()
        {
            var unique = TestLayouts.Load().UniqueKeys().ToArray();

            Assert.That(unique.Select(def => def.InputKey.Value),
                Is.EqualTo(new[] { Key.A, Key.S, Key.Enter, Key.CapsLock, Key.Backquote, Key.Z, Key.OEM1 }));
            Assert.That(unique[0].Label, Is.EqualTo("A"));
        }

        [Test]
        public void MissingArrays_ReadAsEmpty()
        {
            var layout = KeyboardLayout.Parse(@"{ ""id"": ""empty"", ""keys"": [ { ""code"": ""A"" } ] }");
            Assert.That(layout.Keys[0].Rects, Is.Empty);
            Assert.That(layout.Keys[0].AlsoKeys, Is.Empty);
        }

        [Test]
        public void Parse_RejectsEmptyText()
        {
            Assert.Throws<ArgumentException>(() => KeyboardLayout.Parse(""));
            Assert.Throws<ArgumentException>(() => KeyboardLayout.Parse((string)null));
            Assert.Throws<ArgumentNullException>(() => KeyboardLayout.Parse((TextAsset)null));
        }
    }

    public class KeyGraphTests
    {
        KeyGraph graph;

        [SetUp]
        public void SetUp() => graph = new KeyGraph(TestLayouts.Load());

        [TestCase(Key.A, Key.S)]                // side by side
        [TestCase(Key.A, Key.CapsLock)]         // one above the other
        [TestCase(Key.S, Key.CapsLock)]         // a quarter unit of shared border is enough
        [TestCase(Key.S, Key.Backquote)]    // inert keys are still on the board
        [TestCase(Key.Enter, Key.Backquote)]
        public void KeysSharingABorder_AreNeighbours(Key key, Key other)
        {
            Assert.That(graph.AreNeighbours(key, other), Is.True);
            Assert.That(graph.AreNeighbours(other, key), Is.True, "symmetric");
        }

        [TestCase(Key.A, Key.Enter)]            // apart
        [TestCase(Key.Enter, Key.Z)]            // only a corner
        [TestCase(Key.A, Key.A)]                // not its own neighbour
        [TestCase(Key.A, Key.X)]                // not in the layout
        public void OtherKeys_AreNotNeighbours(Key key, Key other)
        {
            Assert.That(graph.AreNeighbours(key, other), Is.False);
        }

        [Test]
        public void Of_ListsAllNeighbours()
        {
            Assert.That(graph.Of(Key.S), Is.EquivalentTo(new[] { Key.A, Key.Enter, Key.CapsLock, Key.Backquote }));
        }

        [Test]
        public void Of_IsEmptyForAKeyNotInTheLayout()
        {
            Assert.That(graph.Of(Key.X), Is.Empty);
        }
    }
}
