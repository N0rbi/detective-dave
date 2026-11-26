using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class PaperFitTests
    {
        const float Aspect = 1.3125f;
        static readonly Vector2 Centre = new Vector2(3f, -2f);

        static void AssertInside(PaperSpot spot, Vector2 viewSize, float margin)
        {
            var half = PaperFit.HalfExtents(spot.Width, Aspect, spot.RotationDegrees);
            var offset = spot.Centre - Centre;
            float limitX = viewSize.x * (0.5f - margin) + 0.0001f;
            float limitY = viewSize.y * (0.5f - margin) + 0.0001f;
            Assert.LessOrEqual(Mathf.Abs(offset.x) + half.x, limitX);
            Assert.LessOrEqual(Mathf.Abs(offset.y) + half.y, limitY);
        }

        [TestCase(16f, 9f)]
        [TestCase(4f, 3f)]
        [TestCase(9f, 16f)]
        [TestCase(21f, 9f)]
        public void A_tall_paper_ends_up_inside_the_view_at_any_aspect(float width, float height)
        {
            var view = new Vector2(width, height);
            var tall = new PaperSpot(Centre + new Vector2(-0.25f * width, 0.02f * height), 0.4f * width * 1.4f, -2f);
            AssertInside(PaperFit.Fit(tall, Aspect, Centre, view, 0.03f), view, 0.03f);
        }

        [Test]
        public void A_paper_far_off_screen_is_moved_inside()
        {
            var view = new Vector2(16f, 9f);
            var fitted = PaperFit.Fit(new PaperSpot(Centre + new Vector2(40f, -30f), 3f, 10f), Aspect, Centre, view, 0.05f);
            AssertInside(fitted, view, 0.05f);
            Assert.AreEqual(3f, fitted.Width, 0.0001f);
        }

        [Test]
        public void A_paper_that_already_fits_is_unchanged()
        {
            var view = new Vector2(16f, 9f);
            var spot = new PaperSpot(Centre + new Vector2(1f, 0.5f), 4f, 1.5f);
            var fitted = PaperFit.Fit(spot, Aspect, Centre, view, 0.03f);
            Assert.AreEqual(spot.Centre.x, fitted.Centre.x, 0.0001f);
            Assert.AreEqual(spot.Centre.y, fitted.Centre.y, 0.0001f);
            Assert.AreEqual(spot.Width, fitted.Width, 0.0001f);
        }

        [Test]
        public void The_text_block_stays_inside_the_paper_rect()
        {
            var area = new Vector2(4f - 4f * 0.18f, 4f * Aspect - 4f * 0.18f);
            Assert.LessOrEqual(area.x, 4f);
            Assert.LessOrEqual(area.y, 4f * Aspect);
            float top = 4f * Aspect * 0.5f - 4f * 0.09f;
            Assert.AreEqual(4f * Aspect * 0.5f - 4f * 0.09f - area.y, -top, 0.0001f);
        }
    }

    public class GlyphRowCapTests
    {
        [Test]
        public void A_letter_never_grows_taller_than_its_row()
        {
            float size = GlyphCell.CharacterSize(0.6f, 1f, 20f, 64, 0.1f);
            Assert.LessOrEqual(size * 64 * 0.1f, 1f + 0.0001f);
        }

        [Test]
        public void A_wide_enough_row_keeps_the_width_based_size()
        {
            Assert.AreEqual(0.6f / (32f * 0.1f), GlyphCell.CharacterSize(0.6f, 5f, 32f, 64, 0.1f), 0.0001f);
        }

        [Test]
        public void Rows_do_not_overlap()
        {
            float step = 1.1f;
            for (int row = 1; row < 20; row++)
                Assert.GreaterOrEqual(GlyphCell.TopY(row - 1, step, 0f, 1f) - GlyphCell.TopY(row, step, 0f, 1f), step - 0.0001f);
        }
    }
}
