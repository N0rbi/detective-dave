using System.Collections.Generic;
using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    public static class DeskFreeSpot
    {
        const float RowPrecision = 100f;

        public static Vector2 Find(IReadOnlyList<DeskMarker> existing, IReadOnlyDictionary<Key, Vector2> keyCentres, Key startKey, Vector2 size)
        {
            var taken = TakenKeys(existing, keyCentres);
            foreach (var candidate in InReadingOrder(keyCentres))
                if (candidate.Key != startKey && !taken.Contains(candidate.Key) && FitsFreeKeys(candidate.Value, size, keyCentres, taken, startKey))
                    return candidate.Value;
            return Average(keyCentres);
        }

        static HashSet<Key> TakenKeys(IReadOnlyList<DeskMarker> existing, IReadOnlyDictionary<Key, Vector2> keyCentres)
        {
            var map = new DeskKeyMap(existing, keyCentres);
            return new HashSet<Key>(keyCentres.Keys.Where(key => map.TryGetCase(key, out _)));
        }

        static IEnumerable<KeyValuePair<Key, Vector2>> InReadingOrder(IReadOnlyDictionary<Key, Vector2> keyCentres) =>
            keyCentres.OrderByDescending(entry => Mathf.Round(entry.Value.y * RowPrecision)).ThenBy(entry => entry.Value.x);

        static bool FitsFreeKeys(Vector2 centre, Vector2 size, IReadOnlyDictionary<Key, Vector2> keyCentres, HashSet<Key> taken, Key startKey)
        {
            var probe = new DeskMarker(string.Empty, centre, size, 0f);
            var covered = keyCentres.Where(entry => probe.Contains(entry.Value)).Select(entry => entry.Key).ToArray();
            return covered.Length > 0 && covered.All(key => key != startKey && !taken.Contains(key));
        }

        static Vector2 Average(IReadOnlyDictionary<Key, Vector2> keyCentres) =>
            keyCentres.Count == 0 ? Vector2.zero : keyCentres.Values.Aggregate(Vector2.zero, (sum, point) => sum + point) / keyCentres.Count;
    }
}
