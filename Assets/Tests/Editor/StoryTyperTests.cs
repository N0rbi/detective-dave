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
        public void CharacterUnitShowsTheNextCharacterOnly()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Character);
            Assert.AreEqual(1, typer.CurrentUnitEnd);
            TypeRequired(typer, "hi");
            Assert.AreEqual(4, typer.CurrentUnitEnd);
        }

        [Test]
        public void WordUnitEndsAfterTheSpaceFollowingTheWord()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            Assert.AreEqual(3, typer.CurrentUnitEnd);
            TypeRequired(typer, "h");
            Assert.AreEqual(3, typer.CurrentUnitEnd);
        }

        [Test]
        public void TheSpaceAfterTheLastLetterOfAWordIsAlreadyTyped()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "hi");
            Assert.AreEqual(3, typer.TypedCount);
            Assert.AreEqual(10, typer.CurrentUnitEnd);
        }

        [Test]
        public void PressingSpaceIsNeverNeededAndDoesNothing()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "hi");
            Assert.IsFalse(typer.TryType(' '));
            Assert.AreEqual(3, typer.TypedCount);
        }

        [Test]
        public void SpacesAtTheEndOfTheTextDoNotBlockCompletion()
        {
            var typer = new StoryTyper("ab  \n", TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "ab");
            Assert.IsTrue(typer.IsComplete);
        }

        [Test]
        public void WordUnitKeepsTheTrailingPunctuationWithItsWord()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "hithere");
            Assert.AreEqual(10, typer.TypedCount);
            Assert.AreEqual(14, typer.CurrentUnitEnd);
        }

        [Test]
        public void SentenceUnitEndsAfterTheSentenceAndItsSpace()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Sentence);
            Assert.AreEqual(10, typer.CurrentUnitEnd);
            TypeRequired(typer, "hithere");
            Assert.AreEqual(10, typer.TypedCount);
            Assert.AreEqual(18, typer.CurrentUnitEnd);
        }

        [TestCase(RevealUnit.Character)]
        [TestCase(RevealUnit.Word)]
        [TestCase(RevealUnit.Sentence)]
        public void TheCurrentUnitEndsAtTheTextEndWhenTypedFully(RevealUnit unit)
        {
            var typer = new StoryTyper(Story, TypingRules.Default, unit);
            TypeRequired(typer, Story);
            Assert.IsTrue(typer.IsComplete);
            Assert.AreEqual(Story.Length, typer.CurrentUnitEnd);
        }

        [Test]
        public void TextWithoutSpacesIsOneWord()
        {
            var typer = new StoryTyper("abc", TypingRules.Default, RevealUnit.Word);
            Assert.AreEqual(3, typer.CurrentUnitEnd);
            TypeRequired(typer, "ab");
            Assert.AreEqual(3, typer.CurrentUnitEnd);
        }

        [Test]
        public void BackspaceBringsBackTheEarlierWord()
        {
            var typer = new StoryTyper(Story, TypingRules.Default, RevealUnit.Word);
            TypeRequired(typer, "hi ");
            Assert.AreEqual(10, typer.CurrentUnitEnd);
            typer.Backspace();
            Assert.AreEqual(3, typer.CurrentUnitEnd);
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
            Assert.AreEqual(0, typer.CurrentUnitEnd);
        }

        [Test]
        public void NewlinesBetweenSentencesAreTypedByThemselves()
        {
            var typer = new StoryTyper("One.\n\nTwo.", TypingRules.Default, RevealUnit.Sentence);
            TypeRequired(typer, "one");
            Assert.AreEqual(6, typer.TypedCount);
            Assert.AreEqual(10, typer.CurrentUnitEnd);
        }
    }
}
