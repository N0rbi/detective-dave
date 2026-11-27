using DetectiveDave.Diagnostics;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class AudioEventLogTests
    {
        int frame;
        AudioEventLog log;

        [SetUp]
        public void SetUp()
        {
            frame = 10;
            log = new AudioEventLog(3, () => frame);
        }

        [Test]
        public void ReturnsTheEventsOfTheAskedFramesOldestFirst()
        {
            log.Record(AudioEventKind.PianoNote, "a", 0.5f);
            frame = 11;
            log.Record(AudioEventKind.LoopSeek, "b", 1f);
            frame = 12;
            log.Record(AudioEventKind.StemStart, "c", 0f);
            var found = log.Between(10, 11);
            Assert.AreEqual(2, found.Count);
            Assert.AreEqual("a", found[0].ClipName);
            Assert.AreEqual("b", found[1].ClipName);
        }

        [Test]
        public void ForgetsTheOldestEventWhenFull()
        {
            log.Record(AudioEventKind.PianoNote, "a", 0f);
            log.Record(AudioEventKind.PianoNote, "b", 0f);
            log.Record(AudioEventKind.PianoNote, "c", 0f);
            log.Record(AudioEventKind.PianoNote, "d", 0f);
            var found = log.Between(0, 100);
            Assert.AreEqual(3, found.Count);
            Assert.AreEqual("b", found[0].ClipName);
            Assert.AreEqual("d", found[2].ClipName);
        }

        [Test]
        public void ReturnsNothingForFramesWithoutEvents()
        {
            log.Record(AudioEventKind.PianoNote, "a", 0f);
            Assert.IsEmpty(log.Between(11, 12));
        }

        [Test]
        public void NeedsRoomForAtLeastOneEvent()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new AudioEventLog(0, () => 0));
        }
    }
}
