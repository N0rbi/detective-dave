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
        [SerializeField] [Tooltip("One of these plays on every move of the player and of the magnifying glass. The order they are listed in does not matter: In Order sorts them by name. A move always plays one of them.")] AudioClip[] pianoClips = new AudioClip[0];
        [SerializeField] [Tooltip("Chord Aware picks a clip that fits the chord playing at that moment, favouring the notes of the chord on strong beats, and never the same clip twice in a row. In Order plays the clips sorted by name (Piano 2 before Piano 10), skipping the clips that do not fit the chord. Random picks any fitting clip, never the same one twice in a row. In every mode a clip is only chosen over a chord whose scale contains all of its notes; When No Clip Fits decides what plays when none does. Without a Chord Chart the order is applied to all clips.")] PianoOrder pianoOrder = PianoOrder.ChordAware;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of a piano note for a move that moves the player.")] float pianoVolume = 0.8f;
        [SerializeField, Range(0f, 0.5f)] [Tooltip("How much the volume of a note changes at random, as a share of its volume. 0.1 means plus or minus 10%.")] float pianoVolumeVariance = 0.1f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume share of a note for a move that does not move the player (a wall, a key that is not next to the player). 0.5 halves the volume.")] float blockedMoveVolumeShare = 0.5f;
        [SerializeField, Min(0f)] [Tooltip("Shortest time between two piano notes, in seconds. 0 plays a note for every move.")] float minSecondsBetweenPianoNotes;
        [SerializeField, Min(1)] [Tooltip("How many piano notes can ring at the same time. The oldest one is cut off when all are busy.")] int pianoPoolSize = 6;

        [Header("Chords")]
        [SerializeField] [Tooltip("The chords of the loop and the notes of every piano clip. Without it the chords are ignored and Piano Order is applied to all clips. Run Window > Detective Dave > Set Up Chord Chart to fill it.")] ChordChart chordChart;
        [SerializeField] [Tooltip("What plays when the chord of that moment has no clip whose notes all fit its scale (or the loop position is outside every chord). Play Closest To Chord plays the clip with the most notes inside the scale, never the same clip as the move before, and the lowest name wins a tie. Play Next Clip plays the next clip in name order. A move is never silent while there is a piano clip.")] WhenNoClipFits whenNoClipFits = WhenNoClipFits.PlayClosestToChord;
        [SerializeField, Range(1f, 10f)] [Tooltip("How much more likely a clip made of chord notes is than a clip that only fits the scale, on strong beats and at the start of a chord. 1 treats them the same. Only used by Chord Aware.")] float chordToneWeight = 3f;

        [Header("Wrong key")]
        [SerializeField] [Tooltip("When the player presses a key that is not next to them (the key that also stuns them), the piano plays the most dissonant clip for the chord playing at that moment. Needs a Chord Chart with clip notes. Off: a wrong key plays a normal note.")] bool dissonantMisclick = true;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume share of the dissonant note, like Blocked Move Volume Share. 0.5 halves the volume.")] float misclickVolumeShare = 0.5f;
        [SerializeField] [Tooltip("How dissonant each kind of note is against the chord. A clip scores the sum over its notes; the clip with the highest score plays.")] DissonanceWeights dissonanceWeights = new DissonanceWeights();
        [SerializeField] [Tooltip("On: a clip scores the average of its notes instead of the sum, so chord clips with three notes do not beat single notes only because they have more notes.")] bool normalizeByNoteCount;

        [Header("Bass")]
        [SerializeField] [Tooltip("When the upright bass plays a note in the loop. The dogs step their animation on these notes. Optional. Run Window > Detective Dave > Set Up Bass Chart to fill it from bass_onsets.json.")] BassChart bassChart;

        [Header("Debounce")]
        [SerializeField, Range(0f, 3f)] [Tooltip("The first piano note after a pause plays a chord clip. Every move within this many seconds of the previous piano note plays a single note instead, so quick runs of moves stay light. Once no note has played for this long, the next one is a chord again. 0 turns it off: every note is a chord.")] float chordDebounceSeconds = 0.6f;
        [SerializeField] [Tooltip("On: every single note restarts the wait, so a continuous run of moves stays single notes until the player pauses. Off: the wait is measured from the last chord only, so a chord returns that many seconds after the last chord even while the player keeps moving.")] bool singlesExtendDebounce = true;
        [SerializeField] [Tooltip("The single note clips, each with its note. They play instead of the chord clips while the debounce is running, and only over chords whose scale contains the note. Export them as Piano Single - <Note><Octave>.wav (Gs3 is G#3), drop them into Assets/Staging and run Window > Detective Dave > Set Up Music. While this list is empty every note is a chord and one warning is logged.")] SingleNoteClip[] singleNoteClips = new SingleNoteClip[0];

        public AudioClip LoopClip => loopClip;

        public float LoopVolume => loopVolume;

        public float FadeInSeconds => fadeInSeconds;

        public AudioClip[] PianoClips => pianoClips;

        public PianoOrder PianoOrder => pianoOrder;

        public float PianoVolume => pianoVolume;

        public float PianoVolumeVariance => pianoVolumeVariance;

        public float BlockedMoveVolumeShare => blockedMoveVolumeShare;

        public bool DissonantMisclick => dissonantMisclick;

        public float MisclickVolumeShare => misclickVolumeShare;

        public DissonanceWeights DissonanceWeights => dissonanceWeights ?? DissonanceWeights.Defaults;

        public bool NormalizeByNoteCount => normalizeByNoteCount;

        public float MinSecondsBetweenPianoNotes => minSecondsBetweenPianoNotes;

        public int PianoPoolSize => pianoPoolSize;

        public float ChordDebounceSeconds => chordDebounceSeconds;

        public bool SinglesExtendDebounce => singlesExtendDebounce;

        public SingleNoteClip[] SingleNoteClips => singleNoteClips;

        public ChordChart ChordChart => chordChart;

        public BassChart BassChart => bassChart;

        public WhenNoClipFits WhenNoClipFits => whenNoClipFits;

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
