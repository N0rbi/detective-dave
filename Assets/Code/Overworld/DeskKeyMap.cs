using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DeskKeyMap
    {
        static readonly IReadOnlyList<Key> NoKeys = new Key[0];

        readonly Dictionary<Key, string> caseByKey = new Dictionary<Key, string>();
        readonly Dictionary<string, List<Key>> keysByCase = new Dictionary<string, List<Key>>();
        readonly List<DeskIssue> sharedKeyIssues = new List<DeskIssue>();
        readonly string[] markerCaseIds;

        public DeskKeyMap(IReadOnlyList<DeskMarker> markers, IReadOnlyDictionary<Key, Vector2> keyCentres)
        {
            markerCaseIds = markers.Select(marker => marker.CaseId).ToArray();
            foreach (var key in keyCentres.Keys.OrderBy(key => (int)key)) Assign(key, OwnersOf(markers, keyCentres[key]));
        }

        public bool TryGetCase(Key key, out string caseId) => caseByKey.TryGetValue(key, out caseId);

        public IReadOnlyList<Key> KeysOf(string caseId) => keysByCase.TryGetValue(caseId, out var keys) ? keys : NoKeys;

        public IReadOnlyList<DeskIssue> Issues(Key startKey)
        {
            var issues = new List<DeskIssue>(sharedKeyIssues);
            issues.AddRange(markerCaseIds.Where(caseId => KeysOf(caseId).Count == 0).Select(caseId => new DeskIssue(DeskIssueKind.CoversNoKey, caseId, null, Key.None)));
            if (caseByKey.TryGetValue(startKey, out var coveringCase)) issues.Add(new DeskIssue(DeskIssueKind.CoversStartKey, coveringCase, null, startKey));
            return issues;
        }

        static string[] OwnersOf(IReadOnlyList<DeskMarker> markers, Vector2 centre) =>
            markers.Where(marker => marker.Contains(centre)).Select(marker => marker.CaseId).ToArray();

        void Assign(Key key, string[] owners)
        {
            if (owners.Length == 0) return;
            caseByKey[key] = owners[0];
            if (!keysByCase.TryGetValue(owners[0], out var keys)) keysByCase[owners[0]] = keys = new List<Key>();
            keys.Add(key);
            for (int index = 1; index < owners.Length; index++) sharedKeyIssues.Add(new DeskIssue(DeskIssueKind.KeySharedByCases, owners[0], owners[index], key));
        }
    }
}
