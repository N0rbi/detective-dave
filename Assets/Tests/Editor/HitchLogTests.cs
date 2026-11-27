using DetectiveDave.Diagnostics;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class HitchLogTests
    {
        [Test]
        public void ThresholdZeroTurnsTheLogOff()
        {
            Assert.IsFalse(HitchDetector.IsEnabled(0f));
            Assert.IsFalse(HitchDetector.IsHitch(500f, 0f));
        }

        [Test]
        public void OnlyAFrameLongerThanTheThresholdIsAHitch()
        {
            Assert.IsFalse(HitchDetector.IsHitch(50f, 50f));
            Assert.IsTrue(HitchDetector.IsHitch(50.1f, 50f));
        }

        [Test]
        public void ReportNamesTheFrameTheTimeAndEveryAudioEvent()
        {
            var events = new[]
            {
                new AudioEvent(41, AudioEventKind.ClipLoadedOnPlay, "Jazz - Piano - 3", 0f),
                new AudioEvent(41, AudioEventKind.PianoNote, "Jazz - Piano - 3", 0.8f)
            };
            var text = HitchReport.Format(41, 120.55f, 50f, 2, events);
            StringAssert.Contains("frame 41 took 120.6 ms", text);
            StringAssert.Contains("2 garbage collections", text);
            StringAssert.Contains("clip 'Jazz - Piano - 3' was not loaded when it had to play", text);
            StringAssert.Contains("piano note 'Jazz - Piano - 3' at volume 0.800", text);
        }

        [Test]
        public void ReportSaysWhenNoAudioEventHappened()
        {
            var text = HitchReport.Format(7, 80f, 50f, 0, new AudioEvent[0]);
            StringAssert.Contains("the audio is not the cause", text);
        }

        [Test]
        public void ReportDescribesSeeksAndStemStarts()
        {
            Assert.AreEqual("seek of 'hook' to 12.345 s", AudioEventText.Of(new AudioEvent(1, AudioEventKind.LoopSeek, "hook", 12.345f)));
            Assert.AreEqual("stem start 'loop' at 0.000 s", AudioEventText.Of(new AudioEvent(1, AudioEventKind.StemStart, "loop", 0f)));
        }
    }
}
