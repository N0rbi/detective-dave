using UnityEngine;

namespace DetectiveDave.CheatPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Cheat Settings", fileName = "Cheat Settings.config.asset")]
    public sealed class CheatSettings : ScriptableObject
    {
        [Header("Cheat codes")]
        [SerializeField] [Tooltip("Turn off for a release build: typing a code then does nothing.")] bool cheatsEnabled = true;
        [SerializeField, Min(0.5f)] [Tooltip("Seconds the player may wait between two letters of a code. A longer pause forgets the letters typed so far.")] float timeoutSeconds = 3f;
        [SerializeField] [Tooltip("The codes the player can type, in the game and in the overworld. Typing a code also moves the player or types into a paper, because the letters are normal keys.")] CheatEntry[] entries = { new CheatEntry("resetplayer", CheatAction.ResetPlayer) };

        [Header("Message on screen")]
        [SerializeField] [Tooltip("Text shown after Reset Player forgets the solved cases.")] string resetPlayerMessage = "Progress reset";
        [SerializeField, Min(0.1f)] [Tooltip("Seconds the message stays on screen.")] float messageSeconds = 2f;
        [SerializeField] [Tooltip("Font of the message. Set Up Cheats fills it with the Notice Font of Overworld Settings.")] Font messageFont;
        [SerializeField, Range(0.01f, 0.1f)] [Tooltip("Text size as a share of the screen height.")] float messageSizeShare = 0.04f;
        [SerializeField] Color messageColor = Color.white;
        [SerializeField] Color messageBackgroundColor = new Color(0f, 0f, 0f, 0.75f);

        public bool CheatsEnabled => cheatsEnabled;

        public float TimeoutSeconds => timeoutSeconds;

        public CheatEntry[] Entries => entries;

        public string ResetPlayerMessage => resetPlayerMessage;

        public float MessageSeconds => messageSeconds;

        public Font MessageFont => messageFont;

        public float MessageSizeShare => messageSizeShare;

        public Color MessageColor => messageColor;

        public Color MessageBackgroundColor => messageBackgroundColor;
    }
}
