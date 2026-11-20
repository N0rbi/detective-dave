using System.Linq;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public class ConfigAssetLabeler : AssetPostprocessor
    {
        public const string Label = "Config";
        public const string Suffix = ".config.asset";

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            var unlabeled = imported.Concat(moved).Where(IsUnlabeledConfig).ToArray();
            if (unlabeled.Length == 0) return;
            EditorApplication.delayCall += () => AddLabel(unlabeled);
        }

        public static bool IsConfigPath(string path) => path.EndsWith(Suffix);

        static bool IsUnlabeledConfig(string path)
        {
            if (!IsConfigPath(path)) return false;
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            return asset && !AssetDatabase.GetLabels(asset).Contains(Label);
        }

        static void AddLabel(string[] paths)
        {
            foreach (var path in paths)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (!asset) continue;
                AssetDatabase.SetLabels(asset, AssetDatabase.GetLabels(asset).Append(Label).ToArray());
            }
        }
    }
}
