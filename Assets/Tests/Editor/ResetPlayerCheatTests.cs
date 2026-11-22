using System.Collections.Generic;
using DetectiveDave.CheatPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class ResetPlayerCheatTests
    {
        sealed class RecordingEnvironment : ICheatEnvironment
        {
            public readonly List<string> Calls = new List<string>();

            public void ForgetSolvedCases() => Calls.Add("forget");

            public void ClearCaseSession() => Calls.Add("clear");

            public void LoadOverworld() => Calls.Add("load");

            public void ShowMessage(string text) => Calls.Add("message:" + text);
        }

        [Test]
        public void ResetPlayerForgetsClearsShowsTheMessageAndLoadsTheOverworldInThatOrder()
        {
            var environment = new RecordingEnvironment();
            new ResetPlayerCheat(environment, "Progress reset").Run();
            CollectionAssert.AreEqual(new[] { "forget", "clear", "message:Progress reset", "load" }, environment.Calls);
        }

        [Test]
        public void ATypedResetPlayerCodeReachesTheEnvironment()
        {
            var environment = new RecordingEnvironment();
            var dispatcher = new CheatDispatcher(new[] { new CheatBinding("resetplayer", new ResetPlayerCheat(environment, "Progress reset")) }, 3f, true);
            foreach (var letter in "resetplayer") dispatcher.OnLetterTyped(letter, 0f);
            Assert.Contains("load", environment.Calls);
        }
    }
}
