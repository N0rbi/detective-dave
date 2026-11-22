using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class SingleNoteReportTests
    {
        [Test]
        public void An_empty_report_explains_how_to_add_single_notes()
        {
            StringAssert.Contains("Assets/Staging", SingleNoteReport.Empty.Text);
            StringAssert.Contains("every note is a chord", SingleNoteReport.Empty.Text);
        }

        [Test]
        public void A_report_names_the_count_the_moved_files_and_the_unused_names()
        {
            var report = new SingleNoteReport(2, new[] { "Piano Single - A3.wav" }, new[] { "\"Bb3\" does not look right." });
            StringAssert.Contains("Single note clips on Music Settings: 2.", report.Text);
            StringAssert.Contains("Moved from Assets/Staging: Piano Single - A3.wav.", report.Text);
            StringAssert.Contains("Not used: \"Bb3\" does not look right.", report.Text);
        }

        [Test]
        public void Only_unused_names_still_make_a_report_with_a_count_of_zero()
        {
            var report = new SingleNoteReport(0, new string[0], new[] { "bad" });
            StringAssert.Contains("Single note clips on Music Settings: 0.", report.Text);
            StringAssert.Contains("Not used: bad", report.Text);
        }
    }
}
