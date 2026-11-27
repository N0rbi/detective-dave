using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class BassPulseTests
    {
        const int LoopBeats = 8;
        const double LoopSeconds = 8.0;

        static BassPulseClock Clock(params float[] beats) => new BassPulseClock(beats, LoopBeats, LoopSeconds);

        [Test]
        public void ANoteInsideTheSpanIsCounted() => Assert.AreEqual(1, Clock(2f).CountBetween(1.5, 2.5));

        [Test]
        public void ANoteAtTheStartOfTheSpanWasAlreadyCounted() => Assert.AreEqual(0, Clock(2f).CountBetween(2.0, 2.5));

        [Test]
        public void ANoteAtTheEndOfTheSpanIsCounted() => Assert.AreEqual(1, Clock(2f).CountBetween(1.5, 2.0));

        [Test]
        public void ANoteOnTheWrapIsCountedOnceWhenTheLoopRestarts() => Assert.AreEqual(1, Clock(0f).CountBetween(7.9, 0.1));

        [Test]
        public void NotesBeforeAndAfterTheWrapAreBothCounted() => Assert.AreEqual(2, Clock(7.5f, 0.25f).CountBetween(7.0, 0.5));

        [Test]
        public void NoTimePassedCountsNothing() => Assert.AreEqual(0, Clock(0f, 2f).CountBetween(2.0, 2.0));

        [Test]
        public void SecondsPerBeatFollowTheLoopLength() => Assert.AreEqual(1, new BassPulseClock(new[] { 1f }, 4, 2.0).CountBetween(0.4, 0.6));

        [Test]
        public void ThePulseReportsNothingOnItsFirstPoll()
        {
            var now = 3.0;
            var pulse = new BassPulseSource(Clock(2f, 4f), () => now);
            Assert.AreEqual(0, pulse.Poll());
            now = 4.5;
            Assert.AreEqual(1, pulse.Poll());
            Assert.AreEqual(0, pulse.Poll());
        }

        [Test]
        public void AClockWithoutNotesIsNotAvailable() => Assert.IsFalse(new BassPulseSource(Clock(), () => 0.0).IsAvailable);

        [Test]
        public void SecondsBecomeQuantizedBeats() =>
            Assert.AreEqual(new[] { 3f, 4f, 5f }, BassOnsetBeats.ToLoopBeats(new[] { 2.355f, 3.153f, 3.956f }, 0.8, 0f, 32, 0.25f));

        [Test]
        public void NotesPastTheLoopWrapAround() =>
            Assert.AreEqual(new[] { 0.5f }, BassOnsetBeats.ToLoopBeats(new[] { 8.4f }, 1.0, 0f, 8, 0.25f));

        [Test]
        public void ANoteRoundedToTheLoopEndBecomesTheLoopStart() =>
            Assert.AreEqual(new[] { 0f }, BassOnsetBeats.ToLoopBeats(new[] { 7.95f }, 1.0, 0f, 8, 0.25f));

        [Test]
        public void NotesThatRoundToTheSameBeatAreKeptOnce() =>
            Assert.AreEqual(new[] { 2f }, BassOnsetBeats.ToLoopBeats(new[] { 1.97f, 2.04f }, 1.0, 0f, 8, 0.25f));

        [Test]
        public void TheStartOffsetShiftsEveryNote() =>
            Assert.AreEqual(new[] { 1f, 3f }, BassOnsetBeats.ToLoopBeats(new[] { 2f, 0f }, 1.0, 1f, 8, 0.25f));

        [Test]
        public void ANegativeOffsetWrapsBackwards() =>
            Assert.AreEqual(new[] { 7f }, BassOnsetBeats.ToLoopBeats(new[] { 0f }, 1.0, -1f, 8, 0.25f));

        [Test]
        public void ABassFileWithinABeatOfTheLoopFits()
        {
            Assert.IsTrue(BassOnsetBeats.DurationFitsLoop(26.015, 25.6, 0.8));
            Assert.IsFalse(BassOnsetBeats.DurationFitsLoop(28.0, 25.6, 0.8));
        }

        [Test]
        public void TheAudioTempoMakesTheFileTheLoop() => Assert.AreEqual(0.8125, BassOnsetBeats.SecondsPerBeatForDuration(26.0, 32), 1e-9);
    }
}
