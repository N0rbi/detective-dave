using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.EditorTools
{
    public static class CaseChainChecks
    {
        public static IReadOnlyList<CaseChainIssue> Run(IReadOnlyList<CaseNode> nodes) =>
            DuplicateIds(nodes)
                .Concat(CasesWithoutLevel(nodes))
                .Concat(RequiredCasesOffTheDesk(nodes))
                .Concat(CasesInACircle(nodes))
                .Concat(NoOpenCaseAtStart(nodes))
                .ToArray();

        public static string Describe(CaseChainIssue issue)
        {
            switch (issue.Kind)
            {
                case CaseChainIssueKind.CircularRequires: return $"{issue.CaseId} is part of a circle of required cases, so it can never open.";
                case CaseChainIssueKind.RequiredCaseNotOnDesk: return $"{issue.CaseId} requires {issue.OtherCaseId}, which is not on this desk, so it can never be solved here.";
                case CaseChainIssueKind.NoOpenCaseAtStart: return "Every case on this desk is locked at the start. At least one case must require nothing.";
                case CaseChainIssueKind.DuplicateId: return $"Two cases share the id {issue.CaseId}. Give one of them another id.";
                case CaseChainIssueKind.NoLevel: return $"{issue.CaseId} has no level yet. Players cannot accept it.";
                default: return issue.Kind.ToString();
            }
        }

        static IEnumerable<CaseChainIssue> DuplicateIds(IReadOnlyList<CaseNode> nodes) =>
            nodes.GroupBy(node => node.Id).Where(group => group.Count() > 1).Select(group => new CaseChainIssue(CaseChainIssueKind.DuplicateId, group.Key, null));

        static IEnumerable<CaseChainIssue> CasesWithoutLevel(IReadOnlyList<CaseNode> nodes) =>
            nodes.Where(node => node.IsOnDesk && !node.HasLevel).Select(node => new CaseChainIssue(CaseChainIssueKind.NoLevel, node.Id, null));

        static IEnumerable<CaseChainIssue> RequiredCasesOffTheDesk(IReadOnlyList<CaseNode> nodes)
        {
            var onDesk = new HashSet<string>(nodes.Where(node => node.IsOnDesk).Select(node => node.Id));
            return nodes
                .Where(node => node.IsOnDesk)
                .SelectMany(node => node.RequiredIds.Where(required => !onDesk.Contains(required)).Select(required => new CaseChainIssue(CaseChainIssueKind.RequiredCaseNotOnDesk, node.Id, required)));
        }

        static IEnumerable<CaseChainIssue> CasesInACircle(IReadOnlyList<CaseNode> nodes)
        {
            var requires = new Dictionary<string, IReadOnlyList<string>>();
            foreach (var node in nodes) requires[node.Id] = node.RequiredIds;
            return requires.Keys.Where(id => CanReachItself(id, requires)).Select(id => new CaseChainIssue(CaseChainIssueKind.CircularRequires, id, null));
        }

        static bool CanReachItself(string start, IReadOnlyDictionary<string, IReadOnlyList<string>> requires)
        {
            var seen = new HashSet<string>();
            var pending = new Stack<string>(requires[start]);
            while (pending.Count > 0)
            {
                var id = pending.Pop();
                if (id == start) return true;
                if (!seen.Add(id) || !requires.TryGetValue(id, out var next)) continue;
                foreach (var required in next) pending.Push(required);
            }
            return false;
        }

        static IEnumerable<CaseChainIssue> NoOpenCaseAtStart(IReadOnlyList<CaseNode> nodes)
        {
            var onDesk = nodes.Where(node => node.IsOnDesk).ToArray();
            if (onDesk.Length > 0 && onDesk.All(node => node.RequiredIds.Count > 0)) yield return new CaseChainIssue(CaseChainIssueKind.NoOpenCaseAtStart, null, null);
        }
    }
}
