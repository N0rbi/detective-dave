using System.Linq;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Music Settings", fileName = "Music Settings.config.asset")]
    public sealed class MusicSettings : ScriptableObject
    {
        [Header("Background loop")]
        [SerializeField] [Tooltip("The jazz loop. It plays all the time, in the game and in the overworld, and never restarts when a level restarts or the scene changes.")] AudioClip loopClip;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the loop.")] float loopVolume = 0.6f;
        [SerializeField, Min(0f)] [Tooltip("Seconds the loop takes to fade in when the game starts. 0 starts it at full volume.")] float fadeInSeconds = 2f;

        [Header("Piano notes")]
        [SerializeField] [Tooltip("One of these plays on every move of the player and of the magnifying glass. The order they are listed in does not matter: In Order sorts them by name.")] AudioClip[] pianoClips = new AudioClip[0];
        [SerializeField] [Tooltip("Chord Aware picks a clip that fits the chord playing at that moment, favouring the notes of the chord on strong beats, and never the same clip twice in a row. In Order plays the clips sorted by name (Piano 2 before Piano 10), skipping the clips that do not fit the chord. Random picks any fitting clip, never the same one twice in a row. In every mode a clip is only played over a chord whose scale contains all of its notes.")] PianoOrder pianoOrder = PianoOrder.ChordAware;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of a piano note for a move that moves the player.")] float pianoVolume = 0.8f;
        [SerializeField, Range(0f, 0.5f)] [Tooltip("How much the volume of a note changes at random, as a share of its volume. 0.1 means plus or minus 10%.")] float pianoVolumeVariance = 0.1f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume share of a note for a move that does not move the player (a wall, a key that is not next to the player). 0.5 halves the volume.")] float blockedMoveVolumeShare = 0.5f;
        [SerializeField, Min(0f)] [Tooltip("Shortest time between two piano notes, in seconds. 0 plays a note for every move.")] float minSecondsBetweenPianoNotes;
        [SerializeField, Min(1)] [Tooltip("How many piano notes can ring at the same time. The oldest one is cut off when all are busy.")] int pianoPoolSize = 6;

        [Header("Chords and rhythm")]
        [SerializeField] [Tooltip("The chords of the loop and the notes of every piano clip. Required: without it the piano is silent. Run Window > Detective Dave > Set Up Chord Chart to fill it.")] ChordChart chordChart;
        [SerializeField] [Tooltip("Where a note is placed in time. Off plays it at once. Quarter, Eighth and Triplet Eighth move it to the next such point of the music, if that point is not further away than Max Delay Seconds. Triplet Eighth gives a swung feel.")] QuantizeGrid quantizeGrid = QuantizeGrid.TripletEighth;
        [SerializeField, Min(0f)] [Tooltip("The longest a note may be held back to reach the next point of the grid, in seconds. A note whose next point is further away plays at once.")] float maxDelaySeconds = 0.25f;
        [SerializeField, Min(0f)] [Tooltip("Notes are scheduled at least this many seconds ahead so they start exactly on time. 0.02 to 0.05 hides small audio hiccups. Only used when Quantize Grid is not Off.")] float lookAheadSeconds = 0.03f;
        [SerializeField] [Tooltip("When ticked a second move in the same grid point plays no note. When unticked it plays on the following grid point, if that is within Max Delay Seconds, and is dropped otherwise.")] bool dropExtraNotesInSlot;
        [SerializeField, Range(1f, 10f)] [Tooltip("How much more likely a clip made of chord notes is than a clip that only fits the scale, on strong beats and at the start of a chord. 1 treats them the same. Only used by Chord Aware.")] float chordToneWeight = 3f;

        public AudioClip LoopClip => loopClip;

        public float LoopVolume => loopVolume;

        public float FadeInSeconds => fadeInSeconds;

        public AudioClip[] PianoClips => pianoClips;

        public PianoOrder PianoOrder => pianoOrder;

        public float PianoVolume => pianoVolume;

        public float PianoVolumeVariance => pianoVolumeVariance;

        public float BlockedMoveVolumeShare => blockedMoveVolumeShare;

        public float MinSecondsBetweenPianoNotes => minSecondsBetweenPianoNotes;

        public int PianoPoolSize => pianoPoolSize;

        public ChordChart ChordChart => chordChart;

        public QuantizeGrid QuantizeGrid => quantizeGrid;

        public float MaxDelaySeconds => maxDelaySeconds;

        public float LookAheadSeconds => lookAheadSeconds;

        public bool DropExtraNotesInSlot => dropExtraNotesInSlot;

        public float ChordToneWeight => chordToneWeight;

        public string MissingReferences
        {
            get
            {
                var missing = new System.Collections.Generic.List<string>();
                if (!loopClip) missing.Add("Loop Clip");
                if (!pianoClips.Any(clip => clip)) missing.Add("Piano Clips");
                if (!chordChart) missing.Add("Chord Chart");
                return string.Join(", ", missing);
            }
        }
    }
}
