using System.Linq;
using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class CaseChainChecksTests
    {
        static CaseNode Node(string id, bool onDesk = true, bool hasLevel = true, params string[] requires) => new CaseNode(id, requires, onDesk, hasLevel);

        static CaseChainIssueKind[] Kinds(params CaseNode[] nodes) => CaseChainChecks.Run(nodes).Select(issue => issue.Kind).ToArray();

        [Test]
        public void ASimpleChainHasNoProblems()
        {
            CollectionAssert.IsEmpty(Kinds(Node("one"), Node("two", true, true, "one")));
        }

        [Test]
        public void CasesThatRequireEachOtherAreACircle()
        {
            var issues = CaseChainChecks.Run(new[] { Node("one", true, true, "two"), Node("two", true, true, "one"), Node("free") });
            CollectionAssert.AreEquivalent(new[] { "one", "two" }, issues.Where(issue => issue.Kind == CaseChainIssueKind.CircularRequires).Select(issue => issue.CaseId));
        }

        [Test]
        public void ACaseThatRequiresItselfIsACircle()
        {
            CollectionAssert.Contains(Kinds(Node("one", true, true, "one"), Node("free")), CaseChainIssueKind.CircularRequires);
        }

        [Test]
        public void ARequiredCaseThatIsNotOnTheDeskIsReported()
        {
            var issues = CaseChainChecks.Run(new[] { Node("one"), Node("two", true, true, "away"), Node("away", false) });
            var issue = issues.Single(found => found.Kind == CaseChainIssueKind.RequiredCaseNotOnDesk);
            Assert.AreEqual("two", issue.CaseId);
            Assert.AreEqual("away", issue.OtherCaseId);
        }

        [Test]
        public void ADeskWhereEveryCaseIsLockedHasNoOpenCase()
        {
            CollectionAssert.Contains(Kinds(Node("one", true, true, "two"), Node("two", true, true, "one")), CaseChainIssueKind.NoOpenCaseAtStart);
        }

        [Test]
        public void AnEmptyDeskIsNotReportedAsAllLocked()
        {
            CollectionAssert.IsEmpty(Kinds());
        }

        [Test]
        public void TwoCasesWithTheSameIdAreReportedOnce()
        {
            var issues = CaseChainChecks.Run(new[] { Node("same"), Node("same"), Node("other") });
            Assert.AreEqual(1, issues.Count(issue => issue.Kind == CaseChainIssueKind.DuplicateId));
        }

        [Test]
        public void ACaseOnTheDeskWithoutALevelIsReported()
        {
            var issue = CaseChainChecks.Run(new[] { Node("one", true, false) }).Single();
            Assert.AreEqual(CaseChainIssueKind.NoLevel, issue.Kind);
            Assert.AreEqual("one", issue.CaseId);
        }

        [Test]
        public void ACaseOffTheDeskWithoutALevelIsNotReported()
        {
            CollectionAssert.IsEmpty(Kinds(Node("one"), Node("away", false, false)));
        }

        [Test]
        public void EveryIssueKindHasAReadableDescription()
        {
            foreach (CaseChainIssueKind kind in System.Enum.GetValues(typeof(CaseChainIssueKind)))
                StringAssert.DoesNotContain(kind.ToString(), CaseChainChecks.Describe(new CaseChainIssue(kind, "a", "b")));
        }
    }
}
