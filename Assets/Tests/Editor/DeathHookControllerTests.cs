using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class DeathHookControllerTests
    {
        sealed class FakeOutput : IDeathHookOutput
        {
            public bool HasClip { get; set; } = true;
            public DeathHookMix Mix = DeathHookMix.Neutral;

            public void Apply(DeathHookMix mix) => Mix = mix;
        }

        const float Tolerance = 0.001f;

        FakeOutput output;
        float now;

        DeathHookController Create(DeathHookSettings settings = null)
        {
            output = new FakeOutput();
            now = 0f;
            return new DeathHookController(settings ?? new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0f), output, () => now);
        }

        void Advance(DeathHookController hook, float seconds)
        {
            now += seconds;
            hook.Tick();
        }

        [Test]
        public void ItStartsIdleAndTickLeavesTheMixNeutral()
        {
            var hook = Create();
            Advance(hook, 1f);
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(0f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void ADeathFadesTheHookUpAndTheMusicDownOverTheFadeInTime()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.125f);
            Assert.AreEqual(DeathHookState.FadingIn, hook.State);
            Assert.AreEqual(0.45f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(0.5f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0.5f, output.Mix.CaseMusicShare, Tolerance);
        }

        [Test]
        public void AfterTheFadeInTheHookIsAtItsVolumeAndTheMusicIsDown()
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
        public void AShareAboveZeroKeepsThatShareOfTheLoopAndTheCaseMusic()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0.3f, 0.1f, false, 0f));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            Assert.AreEqual(0.3f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0.1f, output.Mix.CaseMusicShare, Tolerance);
        }

        [Test]
        public void ARespawnFadesTheHookBackToSilenceAndTheMusicBackUp()
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
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0f, output.Mix.HookVolume, Tolerance);
        }

        [Test]
        public void TheHookStaysUpWhileTheMusicIsDownUntilTheRespawn()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 30f);
            Assert.AreEqual(DeathHookState.Playing, hook.State);
            Assert.AreEqual(0.9f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(0f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void TheStartDelayHoldsTheFadeBack()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 0.5f));
            hook.PlayerDied();
            Advance(hook, 0.4f);
            Assert.AreEqual(0f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
            Advance(hook, 0.35f);
            Assert.AreEqual(0.5f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void ARespawnBeforeTheDelayEndsChangesNothing()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, false, 2f));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerRespawned();
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(0f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void ASecondDeathWhileFadingOutFadesUpFromWhereItIs()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerRespawned();
            Advance(hook, 0.5f);
            hook.PlayerDied();
            Advance(hook, 0.025f);
            Assert.AreEqual(DeathHookState.FadingIn, hook.State);
            Assert.AreEqual(0.54f, output.Mix.HookVolume, Tolerance);
        }

        [Test]
        public void ASecondDeathWhilePlayingChangesNothing()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.PlayerDied();
            Advance(hook, 0.1f);
            Assert.AreEqual(DeathHookState.Playing, hook.State);
            Assert.AreEqual(0.9f, output.Mix.HookVolume, Tolerance);
        }

        [Test]
        public void StopRestoresEverythingAtOnce()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.Stop();
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(0f, output.Mix.HookVolume, Tolerance);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(1f, output.Mix.CaseMusicShare, Tolerance);
        }

        [Test]
        public void ADeathAfterStopFadesUpAgain()
        {
            var hook = Create();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            hook.Stop();
            hook.PlayerDied();
            Advance(hook, 0.5f);
            Assert.AreEqual(0.9f, output.Mix.HookVolume, Tolerance);
        }

        [Test]
        public void WithoutAClipADeathChangesNothing()
        {
            var hook = Create();
            output.HasClip = false;
            hook.PlayerDied();
            Advance(hook, 1f);
            Assert.AreEqual(DeathHookState.Idle, hook.State);
            Assert.AreEqual(1f, output.Mix.LoopShare, Tolerance);
        }

        [Test]
        public void ARespawnWithoutADeathDoesNothing()
        {
            var hook = Create();
            hook.PlayerRespawned();
            Assert.AreEqual(DeathHookState.Idle, hook.State);
        }

        [Test]
        public void ZeroFadesSwitchAtOnce()
        {
            var hook = Create(new DeathHookSettings(0f, 0f, 0.9f, 0f, 0f, false, 0f));
            hook.PlayerDied();
            Advance(hook, 0.01f);
            Assert.AreEqual(DeathHookState.Playing, hook.State);
            hook.PlayerRespawned();
            Advance(hook, 0.01f);
            Assert.AreEqual(DeathHookState.Idle, hook.State);
        }

        [Test]
        public void TheLoopTheCaseMusicAndTheHookDucksMultiply()
        {
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.8f, 0.5f, 0.25f, false, 0f));
            hook.PlayerDied();
            Advance(hook, 0.5f);
            const float loopBase = 0.8f;
            const float caseDuck = 0.4f;
            Assert.AreEqual(0.16f, loopBase * caseDuck * output.Mix.LoopShare, Tolerance);
            Assert.AreEqual(0.8f, output.Mix.HookVolume, Tolerance);
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
            var hook = Create(new DeathHookSettings(0.25f, 1f, 0.9f, 0f, 0f, true, 0f));
            hook.PlayerDied();
            Assert.IsTrue(hook.PianoAllowed);
        }
    }
}
