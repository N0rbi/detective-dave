using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DebounceSummaryTests
    {
        [Test]
        public void The_summary_names_the_window()
        {
            Assert.AreEqual("First note after a pause plays a chord; moves within 0.6 s play single notes.", DebounceSummary.For(0.6f));
        }

        [Test]
        public void The_summary_keeps_two_decimals()
        {
            StringAssert.Contains("1.25 s", DebounceSummary.For(1.25f));
        }

        [Test]
        public void A_window_of_zero_is_reported_as_off()
        {
            StringAssert.Contains("off", DebounceSummary.For(0f));
        }
    }
}
