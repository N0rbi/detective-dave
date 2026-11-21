using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.OverworldPlay
{
    public static class CaseIdRules
    {
        const string NewIdPrefix = "case-";
        const int NewIdLength = 8;

        public static string NewId() => NewIdPrefix + Guid.NewGuid().ToString("N").Substring(0, NewIdLength);

        public static string StoredOrAssetName(string storedId, string assetName) => string.IsNullOrWhiteSpace(storedId) ? assetName : storedId;

        public static IReadOnlyList<string> OtherPathsWithId(string id, string ownPath, IReadOnlyDictionary<string, string> idsByPath) =>
            idsByPath.Where(entry => entry.Value == id && entry.Key != ownPath).Select(entry => entry.Key).ToArray();
    }
}
