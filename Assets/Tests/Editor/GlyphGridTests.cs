using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class GlyphPlanTests
    {
        const FontSlot Placeholder = FontSlot.Placeholder;
        const FontSlot Typed = FontSlot.Typed;

        [Test]
        public void Every_visible_character_gets_a_column_in_its_row()
        {
            var glyphs = GlyphPlan.Build(RichRuns.Parse("ab\ncd"), new[] { Typed, Typed, Typed, Placeholder, Placeholder });
            Assert.AreEqual(4, glyphs.Count);
            Assert.AreEqual((0, 0), (glyphs[0].Row, glyphs[0].Column));
            Assert.AreEqual((0, 1), (glyphs[1].Row, glyphs[1].Column));
            Assert.AreEqual((1, 0), (glyphs[2].Row, glyphs[2].Column));
            Assert.AreEqual((1, 1), (glyphs[3].Row, glyphs[3].Column));
        }

        [Test]
        public void A_space_is_not_drawn_but_keeps_its_column()
        {
            var glyphs = GlyphPlan.Build(RichRuns.Parse("a b"), new[] { Typed, Typed, Placeholder });
            Assert.AreEqual(2, glyphs.Count);
            Assert.AreEqual(0, glyphs[0].Column);
            Assert.AreEqual(2, glyphs[1].Column);
        }

        [Test]
        public void A_typed_letter_and_the_placeholder_that_it_replaces_share_one_cell()
        {
            var waiting = GlyphPlan.Build(RichRuns.Parse("abc"), new[] { Placeholder, Placeholder, Placeholder });
            var typed = GlyphPlan.Build(RichRuns.Parse("abc"), new[] { Typed, Placeholder, Placeholder });
            Assert.AreEqual(waiting[0].Column, typed[0].Column);
            Assert.AreEqual(waiting[0].Row, typed[0].Row);
            Assert.AreEqual(Typed, typed[0].Slot);
            Assert.AreEqual(Placeholder, waiting[0].Slot);
        }

        [Test]
        public void Every_glyph_keeps_the_slot_and_colour_of_its_character()
        {
            var glyphs = GlyphPlan.Build(RichRuns.Parse("<color=#111>ab</color><color=#999>cd</color>"), new[] { Typed, Typed, Placeholder, Placeholder });
            Assert.AreEqual("111", glyphs[1].ColorHex);
            Assert.AreEqual(Typed, glyphs[1].Slot);
            Assert.AreEqual("999", glyphs[2].ColorHex);
            Assert.AreEqual(Placeholder, glyphs[2].Slot);
        }

        [Test]
        public void Uncoloured_text_has_no_colour()
        {
            var glyphs = GlyphPlan.Build(RichRuns.Parse("a"), new[] { Typed });
            Assert.IsNull(glyphs[0].ColorHex);
        }

        [Test]
        public void A_blank_line_keeps_its_row()
        {
            var glyphs = GlyphPlan.Build(RichRuns.Parse("a\n\nb"), new[] { Typed, Typed, Typed, Typed });
            Assert.AreEqual(0, glyphs[0].Row);
            Assert.AreEqual(2, glyphs[1].Row);
            Assert.AreEqual(0, glyphs[1].Column);
        }

        [Test]
        public void A_slot_count_that_does_not_match_the_text_is_an_error()
        {
            Assert.Throws<System.ArgumentException>(() => GlyphPlan.Build(RichRuns.Parse("abc"), new[] { Typed }));
        }
    }

    public class GlyphCellTests
    {
        const float Tolerance = 0.0001f;

        [Test]
        public void A_glyph_is_centred_in_its_cell()
        {
            Assert.AreEqual(0.5f, GlyphCell.CentreX(0, 1f), Tolerance);
            Assert.AreEqual(3.5f, GlyphCell.CentreX(3, 1f), Tolerance);
            Assert.AreEqual(0.75f, GlyphCell.CentreX(1, 0.5f), Tolerance);
        }

        [Test]
        public void Every_cell_has_the_same_width_so_the_pitch_does_not_depend_on_the_font()
        {
            Assert.AreEqual(GlyphCell.CentreX(5, 0.3f) - GlyphCell.CentreX(4, 0.3f), GlyphCell.CentreX(9, 0.3f) - GlyphCell.CentreX(8, 0.3f), Tolerance);
        }

        [Test]
        public void A_glyph_that_fits_the_cell_keeps_its_width()
        {
            Assert.AreEqual(1f, GlyphCell.HorizontalScale(0.5f, 0.6f), Tolerance);
            Assert.AreEqual(1f, GlyphCell.HorizontalScale(0.6f, 0.6f), Tolerance);
        }

        [Test]
        public void A_glyph_wider_than_the_cell_is_squeezed_to_fit()
        {
            Assert.AreEqual(0.5f, GlyphCell.HorizontalScale(1.2f, 0.6f), Tolerance);
        }

        [Test]
        public void A_glyph_with_no_width_is_not_scaled()
        {
            Assert.AreEqual(1f, GlyphCell.HorizontalScale(0f, 0.6f), Tolerance);
        }

        [Test]
        public void The_columns_are_the_whole_cells_that_fit_the_text_width()
        {
            Assert.AreEqual(20, GlyphCell.Columns(10f, 0.5f));
            Assert.AreEqual(3, GlyphCell.Columns(1.9f, 0.6f));
            Assert.AreEqual(1, GlyphCell.Columns(0.1f, 0.6f));
        }

        [Test]
        public void The_last_column_ends_inside_the_text_width()
        {
            float cell = 0.37f;
            int columns = GlyphCell.Columns(10f, cell);
            Assert.LessOrEqual(GlyphCell.RightEdge(columns - 1, cell), 10f + Tolerance);
            Assert.Greater(GlyphCell.RightEdge(columns, cell), 10f - Tolerance);
        }

        [Test]
        public void A_row_sits_below_the_rows_before_it()
        {
            Assert.AreEqual(0f, GlyphCell.TopY(0f, 1.1f, 0f, 1f), Tolerance);
            Assert.AreEqual(-2.2f, GlyphCell.TopY(2f, 1.1f, 0f, 1f), Tolerance);
        }

        [Test]
        public void A_baseline_offset_moves_the_glyph_down_or_up_inside_its_row()
        {
            Assert.AreEqual(-0.11f, GlyphCell.TopY(0f, 1.1f, 0.1f, 1f), Tolerance);
            Assert.AreEqual(0.11f, GlyphCell.TopY(0f, 1.1f, -0.1f, 1f), Tolerance);
        }

        [Test]
        public void A_smaller_row_moves_by_its_own_share_of_the_baseline_offset()
        {
            Assert.AreEqual(-0.055f, GlyphCell.TopY(0f, 1.1f, 0.1f, 0.5f), Tolerance);
        }
    }
}
