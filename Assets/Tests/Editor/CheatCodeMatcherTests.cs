using DetectiveDave.CheatPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class CheatCodeMatcherTests
    {
        const float Timeout = 3f;

        static string Type(CheatCodeMatcher matcher, string letters, float startSeconds = 0f, float stepSeconds = 0.1f)
        {
            string last = null;
            for (var index = 0; index < letters.Length; index++)
                last = matcher.TypeLetter(letters[index], startSeconds + index * stepSeconds) ?? last;
            return last;
        }

        [Test]
        public void TheExactCodeMatches()
        {
            var matcher = new CheatCodeMatcher(new[] { "resetplayer" }, Timeout);
            Assert.AreEqual("resetplayer", Type(matcher, "resetplayer"));
        }

        [Test]
        public void NoMatchBeforeTheLastLetter()
        {
            var matcher = new CheatCodeMatcher(new[] { "resetplayer" }, Timeout);
            Assert.IsNull(Type(matcher, "resetplaye"));
        }

        [Test]
        public void UppercaseLettersMatch()
        {
            var matcher = new CheatCodeMatcher(new[] { "resetplayer" }, Timeout);
            Assert.AreEqual("resetplayer", Type(matcher, "RESETPLAYER"));
        }

        [Test]
        public void ACodeInTheConfigurationWithCapitalsMatchesLowercaseTyping()
        {
            var matcher = new CheatCodeMatcher(new[] { "ResetPlayer" }, Timeout);
            Assert.AreEqual("resetplayer", Type(matcher, "resetplayer"));
        }

        [Test]
        public void LettersBeforeTheCodeDoNotPreventTheMatch()
        {
            var matcher = new CheatCodeMatcher(new[] { "resetplayer" }, Timeout);
            Assert.AreEqual("resetplayer", Type(matcher, "xyzresetplayer"));
        }

        [Test]
        public void AMismatchInTheMiddleStartsAgain()
        {
            var matcher = new CheatCodeMatcher(new[] { "resetplayer" }, Timeout);
            Assert.IsNull(Type(matcher, "resetpxlayer"));
            Assert.AreEqual("resetplayer", Type(matcher, "resetplayer", 10f));
        }

        [Test]
        public void ARestartInsideTheBufferMatchesOverlappingPrefixes()
        {
            var matcher = new CheatCodeMatcher(new[] { "aab" }, Timeout);
            Assert.AreEqual("aab", Type(matcher, "aaab"));
        }

        [Test]
        public void TheBufferIsClearedAfterAMatch()
        {
            var matcher = new CheatCodeMatcher(new[] { "go" }, Timeout);
            Assert.AreEqual("go", Type(matcher, "go"));
            Assert.AreEqual(string.Empty, matcher.TypedSoFar);
        }

        [Test]
        public void TheSameCodeMatchesAgainAfterAMatch()
        {
            var matcher = new CheatCodeMatcher(new[] { "go" }, Timeout);
            Type(matcher, "go");
            Assert.AreEqual("go", Type(matcher, "go", 1f));
        }

        [Test]
        public void AFarLaterLetterForgetsTheLettersBeforeIt()
        {
            var matcher = new CheatCodeMatcher(new[] { "go" }, Timeout);
            matcher.TypeLetter('g', 0f);
            Assert.IsNull(matcher.TypeLetter('o', 3.5f));
        }

        [Test]
        public void AShortPauseKeepsTheLettersTypedSoFar()
        {
            var matcher = new CheatCodeMatcher(new[] { "go" }, Timeout);
            matcher.TypeLetter('g', 0f);
            Assert.AreEqual("go", matcher.TypeLetter('o', 2.9f));
        }

        [Test]
        public void BackspaceRemovesTheLastLetter()
        {
            var matcher = new CheatCodeMatcher(new[] { "go" }, Timeout);
            matcher.TypeLetter('g', 0f);
            matcher.TypeLetter('x', 0.1f);
            matcher.Backspace(0.2f);
            Assert.AreEqual("go", matcher.TypeLetter('o', 0.3f));
        }

        [Test]
        public void BackspaceOnAnEmptyBufferDoesNothing()
        {
            var matcher = new CheatCodeMatcher(new[] { "go" }, Timeout);
            matcher.Backspace(0f);
            Assert.AreEqual("go", Type(matcher, "go", 0.1f));
        }

        [Test]
        public void BackspaceCountsAsActivityForTheTimeout()
        {
            var matcher = new CheatCodeMatcher(new[] { "go" }, Timeout);
            matcher.TypeLetter('g', 0f);
            matcher.TypeLetter('x', 2.9f);
            matcher.Backspace(5.8f);
            Assert.AreEqual("go", matcher.TypeLetter('o', 8.7f));
        }

        [Test]
        public void TheLongestMatchingCodeWins()
        {
            var matcher = new CheatCodeMatcher(new[] { "bc", "abc" }, Timeout);
            Assert.AreEqual("abc", Type(matcher, "abc"));
        }

        [Test]
        public void SeveralCodesAreEachMatched()
        {
            var matcher = new CheatCodeMatcher(new[] { "one", "two" }, Timeout);
            Assert.AreEqual("one", Type(matcher, "one"));
            Assert.AreEqual("two", Type(matcher, "two", 1f));
        }

        [Test]
        public void NoCodesNeverMatch()
        {
            var matcher = new CheatCodeMatcher(new string[0], Timeout);
            Assert.IsNull(Type(matcher, "resetplayer"));
        }

        [Test]
        public void EmptyCodesAreIgnored()
        {
            var matcher = new CheatCodeMatcher(new[] { "", null, "go" }, Timeout);
            Assert.AreEqual("go", Type(matcher, "go"));
        }
    }
}
