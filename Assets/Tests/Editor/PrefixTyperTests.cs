using DetectiveDave.OverworldPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PrefixTyperTests
    {
        static PrefixTyper Single(string text, bool autoTypeFree = true) => new PrefixTyper(new[] { text }, new TypingRules(autoTypeFree));

        [Test]
        public void MatchingCharacterIsTyped()
        {
            var typer = Single("ab");
            Assert.IsTrue(typer.TryType('a'));
            Assert.AreEqual(1, typer.TypedCount);
        }

        [Test]
        public void WrongCharacterIsRejected()
        {
            var typer = Single("ab");
            Assert.IsFalse(typer.TryType('b'));
            Assert.AreEqual(0, typer.TypedCount);
        }

        [Test]
        public void MatchingIgnoresCase()
        {
            var typer = Single("Ab");
            Assert.IsTrue(typer.TryType('a'));
            Assert.IsTrue(typer.TryType('B'));
            Assert.IsTrue(typer.IsComplete);
        }

        [Test]
        public void FreeCharactersAreTypedByThemselves()
        {
            var typer = Single("a, b");
            typer.TryType('a');
            Assert.AreEqual(2, typer.TypedCount);
            Assert.IsTrue(typer.TryType(' '));
            Assert.AreEqual(3, typer.TypedCount);
        }

        [Test]
        public void NewlinesAreTypedByThemselves()
        {
            var typer = Single("a\n\nb");
            typer.TryType('a');
            Assert.AreEqual(3, typer.TypedCount);
        }

        [Test]
        public void FreeCharacterCannotBeTypedByHandWhenAutoTyped()
        {
            var typer = Single("a,b");
            typer.TryType('a');
            Assert.IsFalse(typer.TryType(','));
            Assert.AreEqual(2, typer.TypedCount);
        }

        [Test]
        public void FreeCharactersMustBeTypedWhenAutoTypingIsOff()
        {
            var typer = Single("a,b", false);
            typer.TryType('a');
            Assert.AreEqual(1, typer.TypedCount);
            Assert.IsTrue(typer.TryType(','));
            Assert.AreEqual(2, typer.TypedCount);
        }

        [Test]
        public void LeadingFreeCharacterIsTypedAtOnce() => Assert.AreEqual(1, Single("\"hi\"").TypedCount);

        [Test]
        public void BackspaceRemovesOneRequiredCharacter()
        {
            var typer = Single("abc");
            typer.TryType('a');
            typer.TryType('b');
            Assert.IsTrue(typer.Backspace());
            Assert.AreEqual(1, typer.TypedCount);
        }

        [Test]
        public void BackspaceRemovesTheFreeCharactersAfterIt()
        {
            var typer = Single("ab, c");
            typer.TryType('a');
            typer.TryType('b');
            Assert.AreEqual(3, typer.TypedCount);
            typer.Backspace();
            Assert.AreEqual(1, typer.TypedCount);
        }

        [Test]
        public void BackspaceOnNothingTypedDoesNothing()
        {
            var typer = Single("abc");
            Assert.IsFalse(typer.Backspace());
        }

        [Test]
        public void BackspaceKeepsTheLeadingFreeCharacter()
        {
            var typer = Single("\"hi\"");
            Assert.IsFalse(typer.Backspace());
            Assert.AreEqual(1, typer.TypedCount);
        }

        [Test]
        public void CompletingTheTextReportsItsTarget()
        {
            var typer = new PrefixTyper(new[] { "ab", "cd" }, TypingRules.Default);
            typer.TryType('c');
            typer.TryType('d');
            Assert.AreEqual(1, typer.CompletedTargetIndex);
            Assert.IsTrue(typer.IsComplete);
        }

        [Test]
        public void NothingIsTypedOnceComplete()
        {
            var typer = Single("a");
            typer.TryType('a');
            Assert.IsFalse(typer.TryType('a'));
        }

        [Test]
        public void EmptyTextIsCompleteAtOnce() => Assert.IsTrue(Single(string.Empty).IsComplete);

        [Test]
        public void NullTextCountsAsEmpty() => Assert.IsTrue(Single(null).IsComplete);

        [Test]
        public void NoTargetsIsRejected() => Assert.Throws<System.ArgumentException>(() => new PrefixTyper(new string[0], TypingRules.Default));

        [Test]
        public void TargetsThatDifferStopBeingAliveWhenTheyDiffer()
        {
            var typer = new PrefixTyper(new[] { "ab", "ac" }, TypingRules.Default);
            typer.TryType('a');
            Assert.IsTrue(typer.IsAlive(0));
            Assert.IsTrue(typer.IsAlive(1));
            typer.TryType('c');
            Assert.IsFalse(typer.IsAlive(0));
            Assert.IsTrue(typer.IsAlive(1));
        }

        [Test]
        public void FreeCharacterIsNotTypedWhileTargetsDisagreeOnIt()
        {
            var typer = new PrefixTyper(new[] { "a,b", "a;b" }, TypingRules.Default);
            typer.TryType('a');
            Assert.AreEqual(1, typer.TypedCount);
        }
    }
}
