using DetectiveDave.PlayerPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class PlayerStateMachineTests
    {
        PlayerStateMachine machine;

        [SetUp]
        public void CreateMachine() => machine = new PlayerStateMachine();

        [Test]
        public void BeginLeavesNotStarted()
        {
            machine.Begin();
            Assert.IsTrue(machine.AcceptsMoves);
        }

        [Test]
        public void BeginDoesNothingOnceStarted()
        {
            machine.Begin();
            machine.Enter(PlayerState.Moving);
            machine.Begin();
            Assert.AreEqual(PlayerState.Moving, machine.State);
        }

        [Test]
        public void OnlyIdleAcceptsMoves()
        {
            machine.Begin();
            machine.Enter(PlayerState.Moving);
            Assert.IsFalse(machine.AcceptsMoves);
        }

        [Test]
        public void GoalIsTerminal()
        {
            machine.Begin();
            machine.Enter(PlayerState.Goal);
            machine.Enter(PlayerState.Idle);
            Assert.AreEqual(PlayerState.Goal, machine.State);
        }

        [Test]
        public void ResetLeavesGoal()
        {
            machine.Enter(PlayerState.Goal);
            machine.Reset();
            Assert.AreEqual(PlayerState.NotStarted, machine.State);
        }

        [Test]
        public void ResetOnNotStartedRaisesNoChange()
        {
            int changes = 0;
            machine.Changed += _ => changes++;
            machine.Reset();
            Assert.AreEqual(0, changes);
        }

        [Test]
        public void ResetAfterIdleRaisesChangeOnce()
        {
            machine.Begin();
            int changes = 0;
            machine.Changed += _ => changes++;
            machine.Reset();
            Assert.AreEqual(1, changes);
        }
    }
}
