using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class StoryTyperTests
    {
        const string Story = "Hi there. Bye now.";

        static void TypeRequired(StoryTyper typer, string text)
        {
            foreach (var character in text)
            {
                if (TypingRules.IsFreeCharacter(character)) continue;
                Assert.IsTrue(typer.TryType(character), "could not type '" + character + "'");
            }
        }

        [Test]
        public void CharacterUnitRevealsWhatIsTyped()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Character);
            TypeRequired(typer, "hi");
            Assert.AreEqual(2, typer.RevealedCount);
        }

        [Test]
        public void WordUnitRevealsOnTheSpaceAfterTheWord()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "hi");
            Assert.AreEqual(0, typer.RevealedCount);
            TypeRequired(typer, " ");
            Assert.AreEqual(3, typer.RevealedCount);
        }

        [Test]
        public void WordUnitKeepsTheTrailingPunctuationWithItsWord()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "hi there");
            Assert.AreEqual(3, typer.RevealedCount);
            TypeRequired(typer, " ");
            Assert.AreEqual(10, typer.RevealedCount);
        }

        [Test]
        public void SentenceUnitRevealsAfterTheSentenceAndItsSpace()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Sentence);
            TypeRequired(typer, "hi there");
            Assert.AreEqual(0, typer.RevealedCount);
            TypeRequired(typer, " ");
            Assert.AreEqual(10, typer.RevealedCount);
        }

        [TestCase(RevealUnit.Character)]
        [TestCase(RevealUnit.Word)]
        [TestCase(RevealUnit.Sentence)]
        public void TheWholeStoryIsRevealedWhenTypedFully(RevealUnit unit)
        {
            var typer = new StoryTyper(Story, TypingRules.Default, unit);
            TypeRequired(typer, Story);
            Assert.IsTrue(typer.IsComplete);
            Assert.AreEqual(Story.Length, typer.RevealedCount);
        }

        [Test]
        public void TextWithoutSpacesIsOneWord()
        {
            var typer = new StoryTyper("abc", TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "ab");
            Assert.AreEqual(0, typer.RevealedCount);
            TypeRequired(typer, "c");
            Assert.AreEqual(3, typer.RevealedCount);
        }

        [Test]
        public void BackspaceTakesBackARevealedWord()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "hi ");
            typer.Backspace();
            Assert.AreEqual(0, typer.RevealedCount);
        }

        [Test]
        public void WrongKeyDoesNotProgress()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Character);
            Assert.IsFalse(typer.TryType('z'));
            Assert.AreEqual(0, typer.TypedCount);
        }

        [TestCase(null)]
        [TestCase("")]
        public void EmptyStoryIsCompleteAtOnce(string text)
        {
            var typer = new StoryTyper(text, TypingRules.Default, RevealUnit.Sentence);
            Assert.IsTrue(typer.IsComplete);
            Assert.AreEqual(0, typer.RevealedCount);
        }

        [Test]
        public void NewlinesBetweenSentencesAreTypedByThemselves()
        {
            var typer = new StoryTyper("One.\n\nTwo.", TypingRules.Default, RevealUnit.Sentence);
            TypeRequired(typer, "one");
            Assert.AreEqual(6, typer.TypedCount);
            Assert.AreEqual(6, typer.RevealedCount);
        }
    }
}
