using System.Linq;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public class CaseAssetLabeler : AssetPostprocessor
    {
        public const string Label = "Case";
        public const string Suffix = ".case.asset";

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom) =>
            AssetLabeling.AddLabelLater(AssetLabeling.Unlabeled(imported.Concat(moved), Suffix, Label), Label);
    }
}
