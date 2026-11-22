using System;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class TextBlockFitTests
    {
        const float Tolerance = 0.0001f;
        const float Advance = 0.5f;
        const float Ratio = 1f;

        static string Words(int count) => string.Concat(System.Linq.Enumerable.Repeat("word ", count)).TrimEnd();

        [Test]
        public void Smaller_findings_fit_where_findings_at_full_size_do_not()
        {
            var area = new Vector2(10f, 8f);
            var full = new[] { new TextBlock(Words(40), 1f), new TextBlock(Words(40), 1f) };
            var smaller = new[] { new TextBlock(Words(40), 1f), new TextBlock(Words(40), 0.4f) };
            var fullResult = TextFit.Fit(full, area, 1f, 0.1f, 0.05f, Advance, Ratio);
            var smallerResult = TextFit.Fit(smaller, area, 1f, 0.1f, 0.05f, Advance, Ratio);
            Assert.Greater(smallerResult.LineHeight, fullResult.LineHeight);
        }

        [Test]
        public void The_result_height_never_passes_the_area()
        {
            var area = new Vector2(10f, 6f);
            var blocks = new[] { new TextBlock(Words(30), 1f), new TextBlock(Words(50), 0.85f) };
            var result = TextFit.Fit(blocks, area, 1f, 0.05f, 0.05f, Advance, Ratio);
            Assert.IsTrue(result.Fits);
            int descriptionLines = TextFit.LineCount(TextWrap.Wrap(blocks[0].Text, result.LettersPerLine));
            int findingsLetters = TextFit.LettersPerLine(area.x, result.LineHeight * 0.85f, Advance);
            int findingsLines = TextFit.LineCount(TextWrap.Wrap(blocks[1].Text, findingsLetters));
            float height = descriptionLines * result.LineHeight + result.LineHeight + findingsLines * result.LineHeight * 0.85f;
            Assert.LessOrEqual(height, area.y + Tolerance);
        }

        [Test]
        public void A_word_longer_than_the_line_keeps_shrinking_the_text()
        {
            var area = new Vector2(4f, 10f);
            var result = TextFit.Fit("abcdefghij", area, 1f, 0.1f, 0.1f, Advance, Ratio);
            Assert.IsTrue(result.Fits);
            Assert.GreaterOrEqual(result.LettersPerLine, 10);
        }

        [Test]
        public void An_empty_block_adds_no_height_and_no_divider()
        {
            var blocks = new[] { new TextBlock("one", 1f), new TextBlock(string.Empty, 0.5f) };
            var result = TextFit.Fit(blocks, new Vector2(10f, 1.5f), 1f, 0.1f, 0.1f, Advance, Ratio);
            Assert.AreEqual(1f, result.LineHeight, Tolerance);
            Assert.IsTrue(result.Fits);
        }

        [Test]
        public void The_longest_word_ignores_spaces_and_breaks()
        {
            Assert.AreEqual(5, TextFit.LongestWord("a bcdef\ngh"));
            Assert.AreEqual(0, TextFit.LongestWord(string.Empty));
        }
    }

    public class LineScalesTests
    {
        [Test]
        public void Uniform_scales_every_line_at_full_size()
        {
            Assert.AreEqual(1f, LineScales.Uniform.ShareOf(0));
            Assert.AreEqual(1f, LineScales.Uniform.ShareOf(40));
            Assert.AreEqual(7f, LineScales.Uniform.OffsetOf(7));
        }

        [Test]
        public void A_report_keeps_the_description_and_the_divider_full_size_and_shrinks_the_findings()
        {
            var scales = LineScales.Report("a\nb", "c\nd\ne", 0.5f);
            Assert.AreEqual(1f, scales.ShareOf(0));
            Assert.AreEqual(1f, scales.ShareOf(1));
            Assert.AreEqual(1f, scales.ShareOf(2));
            Assert.AreEqual(0.5f, scales.ShareOf(3));
            Assert.AreEqual(0.5f, scales.ShareOf(5));
        }

        [Test]
        public void A_report_matches_the_line_layout_of_the_shown_text()
        {
            var reveal = new PaperReveal("a\nb", "c\nd\ne");
            var scales = LineScales.Report("a\nb", "c\nd\ne", 0.5f);
            int lineCount = TextFit.LineCount(reveal.Shown(5));
            Assert.AreEqual(6, lineCount);
            Assert.AreEqual(0.5f, scales.ShareOf(lineCount - 1));
            Assert.AreEqual(1f, scales.ShareOf(2));
        }

        [Test]
        public void Offsets_add_up_the_shares_of_the_lines_above()
        {
            var scales = LineScales.Report("a\nb", "c\nd", 0.5f);
            Assert.AreEqual(0f, scales.OffsetOf(0), 0.0001f);
            Assert.AreEqual(3f, scales.OffsetOf(3), 0.0001f);
            Assert.AreEqual(3.5f, scales.OffsetOf(4), 0.0001f);
        }

        [Test]
        public void A_report_without_findings_has_no_divider_line()
        {
            var scales = LineScales.Report("a\nb", string.Empty, 0.5f);
            Assert.AreEqual(1f, scales.ShareOf(1));
            Assert.AreEqual(2f, scales.OffsetOf(2), 0.0001f);
        }
    }

    public class PaperScreenFontTests
    {
        static FontSlot SlotOf(TextRole role) => FontRoles.SlotOf(role);

        [TestCase(PaperScreen.Report)]
        [TestCase(PaperScreen.Review)]
        public void The_final_findings_use_the_findings_font_and_the_description_the_client_font(PaperScreen screen)
        {
            var roles = PaperScreens.RolesOf(screen);
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, SlotOf(roles.LeftSettled));
            Assert.AreEqual(FontSlot.Findings, SlotOf(roles.LeftRevealed));
        }

        [Test]
        public void The_letter_screen_uses_the_client_font_on_the_left_and_the_placeholder_and_typed_fonts_on_the_right()
        {
            var roles = PaperScreens.RolesOf(PaperScreen.Letter);
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, SlotOf(roles.LeftRevealed));
            Assert.AreEqual(FontSlot.ClientAndPlaceholder, SlotOf(roles.RightPending));
            Assert.AreEqual(FontSlot.Typed, SlotOf(roles.RightTyped));
        }

        [Test]
        public void The_notice_screen_uses_the_notice_font_on_the_right()
        {
            var roles = PaperScreens.RolesOf(PaperScreen.Notice);
            Assert.AreEqual(FontSlot.Notice, SlotOf(roles.RightPending));
            Assert.AreEqual(FontSlot.Notice, SlotOf(roles.RightTyped));
        }

        [Test]
        public void Every_screen_maps_every_role_to_a_font_slot()
        {
            foreach (PaperScreen screen in Enum.GetValues(typeof(PaperScreen)))
            {
                var roles = PaperScreens.RolesOf(screen);
                Assert.DoesNotThrow(() => SlotOf(roles.LeftSettled), screen.ToString());
                Assert.DoesNotThrow(() => SlotOf(roles.LeftRevealed), screen.ToString());
                Assert.DoesNotThrow(() => SlotOf(roles.RightPending), screen.ToString());
                Assert.DoesNotThrow(() => SlotOf(roles.RightTyped), screen.ToString());
            }
        }

        [Test]
        public void The_typing_screen_draws_typed_and_waiting_text_in_the_screen_fonts()
        {
            var roles = PaperScreens.RolesOf(PaperScreen.Report);
            var model = new ResponsePaperModel("hello", 20);
            var slots = model.TypingSlots(2, 5);
            Assert.AreEqual(SlotOf(roles.RightTyped), slots[0]);
            Assert.AreEqual(SlotOf(roles.RightPending), slots[4]);
        }

        [Test]
        public void The_review_screen_draws_all_the_response_in_the_typed_font()
        {
            var roles = PaperScreens.RolesOf(PaperScreen.Review);
            var slots = new ResponsePaperModel("hello", 20).ReviewSlots();
            foreach (var slot in slots) Assert.AreEqual(SlotOf(roles.RightTyped), slot);
        }

        [Test]
        public void Every_font_slot_has_a_role_that_uses_it()
        {
            foreach (FontSlot slot in Enum.GetValues(typeof(FontSlot)))
            {
                bool used = false;
                foreach (TextRole role in Enum.GetValues(typeof(TextRole))) used |= SlotOf(role) == slot;
                Assert.IsTrue(used, slot.ToString());
            }
        }
    }
}
