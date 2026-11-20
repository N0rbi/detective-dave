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
            var file = UnityEngine.ScriptableObject.CreateInstance<CaseFile>();
            file.name = "case-a";
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

        [Test]
        public void Shows_both_messages_untyped_at_the_start()
        {
            var paper = new LetterPaperModel("Ann", "Yes please.", "No thanks.", 100);
            StringAssert.Contains("Yes please.\nNo thanks.", paper.Plain);
            var rendered = paper.Render(0, LetterPath.Undecided, Typed, Pending);
            StringAssert.DoesNotContain(Typed, rendered);
        }

        [Test]
        public void Tints_the_head_once_it_is_typed()
        {
            var paper = new LetterPaperModel("Ann", "Yes please.", "No thanks.", 100);
            var rendered = paper.Render(LetterText.Head("Ann").Length, LetterPath.Undecided, Typed, Pending);
            StringAssert.StartsWith("<color=#" + Typed + ">Dear Ann,", rendered);
        }
    }
}
