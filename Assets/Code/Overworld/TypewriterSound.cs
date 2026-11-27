using System.Linq;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class TypewriterSound : MonoBehaviour
    {
        const int VoiceCount = 6;
        const string LogPrefix = "[Overworld sound]";

        OverworldSettings settings;
        AudioClip[] clicks;
        AudioSource[] voices;
        StrikeVariance variance;
        ClickGate descriptionGate;
        IRandomSource random;
        int nextVoice;

        public void Configure(OverworldSettings overworldSettings)
        {
            settings = overworldSettings;
            clicks = settings.TypewriterClicks.Where(clip => clip).ToArray();
            random = new UnityRandomSource();
            variance = new StrikeVariance(random, clicks.Length, settings.ClickPitchMin, settings.ClickPitchMax, settings.ClickVolumeShareMin, settings.ClickVolumeShareMax);
            descriptionGate = new ClickGate(settings.MinSecondsBetweenClicks);
            voices = Enumerable.Range(0, VoiceCount).Select(_ => NewVoice()).ToArray();
            WarnIfSilent();
        }

        public void PlayKey() => PlayStrike(1f);

        public void PlayDescriptionKey()
        {
            if (!settings.DescriptionClicks) return;
            if (!descriptionGate.TryPass(Time.unscaledTime)) return;
            PlayStrike(settings.DescriptionClickShare);
        }

        public void PlayBackspace()
        {
            if (!settings.BackspaceClip) return;
            float pitch = random.Range(settings.ClickPitchMin, settings.ClickPitchMax);
            float volumeShare = random.Range(settings.ClickVolumeShareMin, settings.ClickVolumeShareMax);
            Play(settings.BackspaceClip, pitch, settings.ClickVolume * volumeShare);
        }

        void PlayStrike(float volumeShare)
        {
            if (clicks.Length == 0) return;
            var strike = variance.Next();
            Play(clicks[strike.ClipIndex], strike.Pitch, settings.ClickVolume * strike.VolumeShare * volumeShare);
        }

        void Play(AudioClip clip, float pitch, float volume)
        {
            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.clip = clip;
            voice.pitch = pitch;
            voice.volume = Mathf.Clamp01(volume);
            voice.Play();
        }

        AudioSource NewVoice()
        {
            var voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0f;
            return voice;
        }

        void WarnIfSilent()
        {
            if (clicks.Length == 0) Debug.LogWarning($"{LogPrefix} No typewriter click clips are assigned on the Overworld Settings: typing is silent. Run Window > Detective Dave > Set Up Overworld.");
            if (!FindAnyObjectByType<AudioListener>()) Debug.LogWarning($"{LogPrefix} The scene has no Audio Listener: nothing can be heard. Add one to the main camera.");
            if (settings.ClickVolume <= 0f) Debug.LogWarning($"{LogPrefix} Click Volume on the Overworld Settings is 0.");
        }
    }
}
