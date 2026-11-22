using DetectiveDave.CheatPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class KeyLetterTests
    {
        [TestCase("a", 'a')]
        [TestCase("R", 'r')]
        [TestCase("ö", 'ö')]
        public void ALetterKeyGivesItsLowercaseLetter(string displayName, char expected)
        {
            Assert.IsTrue(KeyLetter.TryFrom(displayName, out var letter));
            Assert.AreEqual(expected, letter);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("Shift")]
        [TestCase("Left Arrow")]
        [TestCase("1")]
        [TestCase(".")]
        [TestCase(" ")]
        public void AnythingButASingleLetterGivesNoLetter(string displayName)
        {
            Assert.IsFalse(KeyLetter.TryFrom(displayName, out _));
        }
    }
}
