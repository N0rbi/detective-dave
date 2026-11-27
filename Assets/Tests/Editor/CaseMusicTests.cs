using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class CaseMusicTests
    {
        sealed class FakePlayer : ICaseMusicPlayer
        {
            public CaseMusic Faded;
            public int FadeOuts;
            public int Stops;
            public void FadeIn(CaseMusic music) => Faded = music;
            public void FadeOut() => FadeOuts++;
            public void StopNow() => Stops++;
        }

        static CaseMusic WithClip(float seconds, bool sync = true) =>
            new CaseMusic(AudioClip.Create("case music", (int)(seconds * 1000), 1, 1000, false), 1.5f, 2f, 0.8f, 0.3f, sync);

        [Test]
        public void ACaseWithMusicFadesItIn()
        {
            var player = new FakePlayer();
            var music = WithClip(10f);
            new CaseMusicCues(_ => music, () => player).CaseOpened("a");
            Assert.AreSame(music, player.Faded);
        }

        [Test]
        public void ACaseWithoutAClipFadesOutWhateverPlays()
        {
            var player = new FakePlayer();
            new CaseMusicCues(_ => new CaseMusic(), () => player).CaseOpened("a");
            Assert.IsNull(player.Faded);
            Assert.AreEqual(1, player.FadeOuts);
        }

        [Test]
        public void EndingTheCaseFadesOut()
        {
            var player = new FakePlayer();
            new CaseMusicCues(_ => null, () => player).CaseEnded();
            Assert.AreEqual(1, player.FadeOuts);
        }

        [Test]
        public void TheMusicStartsAtTheLoopPosition()
        {
            Assert.AreEqual(5.0, LoopFollower.WantedClipTime(30.0, 25.0, 26.0), 1e-9);
        }

        [Test]
        public void ALongerClipIsCutAtTheLoopLength()
        {
            Assert.AreEqual(1.0, LoopFollower.WantedClipTime(26.0, 25.6, 26.0), 1e-9);
        }

        [Test]
        public void SmallDriftIsLeftAlone()
        {
            Assert.IsFalse(LoopFollower.NeedsCorrection(10.0, 10.03, 25.6, 0.04));
        }

        [Test]
        public void BigDriftIsCorrected()
        {
            Assert.IsTrue(LoopFollower.NeedsCorrection(10.0, 10.2, 25.6, 0.04));
        }

        [Test]
        public void DriftAcrossTheWrapIsNotDrift()
        {
            Assert.IsFalse(LoopFollower.NeedsCorrection(25.59, 0.01, 25.6, 0.04));
        }

        [Test]
        public void ALongClipIsReported()
        {
            Assert.IsNotNull(CaseMusicCheck.Problem(WithClip(30f), 25.6));
        }

        [Test]
        public void AClipSlightlyLongerIsFine()
        {
            Assert.IsNull(CaseMusicCheck.Problem(WithClip(26f), 25.6));
        }

        [Test]
        public void AnUnsyncedLongClipIsFine()
        {
            Assert.IsNull(CaseMusicCheck.Problem(WithClip(60f, false), 25.6));
        }
    
        [Test]
        public void HookStemStartsAtTheLoopPhaseWrappedAtTheLoopLengthNotTheHookLength()
        {
            Assert.AreEqual(0.4, LoopFollower.WantedClipTime(26.0, 25.6, 26.0, 0.0), 1e-9);
            Assert.AreEqual(12.8, LoopFollower.WantedClipTime(12.8, 25.6, 26.0, 0.0), 1e-9);
        }

        [Test]
        public void HookStartTimeAddsTheOffsetAndWrapsBothWays()
        {
            Assert.AreEqual(0.1, LoopFollower.WantedClipTime(25.5, 25.6, 26.0, 0.2), 1e-9);
            Assert.AreEqual(25.5, LoopFollower.WantedClipTime(0.1, 25.6, 26.0, -0.2), 1e-9);
        }

        [Test]
        public void HookDriftAcrossTheWrapIsNotACorrection()
        {
            Assert.IsFalse(LoopFollower.NeedsCorrection(25.59, 0.01, 25.6, 0.04));
            Assert.IsTrue(LoopFollower.NeedsCorrection(10.0, 10.1, 25.6, 0.04));
        }
}
}
