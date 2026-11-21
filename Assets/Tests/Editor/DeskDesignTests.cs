using System.Collections.Generic;
using System.Linq;
using DetectiveDave.EditorTools;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class DeskDesignTests
    {
        static readonly Dictionary<Key, Vector2> Row = new Dictionary<Key, Vector2>
        {
            { Key.LeftShift, new Vector2(0f, 0f) },
            { Key.A, new Vector2(1f, 0f) },
            { Key.S, new Vector2(2f, 0f) },
        };

        static readonly Dictionary<Key, Rect[]> RowRects = Row.ToDictionary(entry => entry.Key, entry => new[] { new Rect(entry.Value - Vector2.one * 0.5f, Vector2.one) });

        static DeskMarker Marker(string id, Vector2 centre, Vector2 size) => new DeskMarker(id, centre, size, 0f);

        static DeskDesign Design(params DeskMarker[] markers) => new DeskDesign(markers, Row, RowRects, Key.LeftShift);

        [Test]
        public void CoveredKeysAreFilledForTheirCase()
        {
            var design = Design(Marker("one", new Vector2(1.5f, 0f), new Vector2(2.2f, 1f)));
            CollectionAssert.AreEqual(new[] { Key.A, Key.S }, design.Fills.Select(fill => fill.Key));
            Assert.IsTrue(design.Fills.All(fill => fill.CaseId == "one" && !fill.IsShared));
        }

        [Test]
        public void KeysCoveredByNoCaseHaveNoFill() => CollectionAssert.IsEmpty(Design().Fills);

        [Test]
        public void AKeyUnderTwoCasesIsMarkedShared()
        {
            var design = Design(Marker("one", new Vector2(1f, 0f), new Vector2(1f, 1f)), Marker("two", new Vector2(1f, 0f), new Vector2(1f, 1f)));
            Assert.IsTrue(design.Fills.Single().IsShared);
            Assert.IsTrue(design.Issues.Any(issue => issue.Kind == DeskIssueKind.KeySharedByCases));
        }

        [Test]
        public void AMarkerOverNoKeyIsReported()
        {
            var design = Design(Marker("far", new Vector2(10f, 10f), Vector2.one));
            Assert.AreEqual(DeskIssueKind.CoversNoKey, design.Issues.Single().Kind);
        }

        [Test]
        public void CoveringTheStartKeyIsReported()
        {
            var design = Design(Marker("one", new Vector2(0f, 0f), Vector2.one));
            Assert.IsTrue(design.Issues.Any(issue => issue.Kind == DeskIssueKind.CoversStartKey));
        }
    }
}
