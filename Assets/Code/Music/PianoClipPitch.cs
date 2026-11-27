using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class PianoClipPitch
    {
        [SerializeField] [Tooltip("The piano clip.")] AudioClip clip;
        [SerializeField] [Tooltip("Every note that sounds in the clip as numbers 0 to 11: C=0, C#=1, D=2, D#=3, E=4, F=5, F#=6, G=7, G#=8, A=9, A#=10, B=11. A chord clip lists all of its notes.")] int[] pitchClasses = new int[0];
        [SerializeField] [Tooltip("Note names with octave for you to read, for example C4 E4 G4. Not used by the game.")] string label;

        public PianoClipPitch()
        {
        }

        public PianoClipPitch(AudioClip clip, int[] pitchClasses, string label)
        {
            this.clip = clip;
            this.pitchClasses = pitchClasses;
            this.label = label;
        }

        public AudioClip Clip => clip;

        public string Label => label;

        public int[] PitchClassNumbers => pitchClasses;

        public PitchClassSet PitchClasses => PitchClassSet.Of(pitchClasses);
    }
}
