using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DeathHookControllerTests
    {
        sealed class FakeOutput : IDeathHookOutput
        {
            public bool HasClip { get; set; } = true;
            public bool HookIsPlaying { get; set; }
            public int Starts;
            public int Stops;
            public bool LastLoop;
            public DeathHookMix Mix = DeathHookMix.Neutral;

            public void StartHook(bool loop)
            {
                Starts++;
                LastLoop = loop;
                HookIsPlaying = true;
            }

            public void StopHook()
            {
                Stops++;
                HookIsPlaying = false;
            }

            public void Apply(DeathHookMix mix) => Mix = mix;
        }

        const float Tolerance = 0.001f;

        FakeOutput output;
        float now;

        DeathHookController Create(DeathHookSettings settings = null)
        {
            output = new FakeOutput();
            now = 0f;
            return new DeathHookController(settings ?? new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0f, true, true), output, () => now);
        }

        void Advance(DeathHookController hook, float seconds)
        {
            now += seconds;
            hook.Tick();
        }

        [Test]
        public void ItStartsIdleAndTickDoesNothing()
        {
            var hook = Create();
            Advance(hook, 1f);
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(0, output.Starts);
        }

        [Test]
        public void ADeathStartsTheHookAndFadesItInOverTheFadeInTime()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.125f);
            Assert.AreEqual(DeathHookState.FadingIn, hook.State);
            Assert.AreEqual(1, output.Starts);
            Assert.AreEqual(0.45f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(0.5f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0.5f, output.Mix.CaseMusicShare, Tolerance);
        }

        [Test]
        public void AfterTheFadeInTheHookPlaysAtItsVolumeAndTheMusicIsDown()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.3f);
            Assert.AreEqual(DeathHookState.Playing, hook.State);
            Assert.AreEqual(0.9f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(0f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0f, output.Mix.CaseMusicShare, Tolerance);
        }

        [Test]
        public void ALoopShareAboveZeroKeepsThatShareOfTheLoop()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0.3f, 0.1f, false, 0f, true, true));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            Assert.AreEqual(0.3f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0.1f, output.Mix.CaseMusicShare, Tolerance);
        }

        [Test]
        public void ARespawnFadesTheHookOutAndTheMusicBackUp()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerRespawned();
            Advance(hook, 0.5f);
            Assert.AreEqual(DeathHookState.FadingOut, hook.State);
            Assert.AreEqual(0.45f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(0.5f, output.Mix.LoopShare, Tolerance);
            Advance(hook, 0.6f);
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(1, output.Stops);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0f, output.Mix.HookVolume, Tolerance);
        }

        [Test]
        public void AHookThatPlaysOnceWaitsForTheRespawnInSilenceWithTheMusicStillDown()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            output.HookIsPlaying = false;
            Advance(hook, 0.1f);
            Assert.AreEqual(DeathHookState.WaitingForRespawn, hook.State);
            Assert.AreEqual(0f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void AHookThatLoopsKeepsPlayingWhileTheClipIsStillGoing()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0f, false, true));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            Advance(hook, 5f);
            Assert.IsTrue(output.LastLoop);
            Assert.AreEqual(DeathHookState.Playing, hook.State);
        }

        [Test]
        public void TheStartDelayHoldsTheHookAndTheMusicBack()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0.5f, true, true));
            hook.PlayerDied();
            Advance(hook, 0.4f);
            Assert.AreEqual(0, output.Starts);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
            Advance(hook, 0.35f);
            Assert.AreEqual(1, output.Starts);
            Assert.AreEqual(0.5f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void ARespawnBeforeTheDelayEndsNeverStartsTheHook()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 2f, true, true));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerRespawned();
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(0, output.Starts);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void ASecondDeathRestartsTheHookWhenAsked()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerRespawned();
            Advance(hook, 0.3f);
            hook.PlayerDied();
            Advance(hook, 0.01f);
            Assert.AreEqual(2, output.Starts);
            Assert.AreEqual(DeathHookState.FadingIn, hook.State);
        }

        [Test]
        public void ASecondDeathKeepsTheSoundingHookWhenRestartIsOff()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0f, true, false));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerRespawned();
            Advance(hook, 0.3f);
            hook.PlayerDied();
            Advance(hook, 0.3f);
            Assert.AreEqual(1, output.Starts);
            Assert.AreEqual(DeathHookState.Playing, hook.State);
        }

        [Test]
        public void StopRestoresEverythingAtOnce()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.Stop();
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(1, output.Stops);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(1f, output.Mix.CaseMusicShare, Tolerance);
        }

        [Test]
        public void WithoutAClipADeathChangesNothing()
        {
            var hook = Create();
            output.HasClip = false;
            hook.PlayerDied();
            Advance(hook, 1f);
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(0, output.Starts);
        }

        [Test]
        public void ARespawnWithoutADeathDoesNothing()
        {
            var hook = Create();
            hook.PlayerRespawned();
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(0, output.Stops);
        }

        [Test]
        public void ZeroFadesSwitchAtOnce()
        {
            var hook = Create(new DeathHookSettings(0f, 0f, 0.9f, 0f, 0f, false, 0f, true, true));
            hook.PlayerDied();
            Advance(hook, 0.01f);
            Assert.AreEqual(DeathHookState.Playing, hook.State);
            hook.PlayerRespawned();
            Advance(hook, 0.01f);
            Assert.AreEqual(DeathHookState.Idle, hook.State);
        }

        [Test]
        public void PianoIsMutedFromTheDeathUntilTheRespawnFadeOutStarts()
        {
            var hook = Create();
            Assert.IsTrue(hook.PianoAllowed);
            hook.PlayerDied();
            Assert.IsFalse(hook.PianoAllowed);
            Advance(hook, 0.5f);
            Assert.IsFalse(hook.PianoAllowed);
            hook.PlayerRespawned();
            Assert.IsTrue(hook.PianoAllowed);
        }

        [Test]
        public void PianoWhileTheHookPlaysCanBeAllowed()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, true, 0f, true, true));
            hook.PlayerDied();
            Assert.IsTrue(hook.PianoAllowed);
        }
    
        [Test]
        public void SyncedHookLoopsWhileWaitingEvenWhenPlayHookOnce()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0f, true, true, true, 0f, true));
            hook.PlayerDied();
            Advance(hook, 0.1f);
            Assert.IsTrue(output.LastLoop);
        }

        [Test]
        public void SyncedHookPlaysOnceWhenLoopWhileWaitingIsOff()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0f, true, true, true, 0f, false));
            hook.PlayerDied();
            Advance(hook, 0.1f);
            Assert.IsFalse(output.LastLoop);
        }

        [Test]
        public void ASecondDeathWhileSyncedStartsTheHookAgain()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0f, true, true, true, 0f, true));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerDied();
            Advance(hook, 0.1f);
            Assert.AreEqual(2, output.Starts);
        }
}
}
