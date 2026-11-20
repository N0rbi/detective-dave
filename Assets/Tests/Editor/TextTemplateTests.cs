using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class TextTemplateTests
    {
        static readonly SuspectEntry[] NoSuspects = new SuspectEntry[0];

        [Test]
        public void ClientReplacesEveryOccurrence() =>
            Assert.AreEqual("Ann and Ann", TextTemplate.Resolve("{client} and {client}", "Ann", NoSuspects));

        [Test]
        public void SuspectKeyIsReplacedByItsValue() =>
            Assert.AreEqual("Ann doubts Bob", TextTemplate.Resolve("{client} doubts {husband}", "Ann", new[] { new SuspectEntry("husband", "Bob") }));

        [Test]
        public void UnknownPlaceholderStaysVisible() =>
            Assert.AreEqual("Ann {nobody}", TextTemplate.Resolve("{client} {nobody}", "Ann", NoSuspects));

        [Test]
        public void NullSuspectsAreAllowed() =>
            Assert.AreEqual("Ann", TextTemplate.Resolve("{client}", "Ann", null));

        [Test]
        public void NullClientBecomesEmpty() =>
            Assert.AreEqual("Dear ,", TextTemplate.Resolve("Dear {client},", null, NoSuspects));

        [TestCase(null)]
        [TestCase("")]
        public void EmptyTextStaysEmpty(string text) =>
            Assert.AreEqual(string.Empty, TextTemplate.Resolve(text, "Ann", NoSuspects));

        [Test]
        public void SuspectWithEmptyKeyIsSkipped() =>
            Assert.AreEqual("a{}b", TextTemplate.Resolve("a{}b", "Ann", new[] { new SuspectEntry("", "x") }));

        [Test]
        public void UnresolvedPlaceholdersAreListedOnce() =>
            CollectionAssert.AreEqual(new[] { "{x}", "{y}" }, TextTemplate.UnresolvedPlaceholders("{x} {y} {x}"));

        [Test]
        public void BracesAroundSpacesAreNotPlaceholders() =>
            CollectionAssert.IsEmpty(TextTemplate.UnresolvedPlaceholders("{ not one } and {}"));

        [Test]
        public void NothingIsUnresolvedAfterResolving() =>
            CollectionAssert.IsEmpty(TextTemplate.UnresolvedPlaceholders(TextTemplate.Resolve("{client}", "Ann", NoSuspects)));
    }
}
