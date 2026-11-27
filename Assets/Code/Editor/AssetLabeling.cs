using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public static class AssetLabeling
    {
        public static string[] Unlabeled(IEnumerable<string> paths, string suffix, string label) =>
            paths.Where(path => path.EndsWith(suffix) && LacksLabel(path, label)).ToArray();

        public static void AddLabelLater(string[] paths, string label)
        {
            if (paths.Length == 0) return;
            EditorApplication.delayCall += () => AddLabel(paths, label);
        }

        static bool LacksLabel(string path, string label)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            return asset && !AssetDatabase.GetLabels(asset).Contains(label);
        }

        static void AddLabel(string[] paths, string label)
        {
            foreach (var path in paths)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (!asset) continue;
                AssetDatabase.SetLabels(asset, AssetDatabase.GetLabels(asset).Append(label).ToArray());
            }
        }
    }
}
