using UnityEngine;
using UnityEngine.Serialization;

namespace DetectiveDave.OverworldPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Overworld Settings", fileName = "Overworld Settings.config.asset")]
    public sealed class OverworldSettings : ScriptableObject
    {
        [Header("Art and audio (swap these for the final files)")]
        [SerializeField] [Tooltip("Picture of a case that is open or locked.")] Sprite folderSprite;
        [SerializeField] [Tooltip("Picture of a case that is solved.")] Sprite solvedSprite;
        [SerializeField] [Tooltip("The desk surface drawn behind everything in the overworld and behind the briefing.")] Sprite deskTopSprite;
        [SerializeField] [Tooltip("The sheet of paper under the case description.")] Sprite descriptionPaperSprite;
        [SerializeField] [Tooltip("The sheet of paper the letter and the response are typed on.")] Sprite letterPaperSprite;
        [SerializeField] [Tooltip("The hollow ring drawn around the magnifying glass while the dwell timer runs. White is tinted by the ring track colour.")] Sprite ringSprite;
        [SerializeField] [Tooltip("The keycap picture drawn under Left Shift and Right Shift so the desk reads as a keyboard. It is stretched to the size of the key.")] Sprite shiftKeycapSprite;
        [SerializeField] [Tooltip("Typewriter clicks. One is picked at random for every valid key.")] AudioClip[] typewriterClicks = new AudioClip[0];
        [SerializeField] [Tooltip("Played when Backspace takes a letter away. Leave empty for silence.")] AudioClip backspaceClip;

        [Header("Fonts")]
        [SerializeField] [Tooltip("Used for the client's message on the left paper and for the grey placeholder text that waits to be typed. Use a monospaced font.")] Font clientAndPlaceholderFont;
        [SerializeField] [Tooltip("Used for the investigation findings on the left paper and for the black text the player has typed. Use a monospaced font.")] Font findingsAndTypedFont;
        [SerializeField] [Tooltip("Used for the notice shown on the letter paper when a case has no level yet. Use a monospaced font.")] Font noticeFont;

        [Header("Desk")]
        [SerializeField] [Tooltip("The desk the game starts on.")] DeskLayout desk;
        [SerializeField, Range(0f, 1f)] [Tooltip("How dark a locked case file is. 0 shows it as it is, 1 makes it black.")] float lockedShade = 0.65f;

        [Header("Keyboard look")]
        [SerializeField] [Tooltip("Colour the keycap picture is tinted with.")] Color shiftKeycapColor = Color.white;
        [SerializeField, Range(0.5f, 1.2f)] [Tooltip("Size of the keycap picture as a share of the key it sits on.")] float shiftKeycapSizeShare = 0.95f;

        [Header("Floating case files")]
        [SerializeField] [Tooltip("Case files drift gently. Every file has its own random timing, so they never move together.")] bool caseFloatEnabled = true;
        [SerializeField, Min(0f)] [Tooltip("Smallest drift distance of a file, in key widths. Every file picks its own between the two.")] float caseFloatAmplitudeMinInKeys = 0.03f;
        [SerializeField, Min(0f)] [Tooltip("Largest drift distance of a file, in key widths.")] float caseFloatAmplitudeMaxInKeys = 0.08f;
        [SerializeField, Min(0.1f)] [Tooltip("Shortest time a file takes for one bob up and down, in seconds.")] float caseFloatSecondsPerCycleMin = 2.5f;
        [SerializeField, Min(0.1f)] [Tooltip("Longest time a file takes for one bob up and down, in seconds.")] float caseFloatSecondsPerCycleMax = 4.5f;
        [SerializeField, Range(0f, 10f)] [Tooltip("Largest slow tilt of a file in degrees. 0 keeps the files straight.")] float caseFloatSwayDegreesMax = 1.2f;

        [Header("Route hint")]
        [SerializeField] [Tooltip("Shows a dashed line along the keys from the magnifying glass to the nearest open case, so a new player sees that the desk is a keyboard.")] bool routeHintEnabled = true;
        [SerializeField] [Tooltip("UntilFirstMove hides the line when the player first moves. UntilCaseReached keeps it, shortening from where the player stands, until the player reaches an open case.")] RouteHintVisibility routeHintVisibility = RouteHintVisibility.UntilCaseReached;
        [SerializeField, Min(0.05f)] [Tooltip("Length of one dash, in key widths.")] float routeDashLengthInKeys = 0.2f;
        [SerializeField, Min(0f)] [Tooltip("Gap between two dashes, in key widths.")] float routeGapLengthInKeys = 0.15f;
        [SerializeField, Range(0.01f, 0.3f)] [Tooltip("Thickness of the line, in key widths.")] float routeWidthInKeys = 0.07f;
        [SerializeField, Tooltip("How fast the dashes run towards the case, in key widths per second. 0 keeps them still.")] float routeFlowKeysPerSecond = 0.6f;
        [SerializeField] [Tooltip("Colour of the dashes.")] Color routeColor = new Color(1f, 0.85f, 0.2f, 0.9f);

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
        [SerializeField, Range(0.01f, 0.08f)] [Tooltip("Largest text size, as a share of the screen height. Long texts shrink from here until they fit the paper.")] float textHeightShare = 0.034f;
        [SerializeField, Range(0.005f, 0.08f)] [Tooltip("Smallest text size, as a share of the screen height. Below this the text is left as is and a warning is logged.")] float minTextHeightShare = 0.016f;
        [SerializeField, Range(0.001f, 0.01f)] [Tooltip("How much the text size shrinks per try, as a share of the screen height.")] float textFitStepShare = 0.002f;
        [SerializeField, Range(0.8f, 2f)] [Tooltip("Height of one text row divided by the text size. Raise it if text still runs past the bottom of the paper.")] float textLineHeightRatio = 1.1f;
        [SerializeField, Range(0.3f, 1f)] [Tooltip("Width of one letter divided by the text size. 0.6 fits typewriter fonts. Raise it if lines run past the paper edge.")] float textAdvanceRatio = 0.6f;
        [SerializeField] [Tooltip("Colour of the case description.")] Color descriptionTextColor = new Color(0.1f, 0.1f, 0.12f, 1f);
        [SerializeField] [Tooltip("Colour of the letter that is not typed yet.")] Color pendingTextColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] [Tooltip("Colour of the letter that is typed.")] Color typedTextColor = Color.black;
        [SerializeField] [Tooltip("Colour of the accept answer that is not typed yet, while the player has not chosen.")] Color acceptPendingTextColor = new Color(0.2f, 0.6f, 0.28f, 1f);
        [SerializeField] [Tooltip("Colour of the decline answer that is not typed yet, while the player has not chosen.")] Color declinePendingTextColor = new Color(0.75f, 0.22f, 0.22f, 1f);
        [SerializeField, Min(0f)] [Tooltip("Seconds the answer that was not chosen takes to fade out. It keeps its space on the paper.")] float unchosenFadeSeconds = 0.4f;

        [Header("Description typing")]
        [SerializeField, Min(0.001f)] [Tooltip("Seconds between two characters of the case description typing in. Constant: no pauses.")] float descriptionSecondsPerCharacter = 0.025f;
        [SerializeField, Min(1f)] [Tooltip("Description skip speed multiplier. Pressing any key while the description types makes the rest type this many times faster.")] float descriptionSkipSpeedMultiplier = 3f;
        [SerializeField, Min(0.05f)] [Tooltip("Seconds the writable letter paper takes to slide onto the desk once the description is finished.")] float letterAppearSeconds = 0.35f;

        [Header("Sound")]
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the typewriter clicks.")] float clickVolume = 0.8f;
        [SerializeField, Range(0.5f, 1.5f)] [Tooltip("Lowest pitch of a click.")] float clickPitchMin = 0.94f;
        [SerializeField, Range(0.5f, 1.5f)] [Tooltip("Highest pitch of a click.")] float clickPitchMax = 1.06f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Quietest strike as a share of the click volume. Every strike picks a random volume between the two shares.")] float clickVolumeShareMin = 0.85f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Loudest strike as a share of the click volume.")] float clickVolumeShareMax = 1f;
        [SerializeField, Min(0f)] [Tooltip("Fewest seconds between two clicks of the description typing, so skipping sounds like fast typing and not a buzz.")] float minSecondsBetweenClicks = 0.03f;
        [SerializeField] [Tooltip("Plays a click for every letter of the description as it types in.")] bool descriptionClicks = true;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the description clicks as a share of the normal click volume.")] float descriptionClickShare = 0.5f;

        [Header("Typing")]
        [SerializeField] [Tooltip("Punctuation and symbols type themselves, so nobody needs special symbols on a foreign keyboard. Spaces and line breaks always type themselves.")] bool autoTypeFreeCharacters = true;
        [SerializeField] [FormerlySerializedAs("findingsUnit")] [Tooltip("How much of the response one correct word, sentence or letter reveals.")] RevealUnit responseUnit = RevealUnit.Word;

        public Sprite FolderSprite => folderSprite;
        public Sprite SolvedSprite => solvedSprite;
        public Sprite DeskTopSprite => deskTopSprite;
        public Sprite DescriptionPaperSprite => descriptionPaperSprite;
        public Sprite LetterPaperSprite => letterPaperSprite;
        public Sprite RingSprite => ringSprite;
        public Sprite ShiftKeycapSprite => shiftKeycapSprite;
        public Color ShiftKeycapColor => shiftKeycapColor;
        public float ShiftKeycapSizeShare => shiftKeycapSizeShare;
        public CaseFloatSettings CaseFloat => new CaseFloatSettings(caseFloatEnabled, Mathf.Min(caseFloatAmplitudeMinInKeys, caseFloatAmplitudeMaxInKeys), Mathf.Max(caseFloatAmplitudeMinInKeys, caseFloatAmplitudeMaxInKeys), Mathf.Min(caseFloatSecondsPerCycleMin, caseFloatSecondsPerCycleMax), Mathf.Max(caseFloatSecondsPerCycleMin, caseFloatSecondsPerCycleMax), caseFloatSwayDegreesMax);
        public bool RouteHintEnabled => routeHintEnabled;
        public RouteHintVisibility RouteHintVisibility => routeHintVisibility;
        public float RouteDashLengthInKeys => routeDashLengthInKeys;
        public float RouteGapLengthInKeys => routeGapLengthInKeys;
        public float RouteWidthInKeys => routeWidthInKeys;
        public float RouteFlowKeysPerSecond => routeFlowKeysPerSecond;
        public Color RouteColor => routeColor;
        public Font FontFor(FontSlot slot)
        {
            switch (slot)
            {
                case FontSlot.ClientAndPlaceholder: return clientAndPlaceholderFont;
                case FontSlot.FindingsAndTyped: return findingsAndTypedFont;
                case FontSlot.Notice: return noticeFont;
                default: throw new System.ArgumentOutOfRangeException(nameof(slot), slot, "No font for this slot");
            }
        }

        public AudioClip[] TypewriterClicks => typewriterClicks;
        public AudioClip BackspaceClip => backspaceClip;
        public DeskLayout Desk => desk;
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
        public float MinTextHeightShare => minTextHeightShare;
        public float TextFitStepShare => textFitStepShare;
        public float TextLineHeightRatio => textLineHeightRatio;
        public float TextAdvanceRatio => textAdvanceRatio;
        public Color DescriptionTextColor => descriptionTextColor;
        public Color PendingTextColor => pendingTextColor;
        public Color TypedTextColor => typedTextColor;
        public Color AcceptPendingTextColor => acceptPendingTextColor;
        public Color DeclinePendingTextColor => declinePendingTextColor;
        public float UnchosenFadeSeconds => unchosenFadeSeconds;
        public float ClickVolume => clickVolume;
        public float ClickPitchMin => Mathf.Min(clickPitchMin, clickPitchMax);
        public float ClickPitchMax => Mathf.Max(clickPitchMin, clickPitchMax);
        public float ClickVolumeShareMin => Mathf.Min(clickVolumeShareMin, clickVolumeShareMax);
        public float ClickVolumeShareMax => Mathf.Max(clickVolumeShareMin, clickVolumeShareMax);
        public float MinSecondsBetweenClicks => minSecondsBetweenClicks;
        public bool DescriptionClicks => descriptionClicks;
        public float DescriptionClickShare => descriptionClickShare;
        public TypingRules TypingRules => new TypingRules(autoTypeFreeCharacters);
        public RevealUnit ResponseUnit => responseUnit;
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
                if (!shiftKeycapSprite) missing.Add("shiftKeycapSprite");
                if (!clientAndPlaceholderFont) missing.Add("clientAndPlaceholderFont");
                if (!findingsAndTypedFont) missing.Add("findingsAndTypedFont");
                if (!noticeFont) missing.Add("noticeFont");
                if (typewriterClicks == null || typewriterClicks.Length == 0 || System.Array.TrueForAll(typewriterClicks, clip => !clip)) missing.Add("typewriterClicks");
                return string.Join(", ", missing);
            }
        }
    }
}
