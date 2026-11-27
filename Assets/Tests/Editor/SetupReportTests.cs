using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class SetupReportTests
    {
        [Test]
        public void NoChangesSaysNothingNeededChanging()
        {
            Assert.AreEqual("Nothing needed changing. Already there.", SetupReport.Summary(new string[0], "Already there."));
        }

        [Test]
        public void EachChangeIsListedOnItsOwnLine()
        {
            Assert.AreEqual("Set up:\n- made a\n- made b", SetupReport.Summary(new[] { "made a", "made b" }, "Already there."));
        }
    }
}
