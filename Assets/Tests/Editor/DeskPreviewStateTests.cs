using DetectiveDave.EditorTools;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DeskPreviewStateTests
    {
        static readonly string[] NoRequirements = new string[0];

        [TestCase(DeskPreviewMode.Open, CaseState.Open)]
        [TestCase(DeskPreviewMode.Locked, CaseState.Locked)]
        [TestCase(DeskPreviewMode.Solved, CaseState.Solved)]
        public void AFixedModeShowsEveryCaseTheSameWay(DeskPreviewMode mode, CaseState expected) =>
            Assert.AreEqual(expected, DeskPreviewState.Of(mode, "a", new[] { "b" }, null));

        [Test]
        public void TheProgressFileModeFollowsTheSolvedCases()
        {
            var store = new InMemoryProgressStore();
            store.Solved.Add("a");
            var progress = new CaseProgress(store);
            Assert.AreEqual(CaseState.Solved, DeskPreviewState.Of(DeskPreviewMode.ByProgressFile, "a", NoRequirements, progress));
            Assert.AreEqual(CaseState.Open, DeskPreviewState.Of(DeskPreviewMode.ByProgressFile, "b", new[] { "a" }, progress));
            Assert.AreEqual(CaseState.Locked, DeskPreviewState.Of(DeskPreviewMode.ByProgressFile, "c", new[] { "b" }, progress));
        }

        [Test]
        public void EveryModeHasAName() =>
            Assert.AreEqual(System.Enum.GetValues(typeof(DeskPreviewMode)).Length, DeskPreview.ModeNames.Length);
    }
}
