using System.Collections.Generic;
using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    public readonly struct DeskKeyFill
    {
        public Key Key { get; }
        public IReadOnlyList<Rect> Rects { get; }
        public string CaseId { get; }
        public bool IsShared { get; }

        public DeskKeyFill(Key key, IReadOnlyList<Rect> rects, string caseId, bool isShared)
        {
            Key = key;
            Rects = rects;
            CaseId = caseId;
            IsShared = isShared;
        }
    }

    public sealed class DeskDesign
    {
        public IReadOnlyList<DeskKeyFill> Fills { get; }
        public IReadOnlyList<DeskIssue> Issues { get; }

        public DeskDesign(IReadOnlyList<DeskMarker> markers, IReadOnlyDictionary<Key, Vector2> keyCentres, IReadOnlyDictionary<Key, Rect[]> keyRects, Key startKey)
        {
            var map = new DeskKeyMap(markers, keyCentres);
            Issues = map.Issues(startKey);
            var sharedKeys = new HashSet<Key>(Issues.Where(issue => issue.Kind == DeskIssueKind.KeySharedByCases).Select(issue => issue.Key));
            Fills = keyCentres.Keys
                .OrderBy(key => (int)key)
                .Where(key => map.TryGetCase(key, out _))
                .Select(key => FillOf(map, key, keyRects, sharedKeys))
                .ToArray();
        }

        static DeskKeyFill FillOf(DeskKeyMap map, Key key, IReadOnlyDictionary<Key, Rect[]> keyRects, HashSet<Key> sharedKeys)
        {
            map.TryGetCase(key, out var caseId);
            return new DeskKeyFill(key, keyRects.TryGetValue(key, out var rects) ? rects : new Rect[0], caseId, sharedKeys.Contains(key));
        }
    }
}
