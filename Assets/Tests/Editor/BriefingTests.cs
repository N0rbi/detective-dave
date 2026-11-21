using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class TimedRevealTests
    {
        [Test]
        public void Reveals_at_a_constant_speed_and_stops_at_the_length()
        {
            var reveal = new TimedReveal(10, 0.25f, 3f);
            Assert.AreEqual(0, reveal.VisibleCount);
            Assert.AreEqual(4, reveal.Advance(1f));
            Assert.IsFalse(reveal.IsDone);
            reveal.Advance(10f);
            Assert.AreEqual(10, reveal.VisibleCount);
            Assert.IsTrue(reveal.IsDone);
        }

        [Test]
        public void Skipping_multiplies_the_speed_from_then_on()
        {
            var reveal = new TimedReveal(100, 0.25f, 3f);
            reveal.Advance(1f);
            reveal.Skip();
            reveal.Advance(1f);
            Assert.AreEqual(4 + 12, reveal.VisibleCount);
            Assert.IsTrue(reveal.IsSkipping);
        }

        [Test]
        public void An_empty_text_is_done_at_once()
        {
            Assert.IsTrue(new TimedReveal(0, 0.1f, 3f).IsDone);
        }
    }

    public class BriefingGateTests
    {
        static BriefingGate NewGate(int length = 10) => new BriefingGate(new TimedReveal(length, 0.25f, 3f));

        [Test]
        public void The_letter_is_not_writable_until_the_description_is_finished()
        {
            var gate = NewGate();
            gate.Tick(1f);
            Assert.IsFalse(gate.IsWritable);
            gate.Tick(1f);
            Assert.IsFalse(gate.IsWritable);
            gate.Tick(0.6f);
            Assert.IsTrue(gate.IsWritable);
        }

        [Test]
        public void Any_key_before_the_gate_opens_speeds_the_description_up_for_good()
        {
            var normal = NewGate(100);
            var skipped = NewGate(100);
            skipped.OnAnyKey();
            normal.Tick(2f);
            skipped.Tick(2f);
            Assert.AreEqual(8, normal.VisibleCount);
            Assert.AreEqual(24, skipped.VisibleCount);
            skipped.Tick(2f);
            Assert.AreEqual(48, skipped.VisibleCount);
        }

        [Test]
        public void Keys_after_the_gate_opens_change_nothing()
        {
            var gate = NewGate(4);
            gate.Tick(5f);
            Assert.IsTrue(gate.IsWritable);
            gate.OnAnyKey();
            Assert.AreEqual(4, gate.VisibleCount);
            Assert.AreEqual(0, gate.Tick(5f));
        }

        [Test]
        public void An_empty_description_opens_the_gate_on_the_first_tick()
        {
            var gate = NewGate(0);
            Assert.IsFalse(gate.IsWritable);
            gate.Tick(0f);
            Assert.IsTrue(gate.IsWritable);
        }
    }

    public class BriefingLayoutTests
    {
        [Test]
        public void Places_the_paper_relative_to_the_view()
        {
            var spot = BriefingLayout.Place(new PaperPlacement(-0.25f, 0.1f, 0.4f, -2f), new Vector2(1f, 2f), new Vector2(20f, 10f));
            Assert.AreEqual(-4f, spot.Centre.x, 0.001f);
            Assert.AreEqual(3f, spot.Centre.y, 0.001f);
            Assert.AreEqual(8f, spot.Width, 0.001f);
            Assert.AreEqual(-2f, spot.RotationDegrees);
        }

        [Test]
        public void Counts_the_letters_that_fit_inside_the_padding()
        {
            Assert.AreEqual(20, BriefingLayout.LettersPerLine(10f, 0.1f, 0.5f, 0.8f));
            Assert.AreEqual(1, BriefingLayout.LettersPerLine(0.1f, 0.4f, 5f, 1f));
        }
    }

    public class StoryDisplayTests
    {
        [Test]
        public void Splits_the_text_into_typed_pending_and_hidden()
        {
            var display = StoryDisplay.Of("hello big world", 8, 10);
            Assert.AreEqual("hello bi", display.Typed);
            Assert.AreEqual("g ", display.Pending);
            Assert.AreEqual("world", display.Hidden);
        }

        [Test]
        public void At_the_start_only_the_first_unit_is_pending()
        {
            var display = StoryDisplay.Of("hello big world", 0, 6);
            Assert.AreEqual(string.Empty, display.Typed);
            Assert.AreEqual("hello ", display.Pending);
            Assert.AreEqual("big world", display.Hidden);
        }

        [Test]
        public void At_the_end_everything_is_typed()
        {
            var display = StoryDisplay.Of("abc", 3, 3);
            Assert.AreEqual("abc", display.Typed);
            Assert.AreEqual(string.Empty, display.Pending);
            Assert.AreEqual(string.Empty, display.Hidden);
        }

        [Test]
        public void Counts_past_the_text_or_before_the_typed_part_are_clamped()
        {
            var display = StoryDisplay.Of("abc", 99, 1);
            Assert.AreEqual("abc", display.Typed);
            var early = StoryDisplay.Of("abc", -4, -2);
            Assert.AreEqual(string.Empty, early.Typed);
            Assert.AreEqual(string.Empty, early.Pending);
            Assert.AreEqual("abc", early.Hidden);
        }

        [Test]
        public void A_unit_that_is_already_typed_up_to_its_end_has_no_pending_letters()
        {
            var display = StoryDisplay.Of("ab cd", 3, 3);
            Assert.AreEqual("ab ", display.Typed);
            Assert.AreEqual(string.Empty, display.Pending);
            Assert.AreEqual("cd", display.Hidden);
        }
    }

    public class StoryDisplayFlowTests
    {
        const string Story = "Hi there. Bye now.";

        static string Shown(StoryTyper typer) => StoryDisplay.Of(Story, typer.TypedCount, typer.CurrentUnitEnd).Typed + "|" +
            StoryDisplay.Of(Story, typer.TypedCount, typer.CurrentUnitEnd).Pending + "|" +
            StoryDisplay.Of(Story, typer.TypedCount, typer.CurrentUnitEnd).Hidden;

        static void Type(StoryTyper typer, string text)
        {
            foreach (var character in text) Assert.IsTrue(typer.TryType(character));
        }

        [Test]
        public void Word_unit_shows_the_typed_letters_black_and_only_the_current_word_grey()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            Assert.AreEqual("|Hi |there. Bye now.", Shown(typer));
            Type(typer, "h");
            Assert.AreEqual("H|i |there. Bye now.", Shown(typer));
            Type(typer, "i ");
            Assert.AreEqual("Hi |there. |Bye now.", Shown(typer));
        }

        [Test]
        public void Auto_typed_punctuation_turns_black_at_once()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            Type(typer, "hi there");
            Assert.AreEqual("Hi there.| |Bye now.", Shown(typer));
        }

        [Test]
        public void Backspace_turns_a_typed_letter_grey_again()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            Type(typer, "hi t");
            Assert.AreEqual("Hi t|here. |Bye now.", Shown(typer));
            typer.Backspace();
            Assert.AreEqual("Hi |there. |Bye now.", Shown(typer));
        }

        [Test]
        public void Sentence_unit_shows_one_sentence_at_a_time()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Sentence);
            Assert.AreEqual("|Hi there. |Bye now.", Shown(typer));
            Type(typer, "hi there ");
            Assert.AreEqual("Hi there. |Bye now.|", Shown(typer));
        }

        [Test]
        public void Character_unit_shows_the_next_character_grey()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Character);
            Type(typer, "hi");
            Assert.AreEqual("Hi|" + " " + "|there. Bye now.", Shown(typer));
        }

        [Test]
        public void The_end_of_the_text_leaves_nothing_pending_or_hidden()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            Type(typer, "hi there bye now");
            Assert.IsTrue(typer.IsComplete);
            Assert.AreEqual("Hi there. Bye now.||", Shown(typer));
        }
    }

    public class FindingsPaperModelTests
    {
        [Test]
        public void Renders_typed_pending_and_an_invisible_rest()
        {
            var model = new FindingsPaperModel("abc def ghi", 100);
            Assert.AreEqual("<color=#111>abc </color><color=#999>def </color><color=#00000000>ghi</color>", model.RenderTyping(4, 8, "111", "999"));
        }

        [Test]
        public void Typing_at_the_start_shows_only_the_first_unit_pending()
        {
            var model = new FindingsPaperModel("abc def", 100);
            Assert.AreEqual("<color=#999>abc </color><color=#00000000>def</color>", model.RenderTyping(0, 4, "111", "999"));
        }

        [Test]
        public void Typing_at_the_end_shows_everything_typed()
        {
            var model = new FindingsPaperModel("abc", 100);
            Assert.AreEqual("<color=#111>abc</color>", model.RenderTyping(3, 3, "111", "999"));
        }

        [Test]
        public void Review_shows_everything_typed()
        {
            var model = new FindingsPaperModel("abc def", 100);
            Assert.AreEqual("<color=#111>abc def</color>", model.RenderReview("111"));
        }

        [Test]
        public void Wrapping_keeps_the_length_so_typed_counts_still_line_up()
        {
            var model = new FindingsPaperModel("aaa bbb ccc", 7);
            Assert.AreEqual(11, model.Plain.Length);
        }
    }
}
