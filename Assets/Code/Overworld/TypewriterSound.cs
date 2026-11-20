using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class TypewriterSound : MonoBehaviour
    {
        OverworldSettings settings;
        AudioSource source;

        public void Configure(OverworldSettings overworldSettings)
        {
            settings = overworldSettings;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        public void PlayKey() => PlayClick(1f);

        public void PlayDescriptionKey()
        {
            if (settings.DescriptionClicks) PlayClick(settings.DescriptionClickShare);
        }

        public void PlayBackspace()
        {
            if (settings.BackspaceClip) Play(settings.BackspaceClip, 1f, 1f);
        }

        void PlayClick(float volumeShare)
        {
            var clip = PickClick();
            if (!clip) return;
            Play(clip, volumeShare, Random.Range(settings.ClickPitchMin, settings.ClickPitchMax));
        }

        void Play(AudioClip clip, float volumeShare, float pitch)
        {
            source.pitch = pitch;
            source.PlayOneShot(clip, settings.ClickVolume * volumeShare);
        }

        AudioClip PickClick()
        {
            var clips = settings.TypewriterClicks;
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }
    }
}
