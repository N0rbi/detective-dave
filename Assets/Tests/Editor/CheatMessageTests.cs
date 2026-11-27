using DetectiveDave.CheatPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class CheatMessageTests
    {
        [Test]
        public void ThereIsNoMessageBeforeOneIsShown()
        {
            Assert.IsFalse(new CheatMessage().TryGet(0f, out _));
        }

        [Test]
        public void AMessageShowsForItsDuration()
        {
            var message = new CheatMessage();
            message.Show("Progress reset", 10f, 2f);
            Assert.IsTrue(message.TryGet(11.9f, out var text));
            Assert.AreEqual("Progress reset", text);
        }

        [Test]
        public void AMessageHidesWhenItsDurationIsOver()
        {
            var message = new CheatMessage();
            message.Show("Progress reset", 10f, 2f);
            Assert.IsFalse(message.TryGet(12f, out _));
        }

        [Test]
        public void ANewMessageReplacesTheOldOne()
        {
            var message = new CheatMessage();
            message.Show("first", 0f, 2f);
            message.Show("second", 1f, 2f);
            Assert.IsTrue(message.TryGet(2.5f, out var text));
            Assert.AreEqual("second", text);
        }
    }
}
