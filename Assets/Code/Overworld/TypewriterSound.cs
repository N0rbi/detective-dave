using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class TypewriterSound : MonoBehaviour
    {
        const int SampleRate = 22050;
        const float ClickSeconds = 0.05f;
        const string ClipName = "PLACEHOLDER typewriter click";

        AudioSource source;
        AudioClip clip;
        float volume;

        public void Configure(float clickVolume) => volume = clickVolume;

        public void Play()
        {
            EnsureSource();
            source.PlayOneShot(clip, volume);
        }

        void OnDestroy()
        {
            if (clip) Destroy(clip);
        }

        void EnsureSource()
        {
            if (source) return;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            var samples = ClickSamples.Generate(SampleRate, ClickSeconds);
            clip = AudioClip.Create(ClipName, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
        }
    }
}
