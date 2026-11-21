using System;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.Tests
{
    public class OverworldFlowTests
    {
        sealed class FakeIndicator : IDwellIndicator
        {
            public float Progress = -1f;
            public bool Visible;
            public void Show(float progress) { Visible = true; Progress = progress; }
            public void Hide() => Visible = false;
        }

        sealed class FakeBriefing : IBriefingScreen
        {
            public string OpenedCase;
            public string FindingsCase;
            public string ReviewCase;
            public Action<LetterOutcome> Finished;
            public Action FindingsFinished;
            public Action ReviewFinished;
            public bool Closed;
            public void Open(string caseId, Action<LetterOutcome> finished) { OpenedCase = caseId; Finished = finished; }
            public void OpenFindings(string caseId, Action finished) { FindingsCase = caseId; FindingsFinished = finished; }
            public void OpenReview(string caseId, Action finished) { ReviewCase = caseId; ReviewFinished = finished; }
            public void Close(Action done) { Closed = true; done(); }
        }

        sealed class FakeLauncher : ICaseLauncher
        {
            public string Launched;
            public void Launch(string caseId) => Launched = caseId;
        }

        FakeIndicator indicator;
        FakeBriefing briefing;
        FakeLauncher launcher;
        OverworldFlow flow;
        CaseState state;

        [SetUp]
        public void SetUp()
        {
            state = CaseState.Open;
            indicator = new FakeIndicator();
            briefing = new FakeBriefing();
            launcher = new FakeLauncher();
            var markers = new[] { new DeskMarker("a", Vector2.zero, Vector2.one, 0f) };
            var centres = new System.Collections.Generic.Dictionary<Key, Vector2> { { Key.J, Vector2.zero }, { Key.K, new Vector2(5f, 0f) } };
            var cases = new DeskCases(new DeskKeyMap(markers, centres), id => state);
            flow = new OverworldFlow(cases, new DwellTimer(2f), indicator, briefing, launcher);
        }

        void Dwell()
        {
            flow.OnMoved(Key.J);
            flow.Tick(2.5f);
        }

        [Test]
        public void Arriving_on_an_open_case_starts_the_dwell()
        {
            flow.OnMoved(Key.J);
            Assert.AreEqual(OverworldStage.Dwelling, flow.Stage);
            flow.Tick(1f);
            Assert.AreEqual(0.5f, indicator.Progress, 0.001f);
        }

        [Test]
        public void Arriving_on_an_empty_key_does_nothing()
        {
            flow.OnMoved(Key.K);
            Assert.AreEqual(OverworldStage.Desk, flow.Stage);
        }

        [Test]
        public void Locked_cases_do_not_dwell()
        {
            state = CaseState.Locked;
            flow.OnMoved(Key.J);
            Assert.AreEqual(OverworldStage.Desk, flow.Stage);
        }

        [Test]
        public void Solved_cases_dwell_and_open_a_read_only_review_instead_of_the_letter_or_typing()
        {
            state = CaseState.Solved;
            Dwell();
            Assert.AreEqual("a", briefing.ReviewCase);
            Assert.IsNull(briefing.OpenedCase);
            Assert.IsNull(briefing.FindingsCase);
            Assert.AreEqual(OverworldStage.Briefing, flow.Stage);
            Assert.IsFalse(flow.AcceptsMoves);
        }

        [Test]
        public void Closing_the_review_returns_to_the_desk_without_launching_or_completing_anything()
        {
            state = CaseState.Solved;
            Dwell();
            briefing.ReviewFinished();
            Assert.IsTrue(briefing.Closed);
            Assert.AreEqual(OverworldStage.Desk, flow.Stage);
            Assert.IsTrue(flow.AcceptsMoves);
            Assert.IsNull(launcher.Launched);
        }

        [Test]
        public void Playing_findings_blocks_moves_and_reports_completion_after_the_briefing_closes()
        {
            bool completed = false;
            flow.PlayFindings("a", () => completed = true);
            Assert.AreEqual("a", briefing.FindingsCase);
            Assert.IsFalse(flow.AcceptsMoves);
            Assert.IsFalse(completed);
            briefing.FindingsFinished();
            Assert.IsTrue(briefing.Closed);
            Assert.IsTrue(completed);
            Assert.IsTrue(flow.AcceptsMoves);
        }

        [Test]
        public void Leaving_the_case_cancels_the_dwell()
        {
            flow.OnMoved(Key.J);
            flow.OnPlayerStateChanged(PlayerState.Moving);
            Assert.AreEqual(OverworldStage.Desk, flow.Stage);
            Assert.IsFalse(indicator.Visible);
            flow.Tick(5f);
            Assert.IsNull(briefing.OpenedCase);
        }

        [Test]
        public void Finishing_the_dwell_opens_the_briefing()
        {
            Dwell();
            Assert.AreEqual(OverworldStage.Briefing, flow.Stage);
            Assert.AreEqual("a", briefing.OpenedCase);
            Assert.IsFalse(flow.AcceptsMoves);
        }

        [Test]
        public void Declining_returns_to_the_desk_and_keeps_the_case_open()
        {
            Dwell();
            briefing.Finished(LetterOutcome.Declined);
            Assert.AreEqual(OverworldStage.Desk, flow.Stage);
            Assert.IsNull(launcher.Launched);
            Assert.IsTrue(flow.AcceptsMoves);
        }

        [Test]
        public void Accepting_launches_the_case()
        {
            Dwell();
            briefing.Finished(LetterOutcome.Accepted);
            Assert.AreEqual(OverworldStage.Leaving, flow.Stage);
            Assert.AreEqual("a", launcher.Launched);
        }
    }
}
