using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Overworld Settings", fileName = "Overworld Settings.config")]
    public sealed class OverworldSettings : ScriptableObject
    {
        [Header("Desk")]
        [SerializeField] [Tooltip("Picture of a case that is open or locked.")] Sprite folderSprite;
        [SerializeField] [Tooltip("Picture of a case that is solved.")] Sprite solvedSprite;
        [SerializeField] [Tooltip("PLACEHOLDER desk colour, drawn behind the case files.")] Color deskColor = new Color(0.36f, 0.23f, 0.13f, 1f);
        [SerializeField, Range(0f, 1f)] [Tooltip("How dark the shade over a locked case is.")] float lockedShade = 0.65f;

        [Header("Dwell")]
        [SerializeField, Min(0.1f)] [Tooltip("Seconds the magnifying glass has to stay on a case before the briefing opens.")] float dwellSeconds = 2f;
        [SerializeField, Min(0.1f)] [Tooltip("Radius of the ring around the magnifying glass, in key widths.")] float ringRadiusInKeys = 0.7f;
        [SerializeField, Range(0.01f, 0.3f)] [Tooltip("Thickness of the ring, in key widths.")] float ringWidthInKeys = 0.06f;
        [SerializeField] [Tooltip("Colour of the empty ring.")] Color ringTrackColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] [Tooltip("Colour of the ring while it fills.")] Color ringFillColor = new Color(1f, 0.85f, 0.2f, 1f);

        [Header("Briefing")]
        [SerializeField, Min(0.05f)] [Tooltip("Seconds the screen takes to slide to the briefing and back.")] float slideSeconds = 0.6f;
        [SerializeField, Min(1f)] [Tooltip("Characters per second of the case description typing in.")] float descriptionCharactersPerSecond = 40f;
        [SerializeField, Min(0f)] [Tooltip("Seconds between typing the last letter and the screen sliding away.")] float finishPauseSeconds = 0.6f;
        [SerializeField] [Tooltip("PLACEHOLDER paper colour.")] Color paperColor = new Color(0.93f, 0.9f, 0.82f, 1f);
        [SerializeField] [Tooltip("Colour of the letter that is not typed yet.")] Color pendingTextColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] [Tooltip("Colour of the letter that is typed.")] Color typedTextColor = Color.black;
        [SerializeField] [Tooltip("Colour of the description on the left.")] Color descriptionColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        [SerializeField] [Tooltip("Colour of the screen behind the briefing.")] Color briefingBackdropColor = new Color(0.08f, 0.09f, 0.15f, 1f);
        [SerializeField, Range(0.01f, 0.08f)] [Tooltip("Text size as a share of the screen height.")] float textHeightShare = 0.034f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the PLACEHOLDER typewriter click.")] float clickVolume = 0.6f;

        [Header("Typing")]
        [SerializeField] [Tooltip("Newlines and punctuation type themselves, so nobody needs special symbols on a foreign keyboard.")] bool autoTypeFreeCharacters = true;
        [SerializeField] [Tooltip("How much of the findings one correct word, sentence or letter reveals.")] RevealUnit findingsUnit = RevealUnit.Word;

        public Sprite FolderSprite => folderSprite;
        public Sprite SolvedSprite => solvedSprite;
        public Color DeskColor => deskColor;
        public float LockedShade => lockedShade;
        public float DwellSeconds => dwellSeconds;
        public float RingRadiusInKeys => ringRadiusInKeys;
        public float RingWidthInKeys => ringWidthInKeys;
        public Color RingTrackColor => ringTrackColor;
        public Color RingFillColor => ringFillColor;
        public float SlideSeconds => slideSeconds;
        public float DescriptionCharactersPerSecond => descriptionCharactersPerSecond;
        public float FinishPauseSeconds => finishPauseSeconds;
        public Color PaperColor => paperColor;
        public Color PendingTextColor => pendingTextColor;
        public Color TypedTextColor => typedTextColor;
        public Color DescriptionColor => descriptionColor;
        public Color BriefingBackdropColor => briefingBackdropColor;
        public float TextHeightShare => textHeightShare;
        public float ClickVolume => clickVolume;
        public TypingRules TypingRules => new TypingRules(autoTypeFreeCharacters);
        public RevealUnit FindingsUnit => findingsUnit;
    }
}
