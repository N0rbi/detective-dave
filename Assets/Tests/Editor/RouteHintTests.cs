using System.Collections.Generic;
using DetectiveDave.GuardPlay;
using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public sealed class RouteHintTests
    {
        static readonly Dictionary<Key, Vector2> Row = new Dictionary<Key, Vector2>
        {
            { Key.A, new Vector2(0f, 0f) },
            { Key.S, new Vector2(1f, 0f) },
            { Key.D, new Vector2(2f, 0f) },
            { Key.F, new Vector2(3f, 0f) },
            { Key.G, new Vector2(4f, 0f) },
        };

        FakeBoard board;
        Dictionary<string, CaseState> states;

        [SetUp]
        public void BuildRow()
        {
            board = new FakeBoard();
            board.Connect(Key.A, Key.S);
            board.Connect(Key.S, Key.D);
            board.Connect(Key.D, Key.F);
            board.Connect(Key.F, Key.G);
            states = new Dictionary<string, CaseState> { { "near", CaseState.Open }, { "far", CaseState.Open } };
        }

        DeskRoute Route()
        {
            var markers = new[]
            {
                new DeskMarker("far", new Vector2(4f, 0f), new Vector2(0.5f, 0.5f), 0f),
                new DeskMarker("near", new Vector2(2f, 0f), new Vector2(0.5f, 0.5f), 0f),
            };
            var map = new DeskKeyMap(markers, Row);
            return new DeskRoute(new KeyPathfinder(board, key => true), map, new[] { "far", "near" }, id => states[id]);
        }

        [Test]
        public void RouteGoesToTheNearestOpenCase()
        {
            CollectionAssert.AreEqual(new[] { Key.A, Key.S, Key.D }, Route().ToNearestOpenCase(Key.A));
        }

        [Test]
        public void SolvedAndLockedCasesAreSkipped()
        {
            states["near"] = CaseState.Solved;
            CollectionAssert.AreEqual(new[] { Key.A, Key.S, Key.D, Key.F, Key.G }, Route().ToNearestOpenCase(Key.A));
            states["far"] = CaseState.Locked;
            Assert.That(Route().ToNearestOpenCase(Key.A), Is.Empty);
        }

        [Test]
        public void NoPathGivesNoRoute()
        {
            var lonely = new FakeBoard();
            var map = new DeskKeyMap(new[] { new DeskMarker("near", new Vector2(2f, 0f), new Vector2(0.5f, 0.5f), 0f) }, Row);
            var route = new DeskRoute(new KeyPathfinder(lonely, key => true), map, new[] { "near" }, id => CaseState.Open);
            Assert.That(route.ToNearestOpenCase(Key.A), Is.Empty);
        }

        [Test]
        public void HintShowsTheRouteFromTheStart()
        {
            var hint = new RouteHint(Route(), RouteHintVisibility.UntilFirstMove);
            hint.Begin(Key.A);
            Assert.That(hint.HasLine);
            Assert.That(hint.Keys.Count, Is.EqualTo(3));
        }

        [Test]
        public void UntilFirstMoveHidesTheLineAfterTheFirstMove()
        {
            var hint = new RouteHint(Route(), RouteHintVisibility.UntilFirstMove);
            hint.Begin(Key.A);
            hint.OnMoved(Key.S);
            Assert.That(hint.HasLine, Is.False);
        }

        [Test]
        public void UntilCaseReachedShortensTheLineFromWherePlayerStands()
        {
            var hint = new RouteHint(Route(), RouteHintVisibility.UntilCaseReached);
            hint.Begin(Key.A);
            hint.OnMoved(Key.S);
            CollectionAssert.AreEqual(new[] { Key.S, Key.D }, hint.Keys);
        }

        [Test]
        public void UntilCaseReachedHidesTheLineOnTheCase()
        {
            var hint = new RouteHint(Route(), RouteHintVisibility.UntilCaseReached);
            hint.Begin(Key.A);
            hint.OnMoved(Key.D);
            Assert.That(hint.HasLine, Is.False);
        }

        [Test]
        public void BeginShowsTheLineAgainAfterItWasHidden()
        {
            var hint = new RouteHint(Route(), RouteHintVisibility.UntilFirstMove);
            hint.Begin(Key.A);
            hint.OnMoved(Key.S);
            hint.Begin(Key.A);
            Assert.That(hint.HasLine);
        }
    }
}
