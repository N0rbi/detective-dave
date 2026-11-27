using System;
using DetectiveDave.PlayerPlay;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class OverworldFlow
    {
        public const string NoLevelMessage = "This case has no level yet";

        readonly DeskCases cases;
        readonly DwellTimer dwell;
        readonly IDwellIndicator indicator;
        readonly IBriefingScreen briefing;
        readonly ICaseLauncher launcher;
        readonly ICaseMusicCues music;
        readonly IBriefingCue briefingCue;
        string dwellCaseId;
        CaseState dwellState;

        public OverworldFlow(DeskCases cases, DwellTimer dwell, IDwellIndicator indicator, IBriefingScreen briefing, ICaseLauncher launcher, ICaseMusicCues music = null, IBriefingCue briefingCue = null)
        {
            this.cases = cases;
            this.dwell = dwell;
            this.indicator = indicator;
            this.briefing = briefing;
            this.launcher = launcher;
            this.music = music ?? new NoCaseMusicCues();
            this.briefingCue = briefingCue ?? new NoBriefingCue();
        }

        public OverworldStage Stage { get; private set; } = OverworldStage.Desk;

        void SetStage(OverworldStage next)
        {
            var wasInBriefing = Stage == OverworldStage.Briefing;
            Stage = next;
            var isInBriefing = Stage == OverworldStage.Briefing;
            if (isInBriefing && !wasInBriefing) briefingCue.BriefingOpened();
            if (wasInBriefing && !isInBriefing) briefingCue.BriefingClosed();
        }

        public bool AcceptsMoves => Stage == OverworldStage.Desk || Stage == OverworldStage.Dwelling;

        public void OnMoved(Key key)
        {
            CancelDwell();
            if (!TryGetVisitableCase(key, out var caseId, out var state)) return;
            dwellCaseId = caseId;
            dwellState = state;
            SetStage(OverworldStage.Dwelling);
            dwell.Start();
            indicator.Show(0f);
        }

        public void OnPlayerStateChanged(PlayerState state)
        {
            if (state != PlayerState.Idle) CancelDwell();
        }

        public void Tick(float deltaSeconds)
        {
            if (Stage != OverworldStage.Dwelling) return;
            if (dwell.Tick(deltaSeconds)) OpenBriefing();
            else indicator.Show(dwell.Progress);
        }

        public void PlayReport(string caseId, Action completed)
        {
            SetStage(OverworldStage.Briefing);
            music.CaseEnded();
            briefing.OpenReport(caseId, () => briefing.Close(() => FinishReport(completed)));
        }

        void ReviewReport(string caseId)
        {
            SetStage(OverworldStage.Briefing);
            briefing.OpenReview(caseId, () => briefing.Close(FinishReview));
        }

        void FinishReview() => FinishReport(null);

        bool TryGetVisitableCase(Key key, out string caseId, out CaseState state)
        {
            caseId = null;
            if (!cases.TryGetState(key, out state)) return false;
            return (state == CaseState.Open || state == CaseState.Solved) && cases.TryGetCase(key, out caseId);
        }

        void CancelDwell()
        {
            if (Stage != OverworldStage.Dwelling) return;
            dwell.Cancel();
            indicator.Hide();
            dwellCaseId = null;
            SetStage(OverworldStage.Desk);
        }

        void OpenBriefing()
        {
            var caseId = dwellCaseId;
            indicator.Hide();
            if (dwellState == CaseState.Solved)
            {
                ReviewReport(caseId);
                return;
            }
            SetStage(OverworldStage.Briefing);
            music.CaseOpened(caseId);
            briefing.Open(caseId, outcome => FinishLetter(caseId, outcome));
        }

        void FinishReport(Action completed)
        {
            SetStage(OverworldStage.Desk);
            completed?.Invoke();
        }

        void FinishLetter(string caseId, LetterOutcome outcome)
        {
            if (Stage != OverworldStage.Briefing) return;
            if (outcome == LetterOutcome.Accepted && !launcher.CanLaunch(caseId))
            {
                music.CaseEnded();
                briefing.Refuse(NoLevelMessage, () => briefing.Close(ReturnToDesk));
                return;
            }
            briefing.Close(() => LeaveBriefing(caseId, outcome));
        }

        void ReturnToDesk() => SetStage(OverworldStage.Desk);

        void LeaveBriefing(string caseId, LetterOutcome outcome)
        {
            if (outcome == LetterOutcome.Accepted)
            {
                SetStage(OverworldStage.Leaving);
                launcher.Launch(caseId);
                return;
            }
            music.CaseEnded();
            SetStage(OverworldStage.Desk);
        }
    }
}
