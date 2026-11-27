using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class DissonanceWeights
    {
        [SerializeField, Range(0f, 1f)] [Tooltip("A note one semitone above a chord note (for example F over E).")] float minorSecond = 1f;
        [SerializeField, Range(0f, 1f)] [Tooltip("A note one semitone below a chord note (for example B over C).")] float majorSeventh = 0.95f;
        [SerializeField, Range(0f, 1f)] [Tooltip("A note six semitones from a chord note.")] float tritone = 0.8f;
        [SerializeField, Range(0f, 1f)] [Tooltip("A note two semitones above a chord note.")] float majorSecond = 0.5f;
        [SerializeField, Range(0f, 1f)] [Tooltip("A note two semitones below a chord note.")] float minorSeventh = 0.45f;
        [SerializeField, Range(0f, 1f)] [Tooltip("Any other note outside the scale.")] float other = 0.3f;
        [SerializeField, Range(0f, 1f)] [Tooltip("A note that is in the scale of the chord but is not a chord note. Mildly tense.")] float scaleNonChordTone = 0.1f;

        public float MinorSecond => minorSecond;

        public float MajorSeventh => majorSeventh;

        public float Tritone => tritone;

        public float MajorSecond => majorSecond;

        public float MinorSeventh => minorSeventh;

        public float Other => other;

        public float ScaleNonChordTone => scaleNonChordTone;

        public static DissonanceWeights Defaults => new DissonanceWeights();

        public float ForInterval(int semitonesAboveChordTone)
        {
            switch (((semitonesAboveChordTone % 12) + 12) % 12)
            {
                case 1: return minorSecond;
                case 11: return majorSeventh;
                case 6: return tritone;
                case 2: return majorSecond;
                case 10: return minorSeventh;
                default: return other;
            }
        }
    }
}
