using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public sealed class StartHintGateTests
    {
        FakeBoard board;
        StartHintGate gate;

        [SetUp]
        public void BuildGate()
        {
            board = new FakeBoard();
            board.Connect(Key.LeftShift, Key.Z);
            gate = new StartHintGate(board);
        }

        [Test]
        public void NothingIsWrongBeforeTheGateIsArmed() => Assert.IsFalse(gate.IsWrongStartKey(Key.P));

        [Test]
        public void AKeyThatIsNotNextToTheStartIsWrong()
        {
            gate.Arm(Key.LeftShift);
            Assert.IsTrue(gate.IsWrongStartKey(Key.P));
        }

        [Test]
        public void AKeyNextToTheStartIsNotWrong()
        {
            gate.Arm(Key.LeftShift);
            Assert.IsFalse(gate.IsWrongStartKey(Key.Z));
        }

        [Test]
        public void TheStartKeyItselfIsNotWrong()
        {
            gate.Arm(Key.LeftShift);
            Assert.IsFalse(gate.IsWrongStartKey(Key.LeftShift));
        }

        [Test]
        public void NothingIsWrongAfterTheGateIsDisarmed()
        {
            gate.Arm(Key.LeftShift);
            gate.Disarm();
            Assert.IsFalse(gate.IsWrongStartKey(Key.P));
        }

        [Test]
        public void ArmingAgainMakesWrongKeysWrongAgain()
        {
            gate.Arm(Key.LeftShift);
            gate.Disarm();
            gate.Arm(Key.LeftShift);
            Assert.IsTrue(gate.IsWrongStartKey(Key.P));
        }
    }
}
