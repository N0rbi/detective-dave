using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DeskCaseColorsTests
    {
        [Test]
        public void TheSameCaseAlwaysGetsTheSameColour() => Assert.AreEqual(DeskCaseColors.Fill("casino"), DeskCaseColors.Fill("casino"));

        [Test]
        public void DifferentCasesGetDifferentColours() => Assert.AreNotEqual(DeskCaseColors.Fill("casino"), DeskCaseColors.Fill("missing money"));

        [Test]
        public void TheFillIsTranslucentAndTheOutlineIsSolid()
        {
            Assert.Less(DeskCaseColors.Fill("casino").a, 1f);
            Assert.AreEqual(1f, DeskCaseColors.Outline("casino").a);
        }

        [Test]
        public void AnEmptyIdStillGetsAColour() => Assert.DoesNotThrow(() => DeskCaseColors.Fill(string.Empty));
    }
}
