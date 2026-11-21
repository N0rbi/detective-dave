using UnityEngine;
using UnityEngine.Serialization;

namespace DetectiveDave.PlayerPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Player Movement Settings", fileName = "Player Movement Settings.config")]
    public sealed class PlayerMovementSettings : ScriptableObject
    {
        [Header("Look")]
        [SerializeField, Min(0.1f)] [Tooltip("Player height measured in key sizes.")] float heightInKeys = 1.1f;
        [Tooltip("Walk cycle sprites. Frame 0 is the idle pose. At least one.")]
        [SerializeField] Sprite[] walkFrames;
        [Tooltip("Climb cycle sprites shown while the player is on a gutter. At least one.")]
        [SerializeField] Sprite[] climbFrames;

        [SerializeField] [Tooltip("Turn on if the walk cycle art looks to the right when not flipped.")] bool artFacesRight = false;
        [SerializeField] [Tooltip("Turn off for a player that has no left or right, like the magnifying glass.")] bool flipsWithFacing = true;

        [Header("Timing")]
        [SerializeField, Min(0.01f)] [Tooltip("Seconds to walk onto a neighbouring key.")] float stepSeconds = 0.2f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds of the backwards wobble after pressing a key that is not a neighbour.")] float slipSeconds = 0.4f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds of the whole bump when walking into a wall: lean in, squash, return.")] float wallSquashSeconds = 0.5f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the player takes to fade away in a gap.")] float gapFadeSeconds = 0.8f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the respawn drop from the top of the screen takes.")] float dropSeconds = 0.6f;

        [Header("Distances")]
        [SerializeField, Min(0f)] [FormerlySerializedAs("slipDistance")] [Tooltip("Key sizes the player is thrown backwards when slipping.")] float slipDistanceInKeys = 0.3f;
        [SerializeField, Min(0f)] [FormerlySerializedAs("dropStartHeight")] [Tooltip("Key sizes above the camera top edge where the respawn drop starts.")] float dropMarginInKeys = 1f;
        [SerializeField, Min(0f)] [Tooltip("Key sizes the player leans towards a wall before squashing against it.")] float bumpDistanceInKeys = 0.35f;
        [SerializeField, Range(0.2f, 1f)] [Tooltip("Scale factor the player is squashed to when walking into a wall.")] float squashScale = 0.6f;
        [SerializeField, Min(0f)] [Tooltip("Degrees the player tilts backwards when slipping.")] float slipTiltDegrees = 15f;

        [Header("Rules")]
        [SerializeField] [Tooltip("Off: pressing a key that is not a neighbour makes the player slip back. On: nothing happens.")] bool forgivingMoves = false;

        [Header("Dash gust")]
        [SerializeField] [Tooltip("Turns the whole dash gust on or off. The detective dashes, the magnifying glass does not.")] bool dashGustEnabled = true;
        [SerializeField, Range(0f, 1f)] [Tooltip("Height of the middle of the gust above the player's feet, as a share of the player height. 0.55 puts the band between the shoulders (about 0.77) and the upper legs (about 0.33).")] float gustBandCenterInHeights = 0.55f;
        [SerializeField] [Tooltip("The wind lines drawn behind the player when it dashes to a key. Each one has its own offset, thickness, length and start. Remove all to turn the gust off.")] GustLine[] gustLines =
        {
            new GustLine(-0.19f, 0.03f, 0.9f),
            new GustLine(0f, 0.06f, 0.7f),
            new GustLine(0.19f, 0.04f, 1f)
        };
        [SerializeField] [Tooltip("Colour of the gust lines. Its alpha is the starting opacity.")] Color gustColor = new Color(0.2f, 0.23f, 0.34f, 0.7f);
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the gust lines take to shorten and fade away.")] float gustFadeSeconds = 0.25f;

        [Header("Decorations")]
        [SerializeField] [Tooltip("Drain grate shown on the middle of the start key.")] Sprite startGrate;
        [SerializeField] [Tooltip("Folder with a magnifying glass, shown on the goal key.")] Sprite goalIcon;
        [SerializeField] [Tooltip("Star that spins behind the player once the goal is reached.")] Sprite goalBurst;
        [SerializeField, Min(0.1f)] [Tooltip("Width of the start grate picture, in key widths.")] float startGrateSizeInKeys = 1f;
        [SerializeField, Min(0.1f)] [Tooltip("Width of the goal icon picture, in key widths.")] float goalIconSizeInKeys = 1f;
        [SerializeField, Min(0.1f)] [Tooltip("Width of the goal star picture, in key widths.")] float goalBurstSizeInKeys = 2.2f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the goal star takes to grow to full size.")] float goalBurstGrowSeconds = 0.4f;
        [SerializeField] [Tooltip("Degrees per second the goal star turns.")] float goalBurstDegreesPerSecond = 30f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Height of the goal star centre above the player's feet, as a share of the player height.")] float goalBurstCentreInHeights = 0.5f;

        [Header("Goal sequence")]
        [SerializeField, Min(0.1f)] [Tooltip("Width of the evidence held in the player's hand, in key widths.")] float evidenceSizeInKeys = 0.9f;
        [SerializeField, Range(0f, 3f)] [Tooltip("How high the evidence is held above the player's feet, as a share of the player height.")] float evidenceHandRiseInHeights = 1f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the evidence takes to pop into the hand.")] float evidencePopSeconds = 0.3f;
        [SerializeField, Min(0f)] [Tooltip("Seconds the evidence is held up before it zooms.")] float evidenceHoldSeconds = 0.5f;
        [SerializeField, Range(0.15f, 1f)] [Tooltip("How much of the normal screen the camera shows when zoomed on the player. Smaller zooms in closer.")] float cameraZoomFactor = 0.35f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the camera takes to zoom in on the player, and to zoom back out when a level starts.")] float cameraZoomSeconds = 0.8f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Where the zoom aims: 0 is the player's feet, 1 is the evidence in the hand.")] float zoomFocusHandShare = 0.5f;
        [SerializeField, Range(1f, 3f)] [Tooltip("How much bigger the evidence in the hand grows while the camera zooms.")] float evidenceZoomGrowth = 1.3f;
        [SerializeField, Min(0.1f)] [Tooltip("Seconds the spiral screen change takes, in and out.")] float wipeSeconds = 1f;
        [SerializeField, Range(6, 48)] [Tooltip("Wedges in one turn of the spiral. Fewer wedges look chunkier.")] int wipeSegmentsPerTurn = 12;
        [SerializeField, Range(2, 6)] [Tooltip("Turns of the spiral.")] int wipeTurns = 3;
        [SerializeField] [Tooltip("Colour of the spiral wedges.")] Color wipeColor = new Color(0.08f, 0.09f, 0.15f, 1f);

        [Header("Idle")]
        [SerializeField, Range(0f, 1f)] [Tooltip("How far the climbing pose is drawn below the key centre line, as a share of the player height.")] float climbDropInHeights = 0.5f;
        [SerializeField, Range(0f, 0.1f)] [Tooltip("How much taller the player grows when breathing in, as a share of its height.")] float breathingAmount = 0.025f;
        [SerializeField, Min(0.2f)] [Tooltip("Seconds of one full breath, in and out.")] float breathingSeconds = 2.5f;

        public float HeightInKeys => heightInKeys;
        public Sprite[] WalkFrames => walkFrames;
        public Sprite[] ClimbFrames => climbFrames;
        public bool ArtFacesRight => artFacesRight;
        public bool FlipsWithFacing => flipsWithFacing;
        public bool ForgivingMoves => forgivingMoves;
        public float StepSeconds => stepSeconds;
        public float SlipSeconds => slipSeconds;
        public float WallSquashSeconds => wallSquashSeconds;
        public float GapFadeSeconds => gapFadeSeconds;
        public float DropSeconds => dropSeconds;
        public float SlipDistanceInKeys => slipDistanceInKeys;
        public float DropMarginInKeys => dropMarginInKeys;
        public float BumpDistanceInKeys => bumpDistanceInKeys;
        public bool DashGustEnabled => dashGustEnabled;
        public float GustBandCenterInHeights => gustBandCenterInHeights;
        public GustLine[] GustLines => gustLines;
        public Color GustColor => gustColor;
        public float GustFadeSeconds => gustFadeSeconds;
        public float ClimbDropInHeights => climbDropInHeights;
        public float BreathingAmount => breathingAmount;
        public float BreathingSeconds => breathingSeconds;
        public Sprite StartGrate => startGrate;
        public Sprite GoalIcon => goalIcon;
        public Sprite GoalBurst => goalBurst;
        public float StartGrateSizeInKeys => startGrateSizeInKeys;
        public float GoalIconSizeInKeys => goalIconSizeInKeys;
        public float GoalBurstSizeInKeys => goalBurstSizeInKeys;
        public float GoalBurstGrowSeconds => goalBurstGrowSeconds;
        public float GoalBurstDegreesPerSecond => goalBurstDegreesPerSecond;
        public float GoalBurstCentreInKeys => goalBurstCentreInHeights * heightInKeys;
        public float EvidenceSizeInKeys => evidenceSizeInKeys;
        public float EvidenceHandRiseInKeys => evidenceHandRiseInHeights * heightInKeys;
        public float EvidencePopSeconds => evidencePopSeconds;
        public float EvidenceHoldSeconds => evidenceHoldSeconds;
        public float CameraZoomFactor => cameraZoomFactor;
        public float CameraZoomSeconds => cameraZoomSeconds;
        public float ZoomFocusHandShare => zoomFocusHandShare;
        public float EvidenceZoomGrowth => evidenceZoomGrowth;
        public float WipeSeconds => wipeSeconds;
        public int WipeSegmentsPerTurn => wipeSegmentsPerTurn;
        public int WipeTurns => wipeTurns;
        public Color WipeColor => wipeColor;
        public float SquashScale => squashScale;
        public float SlipTiltDegrees => slipTiltDegrees;

        public bool HasValidWalkFrames => AreValidFrames(walkFrames);
        public bool HasValidClimbFrames => AreValidFrames(climbFrames);

        static bool AreValidFrames(Sprite[] frames) => frames != null && frames.Length >= 1 && System.Array.TrueForAll(frames, frame => frame);

        void OnValidate()
        {
            if (!HasValidWalkFrames) Debug.LogError("PlayerMovementSettings: walkFrames needs at least one non-null sprite.", this);
            if (!HasValidClimbFrames) Debug.LogError("PlayerMovementSettings: climbFrames needs at least one non-null sprite.", this);
        }
    }
}
