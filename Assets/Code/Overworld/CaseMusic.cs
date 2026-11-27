using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [Serializable]
    public sealed class CaseMusic
    {
        [SerializeField] [Tooltip("Optional. The music that plays over the jazz loop while this case is open: from the briefing, through the letter and the level, until the findings are typed or the request is turned down.")] AudioClip clip;
        [SerializeField] [Min(0f)] [Tooltip("Seconds to fade in when the briefing opens.")] float fadeInSeconds = 1.5f;
        [SerializeField] [Min(0f)] [Tooltip("Seconds to fade out when the request is declined, ignored or the findings start.")] float fadeOutSeconds = 2f;
        [SerializeField] [Range(0f, 1f)] [Tooltip("Volume of this music.")] float volume = 0.8f;
        [SerializeField] [Range(0f, 1f)] [Tooltip("The jazz loop is turned down to this share of its volume while this music plays.")] float loopVolumeWhilePlaying = 0.3f;
        [SerializeField] [Tooltip("Keeps this music in step with the jazz loop, so both restart together. Turn off when the clip has its own length.")] bool syncToLoop = true;

        public CaseMusic()
        {
        }

        public CaseMusic(AudioClip clip, float fadeInSeconds, float fadeOutSeconds, float volume, float loopVolumeWhilePlaying, bool syncToLoop)
        {
            this.clip = clip;
            this.fadeInSeconds = fadeInSeconds;
            this.fadeOutSeconds = fadeOutSeconds;
            this.volume = volume;
            this.loopVolumeWhilePlaying = loopVolumeWhilePlaying;
            this.syncToLoop = syncToLoop;
        }

        public AudioClip Clip => clip;
        public float FadeInSeconds => fadeInSeconds;
        public float FadeOutSeconds => fadeOutSeconds;
        public float Volume => volume;
        public float LoopVolumeWhilePlaying => loopVolumeWhilePlaying;
        public bool SyncToLoop => syncToLoop;
        public bool HasClip => clip;
    }
}
