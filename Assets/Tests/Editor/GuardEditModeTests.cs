using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public sealed class GuardEditModeTests
    {
        [Test]
        public void StartsInactiveAndIgnoresClicks()
        {
            var mode = new GuardEditMode();
            Assert.IsFalse(mode.IsActive);
            Assert.IsNull(mode.Click(false));
        }

        [Test]
        public void EnteringAGuardWithoutStartTargetsStart()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, false);
            Assert.IsTrue(mode.IsActive);
            Assert.AreEqual(PatrolTarget.Start, mode.Target);
        }

        [Test]
        public void EnteringAGuardWithStartTargetsEnd()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, true);
            Assert.AreEqual(PatrolTarget.End, mode.Target);
        }

        [Test]
        public void FirstClickPlacesStartThenEveryClickPlacesEnd()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, false);
            Assert.AreEqual(PatrolTarget.Start, mode.Click(false));
            Assert.AreEqual(PatrolTarget.End, mode.Click(false));
            Assert.AreEqual(PatrolTarget.End, mode.Click(false));
        }

        [Test]
        public void ChoosingStartMakesTheNextClickPlaceStart()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, true);
            mode.ChooseTarget(PatrolTarget.Start);
            Assert.AreEqual(PatrolTarget.Start, mode.Click(false));
            Assert.AreEqual(PatrolTarget.End, mode.Target);
        }

        [Test]
        public void ShiftClickPlacesEndWithoutChangingTargetRules()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, false);
            Assert.AreEqual(PatrolTarget.End, mode.Click(true));
            Assert.AreEqual(PatrolTarget.End, mode.Target);
        }

        [Test]
        public void SwitchingGuardResetsTheTarget()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, true);
            mode.SwitchGuard(2, false);
            Assert.AreEqual(2, mode.GuardId);
            Assert.AreEqual(PatrolTarget.Start, mode.Target);
        }

        [Test]
        public void SwitchingToTheSameGuardKeepsTheTarget()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, false);
            mode.ChooseTarget(PatrolTarget.End);
            mode.SwitchGuard(1, false);
            Assert.AreEqual(PatrolTarget.End, mode.Target);
        }

        [Test]
        public void SwitchingWhileInactiveDoesNotActivate()
        {
            var mode = new GuardEditMode();
            mode.SwitchGuard(2, false);
            Assert.IsFalse(mode.IsActive);
        }

        [Test]
        public void ExitStopsClicksAndTargetChanges()
        {
            var mode = new GuardEditMode();
            mode.Enter(1, false);
            mode.Exit();
            Assert.IsFalse(mode.IsActive);
            Assert.IsNull(mode.Click(false));
        }
    }
}
