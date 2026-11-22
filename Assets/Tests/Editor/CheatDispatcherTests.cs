using DetectiveDave.CheatPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class CheatDispatcherTests
    {
        sealed class CountingCheat : IGameCheat
        {
            public int Runs;

            public void Run() => Runs++;
        }

        static void Type(CheatDispatcher dispatcher, string letters, float startSeconds = 0f)
        {
            for (var index = 0; index < letters.Length; index++) dispatcher.OnLetterTyped(letters[index], startSeconds + index * 0.1f);
        }

        static CheatDispatcher DispatcherFor(CountingCheat cheat, bool enabled = true) =>
            new CheatDispatcher(new[] { new CheatBinding("resetplayer", cheat) }, 3f, enabled);

        [Test]
        public void ATypedCodeRunsItsCheatOnce()
        {
            var cheat = new CountingCheat();
            Type(DispatcherFor(cheat), "resetplayer");
            Assert.AreEqual(1, cheat.Runs);
        }

        [Test]
        public void ATypedCodeRunsItsCheatAgainWhenTypedAgain()
        {
            var cheat = new CountingCheat();
            var dispatcher = DispatcherFor(cheat);
            Type(dispatcher, "resetplayer");
            Type(dispatcher, "resetplayer", 5f);
            Assert.AreEqual(2, cheat.Runs);
        }

        [Test]
        public void ADisabledDispatcherRunsNothing()
        {
            var cheat = new CountingCheat();
            Type(DispatcherFor(cheat, false), "resetplayer");
            Assert.AreEqual(0, cheat.Runs);
        }

        [Test]
        public void AnotherWordRunsNothing()
        {
            var cheat = new CountingCheat();
            Type(DispatcherFor(cheat), "resetplanet");
            Assert.AreEqual(0, cheat.Runs);
        }

        [Test]
        public void EachCodeRunsItsOwnCheat()
        {
            var first = new CountingCheat();
            var second = new CountingCheat();
            var dispatcher = new CheatDispatcher(new[] { new CheatBinding("one", first), new CheatBinding("two", second) }, 3f, true);
            Type(dispatcher, "two");
            Assert.AreEqual(0, first.Runs);
            Assert.AreEqual(1, second.Runs);
        }

        [Test]
        public void BackspaceCorrectsATypo()
        {
            var cheat = new CountingCheat();
            var dispatcher = DispatcherFor(cheat);
            Type(dispatcher, "resetplx");
            dispatcher.OnBackspace(0.9f);
            Type(dispatcher, "ayer", 1f);
            Assert.AreEqual(1, cheat.Runs);
        }

        [Test]
        public void ABindingWithoutACheatIsIgnored()
        {
            var dispatcher = new CheatDispatcher(new[] { new CheatBinding("resetplayer", null) }, 3f, true);
            Assert.DoesNotThrow(() => Type(dispatcher, "resetplayer"));
        }
    }
}
