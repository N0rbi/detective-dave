using System.Linq;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.CheatPlay
{
    public sealed class CheatDirector : MonoBehaviour
    {
        static CheatDirector instance;

        readonly CheatMessage message = new CheatMessage();
        CheatSettings settings;
        CheatDispatcher dispatcher;
        GUIStyle style;

        public static CheatDirector Ensure(CheatSettings cheatSettings)
        {
            if (instance) return instance;
            var host = new GameObject("Cheat Director");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<CheatDirector>();
            instance.Configure(cheatSettings);
            return instance;
        }

        public void OnRawKeyPressed(KeyPress press)
        {
            if (press.Key == Key.Backspace)
            {
                dispatcher.OnBackspace(Time.unscaledTime);
                return;
            }
            if (KeyLetter.TryFrom(press.DisplayName, out var letter)) dispatcher.OnLetterTyped(letter, Time.unscaledTime);
        }

        void Configure(CheatSettings cheatSettings)
        {
            settings = cheatSettings;
            var environment = new UnityCheatEnvironment(ShowMessage);
            var bindings = settings.Entries.Select(entry => new CheatBinding(entry.Code, CheatFor(entry.Action, environment))).ToArray();
            dispatcher = new CheatDispatcher(bindings, settings.TimeoutSeconds, settings.CheatsEnabled);
            if (!settings.MessageFont) Debug.LogWarning("[Cheats] Cheat Settings has no Message Font, so cheats show no message. Run Window > Detective Dave > Set Up Cheats.", settings);
        }

        IGameCheat CheatFor(CheatAction action, ICheatEnvironment environment)
        {
            switch (action)
            {
                case CheatAction.ResetPlayer: return new ResetPlayerCheat(environment, settings.ResetPlayerMessage);
                default: throw new System.ArgumentOutOfRangeException(nameof(action), action, "No cheat for this action");
            }
        }

        void ShowMessage(string text) => message.Show(text, Time.unscaledTime, settings.MessageSeconds);

        void OnGUI()
        {
            if (!message.TryGet(Time.unscaledTime, out var text)) return;
            if (!settings.MessageFont) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            style.font = settings.MessageFont;
            style.fontSize = Mathf.RoundToInt(Screen.height * settings.MessageSizeShare);
            style.normal.textColor = settings.MessageColor;
            var area = new Rect(0f, Screen.height * 0.08f, Screen.width, Screen.height * settings.MessageSizeShare * 2f);
            var previousColor = GUI.color;
            GUI.color = settings.MessageBackgroundColor;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(area, text, style);
        }
    }
}
