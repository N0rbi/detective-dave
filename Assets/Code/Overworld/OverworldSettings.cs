using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Overworld Settings", fileName = "Overworld Settings.config")]
    public sealed class OverworldSettings : ScriptableObject
    {
        [Header("Art and audio (swap these for the final files)")]
        [SerializeField] [Tooltip("Picture of a case that is open or locked.")] Sprite folderSprite;
        [SerializeField] [Tooltip("Picture of a case that is solved.")] Sprite solvedSprite;
        [SerializeField] [Tooltip("The desk surface drawn behind everything in the overworld and behind the briefing.")] Sprite deskTopSprite;
        [SerializeField] [Tooltip("The sheet of paper under the case description.")] Sprite descriptionPaperSprite;
        [SerializeField] [Tooltip("The sheet of paper the letter and the findings are typed on.")] Sprite letterPaperSprite;
        [SerializeField] [Tooltip("The hollow ring drawn around the magnifying glass while the dwell timer runs. White is tinted by the ring track colour.")] Sprite ringSprite;
        [SerializeField] [Tooltip("The typewriter font used for the description, the letter and the findings.")] Font textFont;
        [SerializeField] [Tooltip("Typewriter clicks. One is picked at random for every valid key.")] AudioClip[] typewriterClicks = new AudioClip[0];
        [SerializeField] [Tooltip("Played when Backspace takes a letter away. Leave empty for silence.")] AudioClip backspaceClip;

        [Header("Desk")]
        [SerializeField, Range(0f, 1f)] [Tooltip("How dark a locked case file is. 0 shows it as it is, 1 makes it black.")] float lockedShade = 0.65f;

        [Header("Dwell")]
        [SerializeField, Min(0.1f)] [Tooltip("Seconds the magnifying glass has to stay on a case before the briefing opens.")] float dwellSeconds = 2f;
        [SerializeField, Min(0.1f)] [Tooltip("Radius of the ring around the magnifying glass, in key widths.")] float ringRadiusInKeys = 1f;
        [SerializeField, Range(0.01f, 0.3f)] [Tooltip("Thickness of the filling arc, in key widths.")] float ringWidthInKeys = 0.06f;
        [SerializeField] [Tooltip("Colour of the empty ring.")] Color ringTrackColor = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] [Tooltip("Colour of the arc while it fills.")] Color ringFillColor = new Color(1f, 0.85f, 0.2f, 1f);

        [Header("Briefing")]
        [SerializeField, Min(0.05f)] [Tooltip("Seconds the screen takes to slide to the briefing and back.")] float slideSeconds = 0.6f;
        [SerializeField, Min(0f)] [Tooltip("Seconds between typing the last letter and the screen sliding away.")] float finishPauseSeconds = 0.6f;
        [SerializeField] [Tooltip("Where the case description paper lies on the screen.")] PaperPlacement descriptionPlacement = new PaperPlacement(-0.25f, 0.02f, 0.4f, -2f);
        [SerializeField] [Tooltip("Where the letter paper lies on the screen.")] PaperPlacement letterPlacement = new PaperPlacement(0.25f, -0.01f, 0.4f, 1.5f);
        [SerializeField, Range(0f, 0.3f)] [Tooltip("Empty border of the paper picture, as a share of its width. Text starts inside it.")] float paperPaddingShare = 0.09f;
        [SerializeField, Range(0.01f, 0.08f)] [Tooltip("Text size as a share of the screen height.")] float textHeightShare = 0.034f;
        [SerializeField, Range(0.3f, 1f)] [Tooltip("Width of one letter divided by the text size. 0.6 fits typewriter fonts. Raise it if lines run past the paper edge.")] float textAdvanceRatio = 0.6f;
        [SerializeField] [Tooltip("Colour of the case description.")] Color descriptionTextColor = new Color(0.1f, 0.1f, 0.12f, 1f);
        [SerializeField] [Tooltip("Colour of the letter that is not typed yet.")] Color pendingTextColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] [Tooltip("Colour of the letter that is typed.")] Color typedTextColor = Color.black;

        [Header("Description typing")]
        [SerializeField, Min(0.001f)] [Tooltip("Seconds between two characters of the case description typing in. Constant: no pauses.")] float descriptionSecondsPerCharacter = 0.025f;
        [SerializeField, Min(1f)] [Tooltip("Description skip speed multiplier. Pressing any key while the description types makes the rest type this many times faster.")] float descriptionSkipSpeedMultiplier = 3f;
        [SerializeField, Min(0.05f)] [Tooltip("Seconds the writable letter paper takes to slide onto the desk once the description is finished.")] float letterAppearSeconds = 0.35f;

        [Header("Sound")]
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the typewriter clicks.")] float clickVolume = 0.8f;
        [SerializeField, Range(0.5f, 1.5f)] [Tooltip("Lowest pitch of a click.")] float clickPitchMin = 0.94f;
        [SerializeField, Range(0.5f, 1.5f)] [Tooltip("Highest pitch of a click.")] float clickPitchMax = 1.06f;
        [SerializeField] [Tooltip("Plays a click for every letter of the description as it types in.")] bool descriptionClicks = true;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the description clicks as a share of the normal click volume.")] float descriptionClickShare = 0.5f;

        [Header("Typing")]
        [SerializeField] [Tooltip("Newlines and punctuation type themselves, so nobody needs special symbols on a foreign keyboard.")] bool autoTypeFreeCharacters = true;
        [SerializeField] [Tooltip("How much of the findings one correct word, sentence or letter reveals.")] RevealUnit findingsUnit = RevealUnit.Word;

        public Sprite FolderSprite => folderSprite;
        public Sprite SolvedSprite => solvedSprite;
        public Sprite DeskTopSprite => deskTopSprite;
        public Sprite DescriptionPaperSprite => descriptionPaperSprite;
        public Sprite LetterPaperSprite => letterPaperSprite;
        public Sprite RingSprite => ringSprite;
        public Font TextFont => textFont;
        public AudioClip[] TypewriterClicks => typewriterClicks;
        public AudioClip BackspaceClip => backspaceClip;
        public float LockedShade => lockedShade;
        public float DwellSeconds => dwellSeconds;
        public float RingRadiusInKeys => ringRadiusInKeys;
        public float RingWidthInKeys => ringWidthInKeys;
        public Color RingTrackColor => ringTrackColor;
        public Color RingFillColor => ringFillColor;
        public float SlideSeconds => slideSeconds;
        public float FinishPauseSeconds => finishPauseSeconds;
        public PaperPlacement DescriptionPlacement => descriptionPlacement;
        public PaperPlacement LetterPlacement => letterPlacement;
        public float PaperPaddingShare => paperPaddingShare;
        public float TextHeightShare => textHeightShare;
        public float TextAdvanceRatio => textAdvanceRatio;
        public Color DescriptionTextColor => descriptionTextColor;
        public Color PendingTextColor => pendingTextColor;
        public Color TypedTextColor => typedTextColor;
        public float ClickVolume => clickVolume;
        public float ClickPitchMin => Mathf.Min(clickPitchMin, clickPitchMax);
        public float ClickPitchMax => Mathf.Max(clickPitchMin, clickPitchMax);
        public bool DescriptionClicks => descriptionClicks;
        public float DescriptionClickShare => descriptionClickShare;
        public TypingRules TypingRules => new TypingRules(autoTypeFreeCharacters);
        public RevealUnit FindingsUnit => findingsUnit;
        public float DescriptionSecondsPerCharacter => descriptionSecondsPerCharacter;
        public float DescriptionSkipSpeedMultiplier => descriptionSkipSpeedMultiplier;
        public float LetterAppearSeconds => letterAppearSeconds;

        public string MissingReferences
        {
            get
            {
                var missing = new System.Collections.Generic.List<string>();
                if (!folderSprite) missing.Add("folderSprite");
                if (!solvedSprite) missing.Add("solvedSprite");
                if (!deskTopSprite) missing.Add("deskTopSprite");
                if (!descriptionPaperSprite) missing.Add("descriptionPaperSprite");
                if (!letterPaperSprite) missing.Add("letterPaperSprite");
                if (!ringSprite) missing.Add("ringSprite");
                if (!textFont) missing.Add("textFont");
                if (typewriterClicks == null || typewriterClicks.Length == 0 || System.Array.TrueForAll(typewriterClicks, clip => !clip)) missing.Add("typewriterClicks");
                return string.Join(", ", missing);
            }
        }
    }
}
