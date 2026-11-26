using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class BoardFitTests
    {
        static readonly Vector2 WideBoard = new Vector2(4f, 2f);

        [TestCase(10f, 10f)]
        [TestCase(20f, 5f)]
        [TestCase(5f, 20f)]
        public void Fit_inside_never_overflows_the_view(float viewWidth, float viewHeight)
        {
            var scale = BoardFit.Scale(WideBoard, new Vector2(viewWidth, viewHeight), BoardFitMode.FitInside, 0.03f, 1f);
            Assert.LessOrEqual(WideBoard.x * scale, viewWidth * 0.94f + 0.0001f);
            Assert.LessOrEqual(WideBoard.y * scale, viewHeight * 0.94f + 0.0001f);
        }

        [Test]
        public void Fit_inside_touches_the_margin_on_the_tighter_side()
        {
            var scale = BoardFit.Scale(WideBoard, new Vector2(10f, 10f), BoardFitMode.FitInside, 0.05f, 1f);
            Assert.AreEqual(9f, WideBoard.x * scale, 0.0001f);
        }

        [Test]
        public void Cover_fills_the_usable_view_on_both_sides()
        {
            var scale = BoardFit.Scale(WideBoard, new Vector2(10f, 10f), BoardFitMode.Cover, 0f, 1f);
            Assert.AreEqual(10f, WideBoard.y * scale, 0.0001f);
            Assert.GreaterOrEqual(WideBoard.x * scale, 10f);
        }

        [Test]
        public void The_multiplier_scales_the_result()
        {
            var plain = BoardFit.Scale(WideBoard, new Vector2(10f, 10f), BoardFitMode.FitInside, 0.03f, 1f);
            Assert.AreEqual(plain * 0.5f, BoardFit.Scale(WideBoard, new Vector2(10f, 10f), BoardFitMode.FitInside, 0.03f, 0.5f), 0.0001f);
        }

        [Test]
        public void The_offset_is_a_share_of_the_view()
        {
            Assert.AreEqual(new Vector2(3f, 1f), BoardFit.Position(new Vector2(1f, 0f), new Vector2(10f, 4f), new Vector2(0.2f, 0.25f)));
        }

        [Test]
        public void Zero_offset_keeps_the_board_centred()
        {
            Assert.AreEqual(new Vector2(2f, 3f), BoardFit.Position(new Vector2(2f, 3f), new Vector2(10f, 4f), Vector2.zero));
        }
    }
}
