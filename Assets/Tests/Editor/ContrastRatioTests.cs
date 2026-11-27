using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class ContrastRatioTests
    {
        [Test]
        public void Black_on_white_is_twenty_one()
        {
            Assert.AreEqual(21f, ContrastRatio.Of(Color.black, Color.white), 0.01f);
        }

        [Test]
        public void The_same_colour_is_one()
        {
            Assert.AreEqual(1f, ContrastRatio.Of(Color.gray, Color.gray), 0.0001f);
        }

        [Test]
        public void The_order_of_the_colours_does_not_matter()
        {
            Assert.AreEqual(ContrastRatio.Of(Color.red, Color.white), ContrastRatio.Of(Color.white, Color.red), 0.0001f);
        }

        [Test]
        public void Pale_text_on_pale_paper_is_not_readable()
        {
            var paper = new Color(0.93f, 0.92f, 0.87f);

            Assert.IsFalse(ContrastRatio.IsReadable(new Color(0.85f, 0.85f, 0.8f), paper, 3f));
        }

        [Test]
        public void Dark_text_on_pale_paper_is_readable()
        {
            var paper = new Color(0.93f, 0.92f, 0.87f);

            Assert.IsTrue(ContrastRatio.IsReadable(new Color(0.1f, 0.1f, 0.12f), paper, 3f));
        }

        [Test]
        public void A_ratio_equal_to_the_minimum_is_readable()
        {
            float ratio = ContrastRatio.Of(Color.black, Color.gray);

            Assert.IsTrue(ContrastRatio.IsReadable(Color.black, Color.gray, ratio));
        }

        [Test]
        public void A_lighter_colour_gives_a_lower_ratio_against_white()
        {
            Assert.Less(ContrastRatio.Of(Color.gray, Color.white), ContrastRatio.Of(Color.black, Color.white));
        }
    }
}
