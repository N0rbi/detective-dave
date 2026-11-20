using System.Collections.Generic;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class DeskKeyMapTests
    {
        static readonly Dictionary<Key, Vector2> Row = new Dictionary<Key, Vector2>
        {
            { Key.A, new Vector2(0f, 0f) },
            { Key.S, new Vector2(1f, 0f) },
            { Key.D, new Vector2(2f, 0f) },
        };

        static DeskKeyMap Map(IReadOnlyDictionary<Key, Vector2> keys, params DeskMarker[] markers) => new DeskKeyMap(markers, keys);

        static DeskMarker Marker(string id, Vector2 centre, Vector2 size, float rotation = 0f) => new DeskMarker(id, centre, size, rotation);

        [Test]
        public void KeysWithTheirCentreInsideTheMarkerBelongToIt()
        {
            var map = Map(Row, Marker("one", new Vector2(0.5f, 0f), new Vector2(2.2f, 1f)));
            CollectionAssert.AreEqual(new[] { Key.A, Key.S }, map.KeysOf("one"));
            Assert.IsFalse(map.TryGetCase(Key.D, out _));
        }

        [Test]
        public void TryGetCaseNamesTheCase()
        {
            var map = Map(Row, Marker("one", new Vector2(0f, 0f), new Vector2(1f, 1f)));
            Assert.IsTrue(map.TryGetCase(Key.A, out var caseId));
            Assert.AreEqual("one", caseId);
        }

        [Test]
        public void UnknownCaseHasNoKeys() => CollectionAssert.IsEmpty(Map(Row).KeysOf("none"));

        [Test]
        public void BiggerMarkerCoversMoreKeys()
        {
            var map = Map(Row, Marker("one", new Vector2(1f, 0f), new Vector2(4.2f, 1f)));
            Assert.AreEqual(3, map.KeysOf("one").Count);
        }

        [Test]
        public void MarkerRotatedANinetyDegreesCoversTheKeysAboveAndBelow()
        {
            var keys = new Dictionary<Key, Vector2> { { Key.A, new Vector2(0f, 1.5f) }, { Key.S, new Vector2(1.5f, 0f) } };
            var map = Map(keys, Marker("one", Vector2.zero, new Vector2(4f, 1f), 90f));
            CollectionAssert.AreEqual(new[] { Key.A }, map.KeysOf("one"));
        }

        [Test]
        public void MarkerRotatedFortyFiveDegreesCoversItsDiagonalCorner()
        {
            var keys = new Dictionary<Key, Vector2> { { Key.A, new Vector2(1.2f, 0f) }, { Key.S, new Vector2(1.5f, 0f) } };
            var map = Map(keys, Marker("one", Vector2.zero, new Vector2(2f, 2f), 45f));
            CollectionAssert.AreEqual(new[] { Key.A }, map.KeysOf("one"));
        }

        [Test]
        public void SameKeysWithoutRotationAreOutsideTheSameCorner()
        {
            var keys = new Dictionary<Key, Vector2> { { Key.A, new Vector2(1.2f, 0f) } };
            var map = Map(keys, Marker("one", Vector2.zero, new Vector2(2f, 2f), 0f));
            CollectionAssert.IsEmpty(map.KeysOf("one"));
        }

        [Test]
        public void KeyInTwoMarkersBelongsToTheFirstAndIsReported()
        {
            var map = Map(Row, Marker("one", new Vector2(0f, 0f), new Vector2(1f, 1f)), Marker("two", new Vector2(0f, 0f), new Vector2(1f, 1f)));
            Assert.IsTrue(map.TryGetCase(Key.A, out var caseId));
            Assert.AreEqual("one", caseId);
            var issue = map.Issues(Key.LeftShift)[0];
            Assert.AreEqual(DeskIssueKind.KeySharedByCases, issue.Kind);
            Assert.AreEqual("one", issue.CaseId);
            Assert.AreEqual("two", issue.OtherCaseId);
            Assert.AreEqual(Key.A, issue.Key);
        }

        [Test]
        public void MarkerOverNoKeyIsReported()
        {
            var map = Map(Row, Marker("lost", new Vector2(50f, 50f), new Vector2(1f, 1f)));
            var issues = map.Issues(Key.LeftShift);
            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual(DeskIssueKind.CoversNoKey, issues[0].Kind);
            Assert.AreEqual("lost", issues[0].CaseId);
        }

        [Test]
        public void MarkerOverTheStartKeyIsReported()
        {
            var keys = new Dictionary<Key, Vector2> { { Key.LeftShift, new Vector2(0f, 0f) } };
            var map = Map(keys, Marker("one", Vector2.zero, new Vector2(1f, 1f)));
            var issues = map.Issues(Key.LeftShift);
            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual(DeskIssueKind.CoversStartKey, issues[0].Kind);
            Assert.AreEqual("one", issues[0].CaseId);
        }

        [Test]
        public void CleanDeskHasNoIssues()
        {
            var map = Map(Row, Marker("one", new Vector2(0f, 0f), new Vector2(1f, 1f)), Marker("two", new Vector2(2f, 0f), new Vector2(1f, 1f)));
            CollectionAssert.IsEmpty(map.Issues(Key.LeftShift));
        }

        [Test]
        public void MarkerContainsItsEdge() =>
            Assert.IsTrue(Marker("one", Vector2.zero, new Vector2(2f, 2f)).Contains(new Vector2(1f, 1f)));

        [Test]
        public void StateOfACaseKeyComesFromTheResolver()
        {
            var cases = new DeskCases(Map(Row, Marker("one", new Vector2(0f, 0f), new Vector2(1f, 1f))), id => id == "one" ? CaseState.Solved : CaseState.Locked);
            Assert.IsTrue(cases.TryGetState(Key.A, out var state));
            Assert.AreEqual(CaseState.Solved, state);
        }

        [Test]
        public void KeyWithoutACaseHasNoState()
        {
            var cases = new DeskCases(Map(Row, Marker("one", new Vector2(0f, 0f), new Vector2(1f, 1f))), id => CaseState.Open);
            Assert.IsFalse(cases.TryGetState(Key.D, out _));
        }
    }
}
