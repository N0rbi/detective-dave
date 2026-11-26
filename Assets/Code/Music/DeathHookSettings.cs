using System;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    [Serializable]
    public sealed class DeathHookSettings
    {
        [SerializeField] [Tooltip("The muted trumpet that plays instead of the music from the moment the player dies until they can move again. Empty: nothing changes on a death. Set Up Music fills it from the file with 'trumpet' in its name.")] AudioClip hookClip;
        [SerializeField, Range(0f, 1f)] [Tooltip("Volume of the hook.")] float hookVolume = 0.9f;
        [SerializeField, Min(0f)] [Tooltip("Seconds the hook takes to fade in, and the music to fade down, after the death.")] float fadeInSeconds = 0.25f;
        [SerializeField, Min(0f)] [Tooltip("Seconds the hook takes to fade out, and the music to fade back up, when the player can move again.")] float fadeOutSeconds = 1f;
        [SerializeField, Range(0f, 1f)] [Tooltip("The jazz loop is turned down to this share of its volume while the hook plays. 0 silences it. It multiplies with the turn-down of a case's music.")] float loopVolumeWhileHookPlays;
        [SerializeField, Range(0f, 1f)] [Tooltip("The music of the open case is turned down to this share of its volume while the hook plays. 0 silences it.")] float caseMusicVolumeWhileHookPlays;
        [SerializeField] [Tooltip("On: piano notes still play while the hook plays. The player cannot move then, so only a wrong key before the respawn would make one.")] bool pianoWhileHookPlays;
        [SerializeField, Min(0f)] [Tooltip("Seconds between the death and the hook starting. The music keeps playing until then.")] float startDelaySeconds;
        [SerializeField] [Tooltip("On: the hook plays once and then stays silent until the player can move again. Off: it loops until then.")] bool playHookOnce = true;
        [SerializeField] [Tooltip("On: every death starts the hook from the beginning, also a death while it still fades out. Off: a hook that is still sounding keeps going.")] bool restartHookOnEveryDeath = true;

        public DeathHookSettings()
        {
            loopVolumeWhileHookPlays = 0f;
            caseMusicVolumeWhileHookPlays = 0f;
            pianoWhileHookPlays = false;
            startDelaySeconds = 0f;
            playHookOnce = true;
            restartHookOnEveryDeath = true;
        }

        public DeathHookSettings(float fadeInSeconds, float fadeOutSeconds, float hookVolume, float loopVolumeWhileHookPlays, float caseMusicVolumeWhileHookPlays, bool pianoWhileHookPlays, float startDelaySeconds, bool playHookOnce, bool restartHookOnEveryDeath)
        {
            this.fadeInSeconds = fadeInSeconds;
            this.fadeOutSeconds = fadeOutSeconds;
            this.hookVolume = hookVolume;
            this.loopVolumeWhileHookPlays = loopVolumeWhileHookPlays;
            this.caseMusicVolumeWhileHookPlays = caseMusicVolumeWhileHookPlays;
            this.pianoWhileHookPlays = pianoWhileHookPlays;
            this.startDelaySeconds = startDelaySeconds;
            this.playHookOnce = playHookOnce;
            this.restartHookOnEveryDeath = restartHookOnEveryDeath;
        }

        public AudioClip HookClip => hookClip;
        public float HookVolume => hookVolume;
        public float FadeInSeconds => fadeInSeconds;
        public float FadeOutSeconds => fadeOutSeconds;
        public float LoopVolumeWhileHookPlays => loopVolumeWhileHookPlays;
        public float CaseMusicVolumeWhileHookPlays => caseMusicVolumeWhileHookPlays;
        public bool PianoWhileHookPlays => pianoWhileHookPlays;
        public float StartDelaySeconds => startDelaySeconds;
        public bool PlayHookOnce => playHookOnce;
        public bool RestartHookOnEveryDeath => restartHookOnEveryDeath;
    }
}
