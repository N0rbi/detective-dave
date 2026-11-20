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
            if (!TryGetOpenCase(key, out var caseId)) return;
            dwellCaseId = caseId;
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

        bool TryGetOpenCase(Key key, out string caseId)
        {
            caseId = null;
            return cases.TryGetState(key, out var state) && state == CaseState.Open && cases.TryGetCase(key, out caseId);
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
            Stage = OverworldStage.Briefing;
            briefing.Open(caseId, outcome => FinishLetter(caseId, outcome));
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
