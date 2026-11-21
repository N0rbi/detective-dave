using System.Collections.Generic;
using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public static class CaseIds
    {
        const string CaseIdField = "caseId";

        public static bool StampIfEmpty(CaseFile file)
        {
            var serialized = new SerializedObject(file);
            var property = serialized.FindProperty(CaseIdField);
            if (!string.IsNullOrWhiteSpace(property.stringValue)) return false;
            property.stringValue = CaseIdRules.StoredOrAssetName(property.stringValue, file.name);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(file);
            return true;
        }

        public static int StampAll()
        {
            var stamped = AllCases().Count(pair => StampIfEmpty(pair.Value));
            if (stamped > 0) AssetDatabase.SaveAssets();
            return stamped;
        }

        public static IReadOnlyList<string> OtherPathsSharing(CaseFile file) =>
            CaseIdRules.OtherPathsWithId(file.Id, AssetDatabase.GetAssetPath(file), AllCases().ToDictionary(pair => pair.Key, pair => pair.Value.Id));

        static IEnumerable<KeyValuePair<string, CaseFile>> AllCases() =>
            AssetDatabase.FindAssets("l:" + CaseAssetLabeler.Label)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new KeyValuePair<string, CaseFile>(path, AssetDatabase.LoadAssetAtPath<CaseFile>(path)))
                .Where(pair => pair.Value);
    }
}
