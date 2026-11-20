using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DivergingTyperTests
    {
        const string Accept = "I am sorry.";
        const string Decline = "I cannot help.";
        const string Client = "Ann";

        DivergingTyper typer;

        [SetUp]
        public void SetUp() => typer = Build(TypingRules.Default);

        static DivergingTyper Build(TypingRules rules) =>
            new DivergingTyper(LetterText.Compose(Client, Accept), LetterText.Compose(Client, Decline), rules);

        void TypeRequired(string text)
        {
            foreach (var character in text)
            {
                if (TypingRules.IsFreeCharacter(character)) continue;
                Assert.IsTrue(typer.TryType(character), "could not type '" + character + "'");
            }
        }

        [Test]
        public void StartsUndecidedWithNothingToShow()
        {
            Assert.AreEqual(LetterPath.Undecided, typer.Path);
            Assert.AreEqual(LetterOutcome.None, typer.Outcome);
        }

        [Test]
        public void TypingTheSharedHeadLeavesBothPathsOpen()
        {
            TypeRequired("Dear Ann,");
            Assert.AreEqual(LetterText.Head(Client).Length, typer.TypedCount);
            Assert.AreEqual(LetterPath.Undecided, typer.Path);
        }

        [Test]
        public void SharedStartOfBothMessagesLeavesBothPathsOpen()
        {
            TypeRequired("Dear Ann,I ");
            Assert.AreEqual(LetterPath.Undecided, typer.Path);
        }

        [Test]
        public void TypingCLocksTheDeclinePath()
        {
            TypeRequired("Dear Ann,I ");
            Assert.IsTrue(typer.TryType('c'));
            Assert.AreEqual(LetterPath.Decline, typer.Path);
        }

        [Test]
        public void TypingALocksTheAcceptPath()
        {
            TypeRequired("Dear Ann,I ");
            Assert.IsTrue(typer.TryType('a'));
            Assert.AreEqual(LetterPath.Accept, typer.Path);
        }

        [Test]
        public void KeyOfTheOtherPathIsIgnoredOnceLocked()
        {
            TypeRequired("Dear Ann,I c");
            int typed = typer.TypedCount;
            Assert.IsFalse(typer.TryType('m'));
            Assert.AreEqual(typed, typer.TypedCount);
        }

        [Test]
        public void RemovingTheCReopensBothPaths()
        {
            TypeRequired("Dear Ann,I c");
            Assert.IsTrue(typer.Backspace());
            Assert.AreEqual(LetterPath.Undecided, typer.Path);
            Assert.IsTrue(typer.TryType('a'));
            Assert.AreEqual(LetterPath.Accept, typer.Path);
        }

        [Test]
        public void UnrelatedKeyDoesNothing()
        {
            Assert.IsFalse(typer.TryType('z'));
            Assert.AreEqual(0, typer.TypedCount);
        }

        [Test]
        public void CaseDoesNotMatter()
        {
            Assert.IsTrue(typer.TryType('D'));
            Assert.IsTrue(typer.TryType('E'));
        }

        [Test]
        public void TypingTheWholeAcceptLetterAccepts()
        {
            TypeRequired(LetterText.Compose(Client, Accept));
            Assert.AreEqual(LetterOutcome.Accepted, typer.Outcome);
        }

        [Test]
        public void TypingTheWholeDeclineLetterDeclines()
        {
            TypeRequired(LetterText.Compose(Client, Decline));
            Assert.AreEqual(LetterOutcome.Declined, typer.Outcome);
        }

        [Test]
        public void NothingMoreIsTypedAfterTheLetterEnds()
        {
            TypeRequired(LetterText.Compose(Client, Accept));
            Assert.IsFalse(typer.TryType('d'));
        }

        [Test]
        public void BackspaceAfterTheEndReopensTheLetter()
        {
            TypeRequired(LetterText.Compose(Client, Accept));
            typer.Backspace();
            Assert.AreEqual(LetterOutcome.None, typer.Outcome);
        }

        [Test]
        public void FreeCharactersMustBeTypedWhenAutoTypingIsOff()
        {
            typer = Build(new TypingRules(false));
            foreach (var character in "Dear Ann,\n\n") Assert.IsTrue(typer.TryType(character));
            Assert.AreEqual(LetterText.Head(Client).Length, typer.TypedCount);
        }

        [Test]
        public void DivergenceIndexIsWhereTheTextsDiffer() => Assert.AreEqual(2, DivergingTyper.DivergenceIndex("I am", "I cannot"));

        [Test]
        public void DivergenceIndexIgnoresCase() => Assert.AreEqual(4, DivergingTyper.DivergenceIndex("I AM", "i am"));

        [Test]
        public void DivergenceIndexOfPrefixIsTheShorterLength() => Assert.AreEqual(4, DivergingTyper.DivergenceIndex("I am", "I am sorry"));

        [Test]
        public void MessagesThatDifferDiverge() => Assert.IsTrue(DivergingTyper.Diverge(Accept, Decline));

        [Test]
        public void IdenticalMessagesDoNotDiverge() => Assert.IsFalse(DivergingTyper.Diverge(Accept, Accept));

        [Test]
        public void MessageThatIsAPrefixOfTheOtherDoesNotDiverge() => Assert.IsFalse(DivergingTyper.Diverge("I am", "I am sorry"));

        [TestCase("", "x")]
        [TestCase("x", "")]
        [TestCase(null, "x")]
        public void EmptyMessagesDoNotDiverge(string accept, string decline) => Assert.IsFalse(DivergingTyper.Diverge(accept, decline));
    }
}
