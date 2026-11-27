using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    static class GuardSetup
    {
        public static string ApplyOrError()
        {
            var missing = MissingFrames(GuardSetupPaths.WalkFramePattern).Concat(MissingFrames(GuardSetupPaths.ChaseFramePattern)).ToArray();
            if (missing.Length > 0) return "Nothing was changed for the guards. Missing assets:\n" + string.Join("\n", missing);
            var settings = GuardSettingsAsset.EnsureWithSprites(
                PlayerSpriteImport.ImportFrames(GuardSetupPaths.WalkFramePattern),
                PlayerSpriteImport.ImportFrames(GuardSetupPaths.ChaseFramePattern));
            var error = GuardSceneWiring.Wire(settings);
            AssetDatabase.SaveAssets();
            return error;
        }

        static IEnumerable<string> MissingFrames(string pathPattern) =>
            Enumerable.Range(1, PlayerSetupPaths.FrameCount).Select(index => string.Format(pathPattern, index)).Where(path => !AssetImporter.GetAtPath(path));
    }
}
