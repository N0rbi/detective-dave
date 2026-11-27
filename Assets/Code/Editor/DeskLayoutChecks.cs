using System.Collections.Generic;
using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    public readonly struct LayoutKeyCentres
    {
        public string LayoutName { get; }
        public IReadOnlyDictionary<Key, Vector2> Centres { get; }

        public LayoutKeyCentres(string layoutName, IReadOnlyDictionary<Key, Vector2> centres)
        {
            LayoutName = layoutName;
            Centres = centres;
        }
    }

    public readonly struct LayoutIssues
    {
        public string LayoutName { get; }
        public IReadOnlyList<DeskIssue> Issues { get; }

        public LayoutIssues(string layoutName, IReadOnlyList<DeskIssue> issues)
        {
            LayoutName = layoutName;
            Issues = issues;
        }
    }

    public static class DeskLayoutChecks
    {
        public static IReadOnlyList<LayoutIssues> OnEveryLayout(IReadOnlyList<DeskMarker> markers, IEnumerable<LayoutKeyCentres> layouts, Key startKey) =>
            layouts
                .Select(layout => new LayoutIssues(layout.LayoutName, new DeskKeyMap(markers, layout.Centres).Issues(startKey)))
                .Where(report => report.Issues.Count > 0)
                .ToArray();

        public static string Describe(DeskIssue issue)
        {
            switch (issue.Kind)
            {
                case DeskIssueKind.CoversNoKey: return $"{issue.CaseId} covers no key.";
                case DeskIssueKind.KeySharedByCases: return $"{issue.CaseId} and {issue.OtherCaseId} both cover {issue.Key}.";
                case DeskIssueKind.CoversStartKey: return $"{issue.CaseId} covers the start key ({issue.Key}).";
                default: return issue.Kind.ToString();
            }
        }
    }
}
