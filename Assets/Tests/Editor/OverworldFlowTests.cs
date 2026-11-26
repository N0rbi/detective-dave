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
            public string ReportCase;
            public string ReviewCase;
            public Action<LetterOutcome> Finished;
            public Action ReportFinished;
            public Action ReviewFinished;
            public bool Closed;
            public string Refusal;
            public void Open(string caseId, Action<LetterOutcome> finished) { OpenedCase = caseId; Finished = finished; }
            public void OpenReport(string caseId, Action finished) { ReportCase = caseId; ReportFinished = finished; }
            public void OpenReview(string caseId, Action finished) { ReviewCase = caseId; ReviewFinished = finished; }
            public void Refuse(string message, Action finished) { Refusal = message; finished(); }
            public void Close(Action done) { Closed = true; done(); }
        }

        sealed class FakeLauncher : ICaseLauncher
        {
            public string Launched;
            public bool Launchable = true;
            public bool CanLaunch(string caseId) => Launchable;
            public void Launch(string caseId) => Launched = caseId;
        }

        sealed class FakeCues : ICaseMusicCues
        {
            public string Opened;
            public int Ended;
            public void CaseOpened(string caseId) => Opened = caseId;
            public void CaseEnded() => Ended++;
        }

        sealed class FakeBriefingCue : IBriefingCue
        {
            public int Opened;
            public int Closed;
            public void BriefingOpened() => Opened++;
            public void BriefingClosed() => Closed++;
        }

        FakeBriefingCue briefingCue;
        FakeCues cues;
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
            cues = new FakeCues();
            briefingCue = new FakeBriefingCue();
            flow = new OverworldFlow(cases, new DwellTimer(2f), indicator, briefing, launcher, cues, briefingCue);
        }

        void Dwell()
        {
            flow.OnMoved(Key.J);
            flow.Tick(2.5f);
        }

        [Test]
        public void Opening_the_briefing_cues_the_detective_once()
        {
            Dwell();
            Assert.AreEqual(1, briefingCue.Opened);
            Assert.AreEqual(0, briefingCue.Closed);
        }

        [Test]
        public void Declining_the_request_cues_the_detective_to_come_back()
        {
            Dwell();
            briefing.Finished(LetterOutcome.Declined);
            Assert.AreEqual(1, briefingCue.Closed);
        }

        [Test]
        public void Accepting_the_request_cues_the_detective_to_come_back_before_leaving()
        {
            Dwell();
            briefing.Finished(LetterOutcome.Accepted);
            Assert.AreEqual(1, briefingCue.Closed);
            Assert.AreEqual("a", launcher.Launched);
        }

        [Test]
        public void Playing_a_report_cues_the_detective_down_and_up()
        {
            flow.PlayReport("a", () => { });
            Assert.AreEqual(1, briefingCue.Opened);
            briefing.ReportFinished();
            Assert.AreEqual(1, briefingCue.Closed);
        }

        [Test]
        public void Opening_the_briefing_starts_the_case_music()
        {
            Dwell();
            Assert.AreEqual("a", cues.Opened);
        }

        [Test]
        public void Declining_the_request_ends_the_case_music()
        {
            Dwell();
            briefing.Finished(LetterOutcome.Declined);
            Assert.AreEqual(1, cues.Ended);
        }

        [Test]
        public void Accepting_the_request_keeps_the_case_music_playing()
        {
            Dwell();
            briefing.Finished(LetterOutcome.Accepted);
            Assert.AreEqual(0, cues.Ended);
        }

        [Test]
        public void Typing_the_findings_ends_the_case_music()
        {
            flow.PlayReport("a", () => { });
            Assert.AreEqual(1, cues.Ended);
        }

        [Test]
        public void Reviewing_a_solved_case_never_starts_the_case_music()
        {
            state = CaseState.Solved;
            Dwell();
            Assert.IsNull(cues.Opened);
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
            Assert.IsNull(briefing.ReportCase);
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
        public void Playing_the_report_blocks_moves_and_reports_completion_only_after_the_response_is_typed_and_the_briefing_closes()
        {
            bool completed = false;
            flow.PlayReport("a", () => completed = true);
            Assert.AreEqual("a", briefing.ReportCase);
            Assert.IsFalse(flow.AcceptsMoves);
            Assert.IsFalse(completed);
            briefing.ReportFinished();
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

        [Test]
        public void Accepting_a_case_without_a_level_refuses_and_returns_to_the_desk()
        {
            launcher.Launchable = false;
            Dwell();
            briefing.Finished(LetterOutcome.Accepted);
            Assert.AreEqual(OverworldFlow.NoLevelMessage, briefing.Refusal);
            Assert.IsNull(launcher.Launched);
            Assert.AreEqual(OverworldStage.Desk, flow.Stage);
            Assert.IsTrue(flow.AcceptsMoves);
        }

        [Test]
        public void Declining_a_case_without_a_level_is_not_refused()
        {
            launcher.Launchable = false;
            Dwell();
            briefing.Finished(LetterOutcome.Declined);
            Assert.IsNull(briefing.Refusal);
        }
    }
}
