using System.Linq;
using DetectiveDave.EditorTools;
using NUnit.Framework;
using UnityEditor;

namespace DetectiveDave.Tests
{
    public class ConfigAssetTests
    {
        static readonly string[] ProjectFolders = { "Assets" };

        [Test]
        public void EveryConfigAssetCarriesTheConfigLabel()
        {
            var unlabeled = ConfigPaths().Where(path => !LabelsOf(path).Contains(ConfigAssetLabeler.Label)).ToArray();
            Assert.That(unlabeled, Is.Empty);
        }

        [Test]
        public void TheKnownConfigAssetsAreFound()
        {
            var names = ConfigPaths().Select(System.IO.Path.GetFileName).ToArray();
            Assert.That(names, Is.SupersetOf(new[]
            {
                "Player Movement Settings.config.asset",
                "Level Sequence.config.asset",
                "Default Key Art.config.asset",
                "KeyboardPalette.config.asset",
            }));
        }

        [Test]
        public void OnlyAssetFilesAreConfigPaths()
        {
            Assert.IsTrue(ConfigAssetLabeler.IsConfigPath("Assets/X/Thing.config.asset"));
            Assert.IsFalse(ConfigAssetLabeler.IsConfigPath("Assets/X/Thing.asset"));
        }

        static string[] ConfigPaths() =>
            AssetDatabase.FindAssets("t:ScriptableObject", ProjectFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(ConfigAssetLabeler.IsConfigPath)
                .ToArray();

        static string[] LabelsOf(string path) => AssetDatabase.GetLabels(AssetDatabase.LoadMainAssetAtPath(path));
    }
}
