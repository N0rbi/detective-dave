using System.Linq;
using DG.Tweening;
using DetectiveDave.OverworldPlay;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    public sealed class MusicDirector : MonoBehaviour, IPianoVoices
    {
        const string LogPrefix = "[Music]";

        static MusicDirector instance;

        MusicSettings settings;
        AudioClip[] pianoClips;
        AudioSource loop;
        AudioSource[] voices;
        int nextVoice;

        public IMusicReaction Reaction { get; private set; }

        public static MusicDirector Ensure(MusicSettings musicSettings)
        {
            if (instance) return instance;
            var host = new GameObject("Music Director");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<MusicDirector>();
            instance.Configure(musicSettings);
            return instance;
        }

        public void Play(int clipIndex, float volume)
        {
            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.clip = pianoClips[clipIndex];
            voice.volume = Mathf.Clamp01(volume);
            voice.Play();
        }

        void Configure(MusicSettings musicSettings)
        {
            settings = musicSettings;
            pianoClips = NaturalNameOrder.Sorted(settings.PianoClips.Where(clip => clip), clip => clip.name).ToArray();
            voices = Enumerable.Range(0, settings.PianoPoolSize).Select(_ => NewVoice()).ToArray();
            Reaction = BuildReaction();
            StartLoop();
            WarnIfSilent();
        }

        PianoReaction BuildReaction()
        {
            var random = new UnityRandomSource();
            return new PianoReaction(
                PianoSequences.For(settings.PianoOrder, random, pianoClips.Length),
                new PianoNoteVolume(random, settings.PianoVolume, settings.PianoVolumeVariance, settings.BlockedMoveVolumeShare),
                new ClickGate(settings.MinSecondsBetweenPianoNotes),
                this,
                () => Time.unscaledTime);
        }

        void StartLoop()
        {
            loop = NewVoice();
            loop.clip = settings.LoopClip;
            loop.loop = true;
            loop.volume = settings.FadeInSeconds > 0f ? 0f : settings.LoopVolume;
            if (!loop.clip) return;
            loop.Play();
            if (settings.FadeInSeconds > 0f) FadeLoopIn();
        }

        void FadeLoopIn() =>
            DOTween.To(() => loop.volume, volume => loop.volume = volume, settings.LoopVolume, settings.FadeInSeconds).SetUpdate(true).SetLink(gameObject);

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
            if (!settings.LoopClip) Debug.LogWarning($"{LogPrefix} Music Settings has no Loop Clip: there is no background music. Run Window > Detective Dave > Set Up Music.", settings);
            if (pianoClips.Length == 0) Debug.LogWarning($"{LogPrefix} Music Settings has no Piano Clips: moves are silent. Run Window > Detective Dave > Set Up Music.", settings);
            if (!FindAnyObjectByType<AudioListener>()) Debug.LogWarning($"{LogPrefix} The scene has no Audio Listener: nothing can be heard. Add one to the main camera.");
        }
    }
}
