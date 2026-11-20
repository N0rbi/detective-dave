using System.Linq;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public class ConfigAssetLabeler : AssetPostprocessor
    {
        public const string Label = "Config";
        public const string Suffix = ".config.asset";

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom) =>
            AssetLabeling.AddLabelLater(AssetLabeling.Unlabeled(imported.Concat(moved), Suffix, Label), Label);

        public static bool IsConfigPath(string path) => path.EndsWith(Suffix);
    }
}
