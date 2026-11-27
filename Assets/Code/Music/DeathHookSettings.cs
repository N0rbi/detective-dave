using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class DeathHookSettings
    {
        [SerializeField] [Tooltip("The muted trumpet. It runs silently all the time in step with the jazz loop; a death fades it up and the music down until the player can move again. Empty: nothing changes on a death. Set Up Music fills it from the file with 'trumpet' in its name.")] AudioClip hookClip;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the hook.")] float hookVolume = 0.9f;
        [SerializeField, Min(0f)] [Tooltip("Seconds the hook takes to fade in, and the music to fade down, after the death.")] float fadeInSeconds = 0.25f;
        [SerializeField, Min(0f)] [Tooltip("Seconds the hook takes to fade out, and the music to fade back up, when the player can move again.")] float fadeOutSeconds = 1f;
        [SerializeField, Range(0f, 1f)] [Tooltip("The jazz loop is turned down to this share of its volume while the hook plays. 0 silences it. It multiplies with the turn-down of a case's music.")] float loopVolumeWhileHookPlays;
        [SerializeField, Range(0f, 1f)] [Tooltip("The music of the open case is turned down to this share of its volume while the hook plays. 0 silences it.")] float caseMusicVolumeWhileHookPlays;
        [SerializeField] [Tooltip("On: piano notes still play while the hook plays. The player cannot move then, so only a wrong key before the respawn would make one.")] bool pianoWhileHookPlays;
        [SerializeField, Min(0f)] [Tooltip("Seconds between the death and the fade starting. The music keeps playing until then.")] float startDelaySeconds;
        [SerializeField] [Tooltip("The hook runs silently all the time in step with the jazz loop, and a death only fades it up. This shifts the hook against the loop in seconds: raise it if it sounds early against the music, lower it (negative) if late. Use it for the delay of the export.")] float hookSyncOffsetSeconds;

        public DeathHookSettings()
        {
            loopVolumeWhileHookPlays = 0f;
            caseMusicVolumeWhileHookPlays = 0f;
            pianoWhileHookPlays = false;
            startDelaySeconds = 0f;
            hookSyncOffsetSeconds = 0f;
        }

        public DeathHookSettings(float fadeInSeconds, float fadeOutSeconds, float hookVolume, float loopVolumeWhileHookPlays, float caseMusicVolumeWhileHookPlays, bool pianoWhileHookPlays, float startDelaySeconds, float hookSyncOffsetSeconds = 0f)
        {
            this.fadeInSeconds = fadeInSeconds;
            this.fadeOutSeconds = fadeOutSeconds;
            this.hookVolume = hookVolume;
            this.loopVolumeWhileHookPlays = loopVolumeWhileHookPlays;
            this.caseMusicVolumeWhileHookPlays = caseMusicVolumeWhileHookPlays;
            this.pianoWhileHookPlays = pianoWhileHookPlays;
            this.startDelaySeconds = startDelaySeconds;
            this.hookSyncOffsetSeconds = hookSyncOffsetSeconds;
        }

        public AudioClip HookClip => hookClip;
        public float HookSyncOffsetSeconds => hookSyncOffsetSeconds;
        public float HookVolume => hookVolume;
        public float FadeInSeconds => fadeInSeconds;
        public float FadeOutSeconds => fadeOutSeconds;
        public float LoopVolumeWhileHookPlays => loopVolumeWhileHookPlays;
        public float CaseMusicVolumeWhileHookPlays => caseMusicVolumeWhileHookPlays;
        public bool PianoWhileHookPlays => pianoWhileHookPlays;
        public float StartDelaySeconds => startDelaySeconds;
    }
}
