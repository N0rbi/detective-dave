using System.Collections.Generic;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class CaseIdRulesTests
    {
        [Test]
        public void ANewIdIsNeverTheSameTwice() => Assert.AreNotEqual(CaseIdRules.NewId(), CaseIdRules.NewId());

        [Test]
        public void ANewIdStartsWithCase() => StringAssert.StartsWith("case-", CaseIdRules.NewId());

        [Test]
        public void AStoredIdWins() => Assert.AreEqual("kept", CaseIdRules.StoredOrAssetName("kept", "Renamed"));

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void AnEmptyStoredIdFallsBackToTheAssetName(string stored) => Assert.AreEqual("Jealous Wife", CaseIdRules.StoredOrAssetName(stored, "Jealous Wife"));

        [Test]
        public void OnlyOtherAssetsWithTheSameIdAreListed()
        {
            var ids = new Dictionary<string, string> { { "a.asset", "same" }, { "b.asset", "same" }, { "c.asset", "other" } };
            CollectionAssert.AreEqual(new[] { "b.asset" }, CaseIdRules.OtherPathsWithId("same", "a.asset", ids));
        }

        [Test]
        public void AnAssetAloneWithItsIdHasNoOthers()
        {
            var ids = new Dictionary<string, string> { { "a.asset", "same" } };
            CollectionAssert.IsEmpty(CaseIdRules.OtherPathsWithId("same", "a.asset", ids));
        }
    }
}
