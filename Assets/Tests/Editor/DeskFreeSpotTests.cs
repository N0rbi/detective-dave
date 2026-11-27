using System.Collections.Generic;
using DetectiveDave.EditorTools;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class DeskFreeSpotTests
    {
        static readonly Vector2 OneKey = new Vector2(0.9f, 0.9f);
        static readonly Vector2 TwoKeys = new Vector2(2.2f, 0.9f);

        static Dictionary<Key, Vector2> Board() => new Dictionary<Key, Vector2>
        {
            { Key.A, new Vector2(0f, 1f) }, { Key.S, new Vector2(1f, 1f) }, { Key.D, new Vector2(2f, 1f) },
            { Key.LeftShift, new Vector2(0f, 0f) }, { Key.X, new Vector2(1f, 0f) }, { Key.C, new Vector2(2f, 0f) },
        };

        static DeskMarker Covering(string id, Vector2 centre, Vector2 size) => new DeskMarker(id, centre, size, 0f);

        [Test]
        public void AnEmptyDeskStartsAtTheTopLeftKey() =>
            Assert.AreEqual(new Vector2(0f, 1f), DeskFreeSpot.Find(new DeskMarker[0], Board(), Key.LeftShift, OneKey));

        [Test]
        public void ATakenKeyIsSkipped()
        {
            var existing = new[] { Covering("a", new Vector2(0f, 1f), OneKey) };
            Assert.AreEqual(new Vector2(1f, 1f), DeskFreeSpot.Find(existing, Board(), Key.LeftShift, OneKey));
        }

        [Test]
        public void ASpotThatWouldTouchATakenKeyIsSkipped()
        {
            var existing = new[] { Covering("s", new Vector2(1f, 1f), OneKey) };
            var spot = DeskFreeSpot.Find(existing, Board(), Key.LeftShift, TwoKeys);
            Assert.AreEqual(new Vector2(2f, 0f), spot);
        }

        [Test]
        public void TheStartKeyIsNeverCovered()
        {
            var existing = new[] { Covering("top", new Vector2(1f, 1f), new Vector2(3.2f, 0.9f)) };
            var spot = DeskFreeSpot.Find(existing, Board(), Key.LeftShift, TwoKeys);
            Assert.AreEqual(new Vector2(2f, 0f), spot);
        }

        [Test]
        public void ANarrowCaseMayUseTheKeyNextToTheStartKey()
        {
            var existing = new[] { Covering("top", new Vector2(1f, 1f), new Vector2(3.2f, 0.9f)) };
            Assert.AreEqual(new Vector2(1f, 0f), DeskFreeSpot.Find(existing, Board(), Key.LeftShift, OneKey));
        }

        [Test]
        public void ADeskWithNoRoomFallsBackToTheMiddleOfTheKeyboard()
        {
            var existing = new[] { Covering("all", new Vector2(1f, 0.5f), new Vector2(3.2f, 2.2f)) };
            Assert.AreEqual(new Vector2(1f, 0.5f), DeskFreeSpot.Find(existing, Board(), Key.LeftShift, OneKey));
        }

        [Test]
        public void ARowIsReadLeftToRight()
        {
            var board = new Dictionary<Key, Vector2> { { Key.D, new Vector2(2f, 1f) }, { Key.A, new Vector2(0f, 1f) }, { Key.S, new Vector2(1f, 1f) } };
            Assert.AreEqual(new Vector2(0f, 1f), DeskFreeSpot.Find(new DeskMarker[0], board, Key.LeftShift, OneKey));
        }

        [Test]
        public void AnEmptyKeyboardGivesTheOrigin() =>
            Assert.AreEqual(Vector2.zero, DeskFreeSpot.Find(new DeskMarker[0], new Dictionary<Key, Vector2>(), Key.LeftShift, OneKey));
    }
}
