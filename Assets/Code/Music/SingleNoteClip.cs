using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class SingleNoteClip
    {
        [SerializeField] [Tooltip("A clip with one piano note.")] AudioClip clip;
        [SerializeField, Range(0, 127)] [Tooltip("The note of the clip as a MIDI number: 60 is C4 (middle C), 69 is A4. Set Up Music fills it from the file name, for example Piano Single - Gs3 is 56.")] int midiPitch = 60;

        public SingleNoteClip()
        {
        }

        public SingleNoteClip(AudioClip clip, int midiPitch)
        {
            this.clip = clip;
            this.midiPitch = midiPitch;
        }

        public AudioClip Clip => clip;

        public int MidiPitch => midiPitch;

        public int PitchClass => SinglePitch.PitchClassOf(midiPitch);

        public string NoteName => SinglePitch.DisplayName(midiPitch);
    }
}
