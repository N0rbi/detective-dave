using System.Collections.Generic;
using DetectiveDave.EditorTools;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class DeskLayoutChecksTests
    {
        static LayoutKeyCentres Layout(string name, params (Key key, Vector2 centre)[] keys)
        {
            var centres = new Dictionary<Key, Vector2>();
            foreach (var entry in keys) centres[entry.key] = entry.centre;
            return new LayoutKeyCentres(name, centres);
        }

        static DeskMarker Marker(string id, Vector2 centre) => new DeskMarker(id, centre, Vector2.one, 0f);

        [Test]
        public void LayoutsWithoutProblemsAreLeftOut()
        {
            var layouts = new[] { Layout("uk", (Key.LeftShift, Vector2.zero), (Key.A, new Vector2(1f, 0f))) };
            CollectionAssert.IsEmpty(DeskLayoutChecks.OnEveryLayout(new[] { Marker("one", new Vector2(1f, 0f)) }, layouts, Key.LeftShift));
        }

        [Test]
        public void AMarkerThatCoversNoKeyOnOneLayoutIsReportedForThatLayoutOnly()
        {
            var layouts = new[]
            {
                Layout("uk", (Key.LeftShift, Vector2.zero), (Key.A, new Vector2(1f, 0f))),
                Layout("hu", (Key.LeftShift, Vector2.zero), (Key.A, new Vector2(5f, 0f))),
            };
            var reports = DeskLayoutChecks.OnEveryLayout(new[] { Marker("one", new Vector2(1f, 0f)) }, layouts, Key.LeftShift);
            Assert.AreEqual(1, reports.Count);
            Assert.AreEqual("hu", reports[0].LayoutName);
            Assert.AreEqual(DeskIssueKind.CoversNoKey, reports[0].Issues[0].Kind);
        }

        [Test]
        public void ACaseOnTheStartKeyIsReportedOnEveryLayoutItCovers()
        {
            var layouts = new[] { Layout("uk", (Key.LeftShift, Vector2.zero)), Layout("us", (Key.LeftShift, Vector2.zero)) };
            Assert.AreEqual(2, DeskLayoutChecks.OnEveryLayout(new[] { Marker("one", Vector2.zero) }, layouts, Key.LeftShift).Count);
        }

        [Test]
        public void DescribeNamesTheCases()
        {
            StringAssert.Contains("one", DeskLayoutChecks.Describe(new DeskIssue(DeskIssueKind.CoversNoKey, "one", null, Key.None)));
            StringAssert.Contains("two", DeskLayoutChecks.Describe(new DeskIssue(DeskIssueKind.KeySharedByCases, "one", "two", Key.A)));
            StringAssert.Contains("LeftShift", DeskLayoutChecks.Describe(new DeskIssue(DeskIssueKind.CoversStartKey, "one", null, Key.LeftShift)));
        }
    }
}
