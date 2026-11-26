using System;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using UnityEngine;

namespace DetectiveDave.CheatPlay
{
    public sealed class UnityCheatEnvironment : ICheatEnvironment
    {
        const string LogPrefix = "[Cheats]";

        readonly Action<string> showMessage;

        public UnityCheatEnvironment(Action<string> showMessage)
        {
            this.showMessage = showMessage;
        }

        public void ForgetSolvedCases()
        {
            JsonProgressStore.AtDefaultPath().Reset();
            Debug.Log($"{LogPrefix} Reset Player: the solved cases are forgotten.");
        }

        public void ClearCaseSession()
        {
            CaseSession.Clear();
            MusicDirector.CaseMusicPlayer.StopNow();
        }

        public void LoadOverworld()
        {
            Debug.Log($"{LogPrefix} Reset Player: loading the overworld.");
            SceneLoader.Load(GameScene.Overworld);
        }

        public void ShowMessage(string text) => showMessage(text);
    }
}
