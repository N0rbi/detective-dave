using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DwellTimerTests
    {
        [Test]
        public void Completes_once_after_the_configured_time()
        {
            var timer = new DwellTimer(2f);
            timer.Start();
            Assert.IsFalse(timer.Tick(1.5f));
            Assert.AreEqual(0.75f, timer.Progress, 0.001f);
            Assert.IsTrue(timer.Tick(0.5f));
            Assert.IsFalse(timer.Tick(1f));
            Assert.IsFalse(timer.IsRunning);
        }

        [Test]
        public void Cancel_resets_the_progress()
        {
            var timer = new DwellTimer(2f);
            timer.Start();
            timer.Tick(1f);
            timer.Cancel();
            Assert.AreEqual(0f, timer.Progress);
            Assert.IsFalse(timer.Tick(5f));
        }

        [Test]
        public void Does_nothing_before_start()
        {
            Assert.IsFalse(new DwellTimer(1f).Tick(10f));
        }
    }

    public class TextWrapTests
    {
        [Test]
        public void Wraps_at_spaces_without_changing_the_length()
        {
            var wrapped = TextWrap.Wrap("aaa bbb ccc", 7);
            Assert.AreEqual("aaa bbb\nccc", wrapped);
            Assert.AreEqual(11, wrapped.Length);
        }

        [Test]
        public void Leaves_short_and_empty_text_alone()
        {
            Assert.AreEqual("short", TextWrap.Wrap("short", 20));
            Assert.AreEqual(string.Empty, TextWrap.Wrap(null, 5));
        }
    }

    public class RingGeometryTests
    {
        [Test]
        public void Is_empty_without_progress()
        {
            Assert.IsEmpty(RingGeometry.ArcPoints(UnityEngine.Vector2.zero, 1f, 0f, 32));
        }

        [Test]
        public void Starts_at_the_top_and_grows_with_progress()
        {
            var half = RingGeometry.ArcPoints(UnityEngine.Vector2.zero, 1f, 0.5f, 32);
            var full = RingGeometry.ArcPoints(UnityEngine.Vector2.zero, 1f, 1f, 32);
            Assert.AreEqual(1f, half[0].y, 0.001f);
            Assert.AreEqual(0f, half[0].x, 0.001f);
            Assert.Greater(full.Length, half.Length);
        }
    }

    public class CaseSessionTests
    {
        [TearDown]
        public void TearDown() => CaseSession.TryTakeSolved(out _);

        [Test]
        public void Solved_id_is_taken_once_and_clears_the_session()
        {
            var file = LegacyCaseFiles.Named("case-a");
            CaseSession.Begin(file);
            Assert.IsTrue(CaseSession.IsActive);
            CaseSession.MarkLevelSolved();
            Assert.IsTrue(CaseSession.TryTakeSolved(out var id));
            Assert.AreEqual("case-a", id);
            Assert.IsFalse(CaseSession.IsActive);
            Assert.IsFalse(CaseSession.TryTakeSolved(out _));
            UnityEngine.Object.DestroyImmediate(file);
        }
    }

    public class LetterPaperModelTests
    {
        const string Typed = "111111";
        const string Pending = "999999";
        const string Green = "22AA22";
        const string Red = "CC2222";

        static readonly LetterPalette Palette = new LetterPalette(Typed, Pending, Green, Red);

        [Test]
        public void Shows_both_messages_untyped_at_the_start()
        {
            var paper = new LetterPaperModel("Ann", "Yes please.", "No thanks.", 100);
            StringAssert.Contains("Yes please.\nNo thanks.", paper.Plain);
            var rendered = paper.Render(0, LetterPath.Undecided, Palette, 1f);
            StringAssert.DoesNotContain(Typed, rendered);
        }

        [Test]
        public void Tints_the_head_once_it_is_typed()
        {
            var paper = new LetterPaperModel("Ann", "Yes please.", "No thanks.", 100);
            var rendered = paper.Render(LetterText.Head("Ann").Length, LetterPath.Undecided, Palette, 1f);
            StringAssert.StartsWith("<color=#" + Typed + ">Dear Ann,", rendered);
        }

        [Test]
        public void The_accept_answer_waits_in_green_and_the_decline_answer_in_red()
        {
            var paper = new LetterPaperModel("Ann", "Yes", "No", 100);
            var rendered = paper.Render(LetterText.Head("Ann").Length, LetterPath.Undecided, Palette, 1f);
            StringAssert.Contains("<color=#" + Green + ">Yes</color>", rendered);
            StringAssert.Contains("<color=#" + Red + ">No</color>", rendered);
        }

        [Test]
        public void The_chosen_answer_goes_grey_and_black_and_the_other_one_fades_in_place()
        {
            var paper = new LetterPaperModel("Ann", "Yes", "No", 100);
            int typed = LetterText.Head("Ann").Length + "Yes".Length;
            var rendered = paper.Render(typed, LetterPath.Accept, Palette, 0f);
            StringAssert.StartsWith("<color=#" + Typed + ">Dear Ann,\n\nYes</color>", rendered);
            StringAssert.Contains("<color=#" + Pending + "00>No</color>", rendered);
        }

        [Test]
        public void The_fade_keeps_the_text_so_the_space_stays()
        {
            var paper = new LetterPaperModel("Ann", "Yes", "No", 100);
            var rendered = paper.Render(LetterText.Head("Ann").Length + 3, LetterPath.Accept, Palette, 0.5f);
            StringAssert.Contains("<color=#" + Pending + "80>No</color>", rendered);
            Assert.AreEqual(paper.Plain.Length, StripTags(rendered).Length);
        }

        [Test]
        public void The_declined_side_fades_the_accept_answer_instead()
        {
            var paper = new LetterPaperModel("Ann", "Yes", "No", 100);
            var rendered = paper.Render(LetterText.Head("Ann").Length + 1, LetterPath.Decline, Palette, 1f);
            StringAssert.Contains(Pending + "FF>", rendered);
        }

        static string StripTags(string rendered) => System.Text.RegularExpressions.Regex.Replace(rendered, "<[^>]*>", string.Empty);
    }
}
