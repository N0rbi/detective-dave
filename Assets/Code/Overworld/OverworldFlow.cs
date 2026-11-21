using System;
using DetectiveDave.PlayerPlay;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class OverworldFlow
    {
        readonly DeskCases cases;
        readonly DwellTimer dwell;
        readonly IDwellIndicator indicator;
        readonly IBriefingScreen briefing;
        readonly ICaseLauncher launcher;
        string dwellCaseId;
        CaseState dwellState;

        public OverworldFlow(DeskCases cases, DwellTimer dwell, IDwellIndicator indicator, IBriefingScreen briefing, ICaseLauncher launcher)
        {
            this.cases = cases;
            this.dwell = dwell;
            this.indicator = indicator;
            this.briefing = briefing;
            this.launcher = launcher;
        }

        public OverworldStage Stage { get; private set; } = OverworldStage.Desk;

        public bool AcceptsMoves => Stage == OverworldStage.Desk || Stage == OverworldStage.Dwelling;

        public void OnMoved(Key key)
        {
            CancelDwell();
            if (!TryGetVisitableCase(key, out var caseId, out var state)) return;
            dwellCaseId = caseId;
            dwellState = state;
            Stage = OverworldStage.Dwelling;
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
            Stage = OverworldStage.Briefing;
            briefing.OpenReport(caseId, () => briefing.Close(() => FinishReport(completed)));
        }

        void ReviewReport(string caseId)
        {
            Stage = OverworldStage.Briefing;
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
            Stage = OverworldStage.Desk;
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
            Stage = OverworldStage.Briefing;
            briefing.Open(caseId, outcome => FinishLetter(caseId, outcome));
        }

        void FinishReport(Action completed)
        {
            Stage = OverworldStage.Desk;
            completed?.Invoke();
        }

        void FinishLetter(string caseId, LetterOutcome outcome)
        {
            if (Stage != OverworldStage.Briefing) return;
            briefing.Close(() => LeaveBriefing(caseId, outcome));
        }

        void LeaveBriefing(string caseId, LetterOutcome outcome)
        {
            if (outcome == LetterOutcome.Accepted)
            {
                Stage = OverworldStage.Leaving;
                launcher.Launch(caseId);
                return;
            }
            Stage = OverworldStage.Desk;
        }
    }
}
