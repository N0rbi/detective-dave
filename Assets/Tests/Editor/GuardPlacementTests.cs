using System.Linq;
using DetectiveDave.EditorTools;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public sealed class GuardPlacementTests
    {
        FakeBoard board;
        FakeLevel level;

        [SetUp]
        public void CreateRow()
        {
            board = new FakeBoard();
            level = new FakeLevel();
            board.Connect(Key.A, Key.S);
            board.Connect(Key.S, Key.D);
            board.Connect(Key.D, Key.F);
        }

        GuardPlacement Judge(Key start, Key end, Key playerStart = Key.LeftShift) =>
            GuardPlacement.Judge(start, end, key => key != Key.Z, key => level.KindOf(key), board, playerStart);

        [Test]
        public void FloorRow_HasNoProblemsAndTheRoute()
        {
            var placement = Judge(Key.A, Key.F);
            Assert.That(placement.IsFine, Is.True);
            Assert.That(placement.Route, Is.EqualTo(new[] { Key.A, Key.S, Key.D, Key.F }));
        }

        [Test]
        public void NothingPlaced_ReportsBothNotPlaced()
        {
            Assert.That(Judge(Key.None, Key.None).Problems, Is.EquivalentTo(new[] { GuardProblemKind.StartNotPlaced, GuardProblemKind.EndNotPlaced }));
        }

        [Test]
        public void KeyMissingOnTheKeyboard_IsReported()
        {
            Assert.That(Judge(Key.Z, Key.F).Problems, Is.EqualTo(new[] { GuardProblemKind.StartNotOnKeyboard }));
            Assert.That(Judge(Key.A, Key.Z).Problems, Is.EqualTo(new[] { GuardProblemKind.EndNotOnKeyboard }));
        }

        [TestCase(CellKind.Wall)]
        [TestCase(CellKind.Gap)]
        [TestCase(CellKind.Gutter)]
        public void StartOnBlockedKey_IsReported(CellKind kind)
        {
            level.Kinds[Key.A] = kind;
            Assert.That(Judge(Key.A, Key.F).Problems, Is.EqualTo(new[] { GuardProblemKind.StartOnBlockedKey }));
        }

        [Test]
        public void EndOnBlockedKey_IsReported()
        {
            level.Kinds[Key.F] = CellKind.Gap;
            Assert.That(Judge(Key.A, Key.F).Problems, Is.EqualTo(new[] { GuardProblemKind.EndOnBlockedKey }));
        }

        [Test]
        public void StartOnPlayerShift_IsReported()
        {
            board.Connect(Key.LeftShift, Key.A);
            Assert.That(Judge(Key.LeftShift, Key.F).Problems, Does.Contain(GuardProblemKind.StartOnPlayerShift));
        }

        [Test]
        public void BlockedKeyBetween_ReportsNoPath()
        {
            level.Kinds[Key.S] = CellKind.Gutter;
            var placement = Judge(Key.A, Key.F);
            Assert.That(placement.Problems, Is.EqualTo(new[] { GuardProblemKind.NoPathBetweenStartAndEnd }));
            Assert.That(placement.Route, Is.Empty);
        }

        [Test]
        public void EveryProblemHasItsOwnText()
        {
            var kinds = System.Enum.GetValues(typeof(GuardProblemKind)).Cast<GuardProblemKind>();
            var texts = kinds.Select(kind => GuardProblemText.Of(kind, Key.Z, Key.Z)).ToList();
            Assert.That(texts.Distinct().Count(), Is.EqualTo(texts.Count));
        }
    }
}
