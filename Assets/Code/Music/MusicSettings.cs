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
        [SerializeField] [Tooltip("One of these plays at random on every move of the player and of the magnifying glass. The same one never plays twice in a row.")] AudioClip[] pianoClips = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of a piano note for a move that moves the player.")] float pianoVolume = 0.8f;
        [SerializeField, Range(0f, 0.5f)] [Tooltip("How much the volume of a note changes at random, as a share of its volume. 0.1 means plus or minus 10%.")] float pianoVolumeVariance = 0.1f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume share of a note for a move that does not move the player (a wall, a key that is not next to the player). 0.5 halves the volume.")] float blockedMoveVolumeShare = 0.5f;
        [SerializeField, Min(0f)] [Tooltip("Shortest time between two piano notes, in seconds. 0 plays a note for every move.")] float minSecondsBetweenPianoNotes;
        [SerializeField, Min(1)] [Tooltip("How many piano notes can ring at the same time. The oldest one is cut off when all are busy.")] int pianoPoolSize = 6;

        public AudioClip LoopClip => loopClip;

        public float LoopVolume => loopVolume;

        public float FadeInSeconds => fadeInSeconds;

        public AudioClip[] PianoClips => pianoClips;

        public float PianoVolume => pianoVolume;

        public float PianoVolumeVariance => pianoVolumeVariance;

        public float BlockedMoveVolumeShare => blockedMoveVolumeShare;

        public float MinSecondsBetweenPianoNotes => minSecondsBetweenPianoNotes;

        public int PianoPoolSize => pianoPoolSize;

        public string MissingReferences
        {
            get
            {
                var missing = new System.Collections.Generic.List<string>();
                if (!loopClip) missing.Add("Loop Clip");
                if (!pianoClips.Any(clip => clip)) missing.Add("Piano Clips");
                return string.Join(", ", missing);
            }
        }
    }
}
