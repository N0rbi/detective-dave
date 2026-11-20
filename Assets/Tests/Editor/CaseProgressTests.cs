using System.Collections.Generic;
using System.Linq;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public sealed class InMemoryProgressStore : IProgressStore
    {
        public readonly List<string> Solved = new List<string>();
        public int Saves { get; private set; }

        public IReadOnlyCollection<string> LoadSolved() => Solved.ToArray();

        public void SaveSolved(IReadOnlyCollection<string> solvedIds)
        {
            Saves++;
            Solved.Clear();
            Solved.AddRange(solvedIds);
        }
    }

    public class CaseProgressTests
    {
        static readonly string[] NoRequirements = new string[0];

        InMemoryProgressStore store;
        CaseProgress progress;

        [SetUp]
        public void SetUp()
        {
            store = new InMemoryProgressStore();
            progress = new CaseProgress(store);
        }

        [Test]
        public void CaseWithoutRequirementsIsOpen() => Assert.AreEqual(CaseState.Open, progress.StateOf("a", NoRequirements));

        [Test]
        public void CaseWithAnUnsolvedRequirementIsLocked() => Assert.AreEqual(CaseState.Locked, progress.StateOf("b", new[] { "a" }));

        [Test]
        public void CaseOpensWhenEveryRequirementIsSolved()
        {
            progress.MarkSolved("a");
            Assert.AreEqual(CaseState.Locked, progress.StateOf("c", new[] { "a", "b" }));
            progress.MarkSolved("b");
            Assert.AreEqual(CaseState.Open, progress.StateOf("c", new[] { "a", "b" }));
        }

        [Test]
        public void SolvedWinsOverLocked()
        {
            progress.MarkSolved("b");
            Assert.AreEqual(CaseState.Solved, progress.StateOf("b", new[] { "a" }));
        }

        [Test]
        public void RequirementThatIsNotKnownKeepsTheCaseLocked() => Assert.AreEqual(CaseState.Locked, progress.StateOf("b", new[] { "missing" }));

        [Test]
        public void MarkingSolvedIsSaved()
        {
            progress.MarkSolved("a");
            CollectionAssert.AreEqual(new[] { "a" }, store.Solved);
        }

        [Test]
        public void MarkingTheSameCaseTwiceSavesOnce()
        {
            progress.MarkSolved("a");
            progress.MarkSolved("a");
            Assert.AreEqual(1, store.Saves);
        }

        [Test]
        public void SolvedCasesAreLoadedFromTheStore()
        {
            store.Solved.Add("a");
            Assert.IsTrue(new CaseProgress(store).IsSolved("a"));
        }

        [Test]
        public void ResetForgetsEverythingAndSaves()
        {
            progress.MarkSolved("a");
            progress.Reset();
            Assert.IsFalse(progress.IsSolved("a"));
            Assert.IsEmpty(store.Solved);
        }
    }
}
