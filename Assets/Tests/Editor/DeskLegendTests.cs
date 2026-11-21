using System.Linq;
using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DeskLegendTests
    {
        [Test]
        public void TheLegendNamesFreeKeysCasesOverlapsAndStart() => Assert.AreEqual(4, DeskLegend.Entries().Count);

        [Test]
        public void EveryEntryHasADistinctLabel()
        {
            var labels = DeskLegend.Entries().Select(entry => entry.Label).ToArray();
            Assert.IsTrue(labels.All(label => !string.IsNullOrWhiteSpace(label)));
            Assert.AreEqual(labels.Length, labels.Distinct().Count());
        }

        [Test]
        public void TheLegendUsesTheColoursTheSceneViewDraws()
        {
            var colours = DeskLegend.Entries().Select(entry => entry.Colour).ToArray();
            CollectionAssert.Contains(colours, DeskCaseColors.FreeKeyFill);
            CollectionAssert.Contains(colours, DeskCaseColors.SharedKey);
            CollectionAssert.Contains(colours, DeskCaseColors.StartKey);
        }

        [Test]
        public void AFreeKeyIsTranslucentButClearlyVisible()
        {
            Assert.Less(DeskCaseColors.FreeKeyFill.a, 1f);
            Assert.GreaterOrEqual(DeskCaseColors.FreeKeyFill.a, 0.2f);
        }

        [Test]
        public void ACoveredKeyLooksDifferentFromAFreeKey() =>
            Assert.AreNotEqual(DeskCaseColors.FreeKeyFill, DeskCaseColors.Fill(DeskLegend.CaseSampleId));
    }
}
