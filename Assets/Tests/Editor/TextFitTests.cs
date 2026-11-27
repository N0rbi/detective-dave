using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class TextFitTests
    {
        const float Tolerance = 0.0001f;
        const float Advance = 0.5f;
        const float Ratio = 1f;

        static string Words(int count) => string.Concat(System.Linq.Enumerable.Repeat("word ", count)).TrimEnd();

        [Test]
        public void ShortTextKeepsTheLargestSize()
        {
            var result = TextFit.Fit("Hello there", new Vector2(10f, 10f), 1f, 0.2f, 0.1f, Advance, Ratio);
            Assert.AreEqual(1f, result.LineHeight, Tolerance);
            Assert.IsTrue(result.Fits);
        }

        [Test]
        public void LongTextShrinksUntilItFits()
        {
            var text = Words(80);
            var result = TextFit.Fit(text, new Vector2(10f, 4f), 1f, 0.1f, 0.1f, Advance, Ratio);
            Assert.Less(result.LineHeight, 1f);
            Assert.IsTrue(result.Fits);
            Assert.LessOrEqual(TextFit.LineCount(TextWrap.Wrap(text, result.LettersPerLine)) * result.LineHeight, 4f);
        }

        [Test]
        public void TextThatNeverFitsStopsAtTheSmallestSizeAndReportsIt()
        {
            var result = TextFit.Fit(Words(20000), new Vector2(10f, 1f), 1f, 0.5f, 0.1f, Advance, Ratio);
            Assert.AreEqual(0.5f, result.LineHeight, Tolerance);
            Assert.IsFalse(result.Fits);
        }

        [Test]
        public void LetterCountFollowsTheSize()
        {
            Assert.AreEqual(20, TextFit.LettersPerLine(10f, 1f, Advance));
            Assert.AreEqual(40, TextFit.LettersPerLine(10f, 0.5f, Advance));
        }

        [Test]
        public void LineCountCountsBreaks()
        {
            Assert.AreEqual(0, TextFit.LineCount(""));
            Assert.AreEqual(1, TextFit.LineCount("one"));
            Assert.AreEqual(4, TextFit.LineCount("a\nb\n\nc"));
        }
    }
}
