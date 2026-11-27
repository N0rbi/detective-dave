using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class ChordSegment
    {
        [SerializeField] [Tooltip("Name of the chord, for example Am7. Only shown to you.")] string chordName = "Chord";
        [SerializeField, Min(0f)] [Tooltip("Beat of the loop where this chord starts. Beat 0 is the first beat of the chart.")] float startBeat;
        [SerializeField, Min(0.25f)] [Tooltip("How many beats the chord lasts.")] float lengthBeats = 4f;
        [SerializeField] [Tooltip("The notes of the chord as numbers 0 to 11: C=0, C#=1, D=2, D#=3, E=4, F=5, F#=6, G=7, G#=8, A=9, A#=10, B=11. Octave does not matter.")] int[] chordTones = new int[0];
        [SerializeField] [Tooltip("The scale that is safe to play over this chord, same numbers. A piano clip is only played when all of its notes are in this scale. It should contain the chord tones.")] int[] scaleTones = new int[0];

        public ChordSegment()
        {
        }

        public ChordSegment(string chordName, float startBeat, float lengthBeats, int[] chordTones, int[] scaleTones)
        {
            this.chordName = chordName;
            this.startBeat = startBeat;
            this.lengthBeats = lengthBeats;
            this.chordTones = chordTones;
            this.scaleTones = scaleTones;
        }

        public string ChordName => chordName;

        public float StartBeat => startBeat;

        public float LengthBeats => lengthBeats;

        public float EndBeat => startBeat + lengthBeats;

        public int[] ChordToneNumbers => chordTones;

        public int[] ScaleToneNumbers => scaleTones;

        public PitchClassSet ChordTones => PitchClassSet.Of(chordTones);

        public PitchClassSet ScaleTones => PitchClassSet.Of(scaleTones);

        public bool Covers(double beat) => beat >= startBeat && beat < EndBeat;
    }
}
