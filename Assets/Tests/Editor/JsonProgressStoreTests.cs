using System.IO;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class JsonProgressStoreTests
    {
        string folder;
        string path;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "detective-dave-progress-" + System.Guid.NewGuid().ToString("N"));
            path = Path.Combine(folder, "progress.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }

        [Test]
        public void MissingFileMeansNothingSolved() => Assert.IsEmpty(new JsonProgressStore(path).LoadSolved());

        [Test]
        public void SavedCasesComeBack()
        {
            new JsonProgressStore(path).SaveSolved(new[] { "a", "b" });
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, new JsonProgressStore(path).LoadSolved());
        }

        [Test]
        public void SavingCreatesTheFolder()
        {
            new JsonProgressStore(path).SaveSolved(new string[0]);
            Assert.IsTrue(File.Exists(path));
        }

        [Test]
        public void SavingNothingClearsTheFile()
        {
            var store = new JsonProgressStore(path);
            store.SaveSolved(new[] { "a" });
            store.SaveSolved(new string[0]);
            Assert.IsEmpty(store.LoadSolved());
        }

        [Test]
        public void BrokenFileMeansNothingSolved()
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(path, "{ not json");
            Assert.IsEmpty(new JsonProgressStore(path).LoadSolved());
        }

        [Test]
        public void ProgressSurvivesAReloadThroughCaseProgress()
        {
            new CaseProgress(new JsonProgressStore(path)).MarkSolved("a");
            Assert.IsTrue(new CaseProgress(new JsonProgressStore(path)).IsSolved("a"));
        }
    }
}
