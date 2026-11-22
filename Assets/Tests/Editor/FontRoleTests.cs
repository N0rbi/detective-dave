using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class FontRolesTests
    {
        [Test]
        public void The_client_message_and_the_placeholder_share_one_font()
        {
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, FontRoles.SlotOf(TextRole.ClientMessage));
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, FontRoles.SlotOf(TextRole.Placeholder));
        }

        [Test]
        public void The_findings_and_the_typed_text_have_their_own_fonts()
        {
            Assert.AreEqual(FontSlot.Findings, FontRoles.SlotOf(TextRole.Findings));
            Assert.AreEqual(FontSlot.Typed, FontRoles.SlotOf(TextRole.Typed));
            Assert.AreNotEqual(FontRoles.SlotOf(TextRole.Findings), FontRoles.SlotOf(TextRole.Typed));
        }

        [Test]
        public void The_notice_has_its_own_font()
        {
            Assert.AreEqual(FontSlot.Notice, FontRoles.SlotOf(TextRole.Notice));
        }

        [Test]
        public void Fill_gives_every_character_the_same_slot()
        {
            CollectionAssert.AreEqual(new[] { FontSlot.Notice, FontSlot.Notice, FontSlot.Notice }, FontRoles.Fill(3, TextRole.Notice));
        }
    }

    public class RichRunsTests
    {
        [Test]
        public void Plain_text_is_one_uncoloured_run()
        {
            var runs = RichRuns.Parse("abc");
            Assert.AreEqual(1, runs.Count);
            Assert.AreEqual("abc", runs[0].Text);
            Assert.IsNull(runs[0].ColorHex);
        }

        [Test]
        public void Colour_tags_become_coloured_runs_in_order()
        {
            var runs = RichRuns.Parse("<color=#111>ab</color><color=#99999980>cd</color>");
            Assert.AreEqual(2, runs.Count);
            Assert.AreEqual("ab", runs[0].Text);
            Assert.AreEqual("111", runs[0].ColorHex);
            Assert.AreEqual("cd", runs[1].Text);
            Assert.AreEqual("99999980", runs[1].ColorHex);
        }

        [Test]
        public void Text_around_a_tag_stays_uncoloured()
        {
            var runs = RichRuns.Parse("a<color=#111>b</color>c");
            Assert.AreEqual(3, runs.Count);
            Assert.IsNull(runs[0].ColorHex);
            Assert.AreEqual("111", runs[1].ColorHex);
            Assert.IsNull(runs[2].ColorHex);
        }

        [Test]
        public void An_unclosed_tag_is_an_error()
        {
            Assert.Throws<System.FormatException>(() => RichRuns.Parse("<color=#111>ab"));
        }
    }

    public class LayerLinesTests
    {
        const FontSlot Client = FontSlot.ClientAndPlaceholder;
        const FontSlot Typed = FontSlot.Typed;

        [Test]
        public void A_layer_keeps_its_own_characters_and_hides_the_others_in_place()
        {
            var runs = RichRuns.Parse("<color=#111>ab</color><color=#999>cd</color>");
            var slots = new[] { Typed, Typed, Client, Client };
            Assert.AreEqual(new[] { "<color=#111>ab</color><color=#00000000>cd</color>" }, LayerLines.Build(runs, slots, Typed));
            Assert.AreEqual(new[] { "<color=#00000000>ab</color><color=#999>cd</color>" }, LayerLines.Build(runs, slots, Client));
        }

        [Test]
        public void Lines_split_at_line_breaks()
        {
            var runs = RichRuns.Parse("ab\ncd");
            var slots = new[] { Client, Client, Client, Client, Client };
            CollectionAssert.AreEqual(new[] { "ab", "cd" }, LayerLines.Build(runs, slots, Client));
        }

        [Test]
        public void A_line_without_any_character_of_the_layer_is_null()
        {
            var runs = RichRuns.Parse("ab\ncd");
            var slots = new[] { Typed, Typed, Typed, Client, Client };
            CollectionAssert.AreEqual(new[] { "ab", null }, LayerLines.Build(runs, slots, Typed));
        }

        [Test]
        public void A_blank_line_keeps_its_place_as_null()
        {
            var runs = RichRuns.Parse("a\n\nb");
            var slots = new[] { Client, Client, Client, Client };
            CollectionAssert.AreEqual(new[] { "a", null, "b" }, LayerLines.Build(runs, slots, Client));
        }

        [Test]
        public void A_run_that_spans_two_slots_is_split_into_visible_and_hidden_parts()
        {
            var runs = RichRuns.Parse("<color=#111>abcd</color>");
            var slots = new[] { Typed, Typed, Client, Client };
            Assert.AreEqual(new[] { "<color=#111>ab</color><color=#00000000>cd</color>" }, LayerLines.Build(runs, slots, Typed));
        }

        [Test]
        public void Uncoloured_text_of_the_layer_stays_uncoloured()
        {
            var runs = RichRuns.Parse("ab");
            var slots = new[] { Client, Typed };
            Assert.AreEqual(new[] { "a<color=#00000000>b</color>" }, LayerLines.Build(runs, slots, Client));
        }

        [Test]
        public void A_slot_count_that_does_not_match_the_text_is_an_error()
        {
            var runs = RichRuns.Parse("abc");
            Assert.Throws<System.ArgumentException>(() => LayerLines.Build(runs, new[] { Client }, Client));
        }
    }

    public class FontSlotsOfModelsTests
    {
        [Test]
        public void The_left_paper_uses_the_client_font_for_the_description_and_the_findings_font_for_the_findings()
        {
            var reveal = new PaperReveal("desc", "found");
            var slots = reveal.Slots(3, TextRole.ClientMessage, TextRole.Findings);
            Assert.AreEqual(reveal.Shown(3).Length, slots.Length);
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, slots[0]);
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, slots[4 + PaperReveal.Separator.Length - 1]);
            Assert.AreEqual(FontSlot.Findings, slots[4 + PaperReveal.Separator.Length]);
            Assert.AreEqual(FontSlot.Findings, slots[slots.Length - 1]);
        }

        [Test]
        public void The_left_paper_slots_follow_the_revealed_count()
        {
            var reveal = new PaperReveal("desc", "found");
            Assert.AreEqual(reveal.Shown(0).Length, reveal.Slots(0, TextRole.ClientMessage, TextRole.Findings).Length);
            Assert.AreEqual(reveal.Shown(99).Length, reveal.Slots(99, TextRole.ClientMessage, TextRole.Findings).Length);
            Assert.AreEqual(reveal.Shown(0).Length, reveal.Slots(-4, TextRole.ClientMessage, TextRole.Findings).Length);
        }

        [Test]
        public void With_nothing_settled_the_whole_left_paper_is_the_client_message()
        {
            var reveal = new PaperReveal(string.Empty, "intro");
            CollectionAssert.AreEqual(FontRoles.Fill(2, TextRole.ClientMessage), reveal.Slots(2, TextRole.ClientMessage, TextRole.ClientMessage));
        }

        [Test]
        public void Typed_response_letters_use_the_typed_font_and_the_rest_the_placeholder_font()
        {
            var model = new ResponsePaperModel("abc def ghi", 100);
            var slots = model.TypingSlots(4, 8);
            Assert.AreEqual(11, slots.Length);
            for (int index = 0; index < 4; index++) Assert.AreEqual(FontSlot.Typed, slots[index]);
            for (int index = 4; index < 11; index++) Assert.AreEqual(FontSlot.ClientAndPlaceholder, slots[index]);
        }

        [Test]
        public void A_reviewed_response_is_all_in_the_typed_font()
        {
            var model = new ResponsePaperModel("abc def", 100);
            CollectionAssert.AreEqual(FontRoles.Fill(7, TextRole.Typed), model.ReviewSlots());
        }

        [Test]
        public void The_response_slots_match_the_rendered_text_length()
        {
            var model = new ResponsePaperModel("abc def ghi", 100);
            var runs = RichRuns.Parse(model.RenderTyping(4, 8, "111", "999"));
            int visible = 0;
            foreach (var run in runs) visible += run.Text.Length;
            Assert.AreEqual(visible, model.TypingSlots(4, 8).Length);
        }

        [Test]
        public void The_letter_uses_the_typed_font_for_typed_letters_and_the_placeholder_font_for_the_rest()
        {
            var paper = new LetterPaperModel("Ann", "Yes", "No", 100);
            int head = LetterText.Head("Ann").Length;
            var slots = paper.Slots(head, LetterPath.Undecided);
            Assert.AreEqual(paper.Plain.Length, slots.Length);
            Assert.AreEqual(FontSlot.Typed, slots[0]);
            Assert.AreEqual(FontSlot.Typed, slots[head - 1]);
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, slots[head]);
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, slots[slots.Length - 1]);
        }

        [Test]
        public void The_letter_slots_match_the_rendered_text_length()
        {
            var paper = new LetterPaperModel("Ann", "Yes", "No", 100);
            var palette = new LetterPalette("111111", "999999", "22AA22", "CC2222");
            var runs = RichRuns.Parse(paper.Render(LetterText.Head("Ann").Length + 3, LetterPath.Accept, palette, 0.5f));
            int visible = 0;
            foreach (var run in runs) visible += run.Text.Length;
            Assert.AreEqual(visible, paper.Slots(LetterText.Head("Ann").Length + 3, LetterPath.Accept).Length);
        }
    }
}
