using System.Collections.Generic;
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

        public void Play(int clipIndex, float volume, double delaySeconds)
        {
            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.Stop();
            voice.clip = pianoClips[clipIndex];
            voice.volume = Mathf.Clamp01(volume);
            if (delaySeconds <= 0.0) voice.Play();
            else voice.PlayScheduled(AudioSettings.dspTime + delaySeconds);
        }

        void Configure(MusicSettings musicSettings)
        {
            settings = musicSettings;
            pianoClips = NaturalNameOrder.Sorted(settings.PianoClips.Where(clip => clip), clip => clip.name).ToArray();
            voices = Enumerable.Range(0, settings.PianoPoolSize).Select(_ => NewVoice()).ToArray();
            Reaction = settings.ChordChart ? (IMusicReaction)BuildReaction(settings.ChordChart) : new NoPianoReaction();
            StartLoop();
            WarnIfSilent();
            WarnAboutChart();
        }

        PianoReaction BuildReaction(ChordChart chart)
        {
            var random = new UnityRandomSource();
            var clock = chart.CreateClock();
            var palette = new ChordPalette(chart.Segments, pianoClips.Select(clip => PitchesOf(chart, clip)).ToList());
            var picker = new WarnOnSilentChordPicker(PianoPickers.For(settings.PianoOrder, random, palette, settings.ChordToneWeight), palette, message => Debug.LogWarning($"{LogPrefix} {message}", chart));
            var quantizer = new NoteQuantizer(clock, settings.QuantizeGrid, settings.MaxDelaySeconds, settings.LookAheadSeconds, settings.DropExtraNotesInSlot);
            return new PianoReaction(
                picker,
                new PianoNoteVolume(random, settings.PianoVolume, settings.PianoVolumeVariance, settings.BlockedMoveVolumeShare),
                new ClickGate(settings.MinSecondsBetweenPianoNotes),
                this,
                () => Time.unscaledTime,
                new ChordNoteTiming(clock, quantizer, LoopSeconds));
        }

        static PitchClassSet PitchesOf(ChordChart chart, AudioClip clip)
        {
            var entry = chart.ClipPitches.FirstOrDefault(pitch => pitch.Clip == clip);
            return entry == null ? default : entry.PitchClasses;
        }

        double LoopSeconds() => loop && loop.clip ? loop.timeSamples / (double)loop.clip.frequency : 0.0;

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
            if (!settings.ChordChart) Debug.LogWarning($"{LogPrefix} Music Settings has no Chord Chart: moves are silent. Run Window > Detective Dave > Set Up Chord Chart.", settings);
            if (!FindAnyObjectByType<AudioListener>()) Debug.LogWarning($"{LogPrefix} The scene has no Audio Listener: nothing can be heard. Add one to the main camera.");
        }

        void WarnAboutChart()
        {
            var chart = settings.ChordChart;
            if (!chart) return;
            var loopSeconds = settings.LoopClip ? settings.LoopClip.length : 0f;
            foreach (var problem in ChordChartValidation.Problems(chart, loopSeconds)) Debug.LogWarning($"{LogPrefix} Chord Chart: {problem}", chart);
            foreach (var clip in UnmappedClips(chart)) Debug.LogWarning($"{LogPrefix} Chord Chart: {clip.name} has no entry in Clip Pitches, so it never plays.", chart);
        }

        IEnumerable<AudioClip> UnmappedClips(ChordChart chart) =>
            pianoClips.Where(clip => chart.ClipPitches.All(pitch => pitch.Clip != clip));
    }
}
