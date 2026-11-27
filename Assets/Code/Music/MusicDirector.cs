using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DetectiveDave.Diagnostics;
using DetectiveDave.OverworldPlay;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    public sealed class MusicDirector : MonoBehaviour, IPianoVoices, ICaseMusicPlayer, IDeathHookOutput
    {
        const string LogPrefix = "[Music]";
        const double DriftToleranceSeconds = 0.04;
        const int AudioEventCapacity = 256;

        static MusicDirector instance;

        MusicSettings settings;
        AudioClip[] pianoClips;
        IReadOnlyList<SingleNoteClip> singleClips;
        AudioSource loop;
        AudioSource caseSource;
        AudioSource hookSource;
        float loopHookShare = 1f;
        float caseHookShare = 1f;
        float hookVolume;
        CaseMusic caseMusic;
        float loopBaseVolume;
        float loopDuck = 1f;
        float caseVolume;
        Tween caseTween;
        Tween duckTween;
        AudioSource[] voices;
        int nextVoice;
        IAudioEvents audioEvents = new NoAudioEvents();
        bool recordsAudioEvents;

        public IMusicReaction Reaction { get; private set; }

        public DeathHookController DeathHook { get; private set; }

        public bool HasClip => settings.DeathHook.HookClip;

        void StartHookStem()
        {
            var clip = settings.DeathHook.HookClip;
            if (!clip || !loop || !loop.clip) return;
            hookSource.clip = clip;
            hookSource.loop = true;
            hookSource.volume = 0f;
            hookSource.mute = true;
            hookSource.time = (float)LoopFollower.WantedClipTime(LoopSeconds(), loop.clip.length, clip.length, settings.DeathHook.HookSyncOffsetSeconds);
            audioEvents.Record(AudioEventKind.StemStart, clip.name, hookSource.time);
            hookSource.Play();
        }

        void FollowLoopWithHook()
        {
            if (!hookSource || !hookSource.isPlaying || !loop || !loop.clip) return;
            var cycle = Math.Min(loop.clip.length, hookSource.clip.length);
            var wanted = LoopFollower.WantedClipTime(LoopSeconds(), loop.clip.length, hookSource.clip.length, settings.DeathHook.HookSyncOffsetSeconds);
            if (!LoopFollower.NeedsCorrection(hookSource.time, wanted, cycle, DriftToleranceSeconds)) return;
            hookSource.time = (float)wanted;
            audioEvents.Record(AudioEventKind.LoopSeek, hookSource.clip.name, hookSource.time);
        }

        public void Apply(DeathHookMix mix)
        {
            hookVolume = mix.HookVolume;
            loopHookShare = mix.LoopShare;
            caseHookShare = mix.CaseMusicShare;
            ApplyVolumes();
        }

        public IBassPulse BassPulse { get; private set; } = new NoBassPulse();

        public static IBassPulse CurrentBassPulse => instance ? instance.BassPulse : null;

        public static ICaseMusicPlayer CaseMusicPlayer => instance ? instance : NothingToPlay;

        static readonly ICaseMusicPlayer NothingToPlay = new NoCaseMusicPlayer();

        public void FadeIn(CaseMusic music)
        {
            if (!music.HasClip) return;
            if (caseMusic != null && caseMusic.Clip == music.Clip && caseSource.isPlaying) return;
            caseTween?.Kill();
            caseMusic = music;
            caseSource.clip = music.Clip;
            caseSource.loop = true;
            caseSource.time = StartTime(music);
            audioEvents.Record(AudioEventKind.StemStart, music.Clip.name, caseSource.time);
            caseVolume = 0f;
            caseSource.volume = 0f;
            caseSource.Play();
            caseTween = DOTween.To(() => caseVolume, value => caseVolume = value, music.Volume, music.FadeInSeconds).SetUpdate(true).SetLink(gameObject);
            TweenDuck(music.LoopVolumeWhilePlaying, music.FadeInSeconds);
        }

        public void FadeOut()
        {
            if (caseMusic == null || !caseSource.isPlaying) return;
            var seconds = caseMusic.FadeOutSeconds;
            caseMusic = null;
            caseTween?.Kill();
            caseTween = DOTween.To(() => caseVolume, value => caseVolume = value, 0f, seconds).SetUpdate(true).SetLink(gameObject).OnComplete(() => caseSource.Stop());
            TweenDuck(1f, seconds);
        }

        public void StopNow()
        {
            caseTween?.Kill();
            duckTween?.Kill();
            caseMusic = null;
            caseSource.Stop();
            caseVolume = 0f;
            loopDuck = 1f;
            DeathHook.Stop();
        }

        void Update()
        {
            DeathHook.Tick();
            FollowLoopWithHook();
            ApplyVolumes();
            if (!caseSource || !caseSource.isPlaying) return;
            if (caseMusic != null && caseMusic.SyncToLoop) FollowLoop();
        }

        void ApplyVolumes()
        {
            if (loop) loop.volume = loopBaseVolume * loopDuck * loopHookShare;
            if (caseSource && caseSource.isPlaying) caseSource.volume = caseVolume * caseHookShare;
            if (!hookSource) return;
            hookSource.volume = hookVolume;
            hookSource.mute = hookVolume <= 0f;
        }

        float StartTime(CaseMusic music)
        {
            if (!music.SyncToLoop || !loop || !loop.clip) return 0f;
            return (float)LoopFollower.WantedClipTime(LoopSeconds(), loop.clip.length, music.Clip.length);
        }

        void FollowLoop()
        {
            if (!loop || !loop.clip) return;
            var cycle = Math.Min(loop.clip.length, caseSource.clip.length);
            var wanted = LoopFollower.WantedClipTime(LoopSeconds(), loop.clip.length, caseSource.clip.length);
            if (!LoopFollower.NeedsCorrection(caseSource.time, wanted, cycle, DriftToleranceSeconds)) return;
            caseSource.time = (float)wanted;
            audioEvents.Record(AudioEventKind.LoopSeek, caseSource.clip.name, caseSource.time);
        }

        void TweenDuck(float target, float seconds)
        {
            duckTween?.Kill();
            duckTween = DOTween.To(() => loopDuck, value => loopDuck = value, target, seconds).SetUpdate(true).SetLink(gameObject);
        }

        public static MusicDirector Ensure(MusicSettings musicSettings)
        {
            if (instance) return instance;
            var host = new GameObject("Music Director");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<MusicDirector>();
            instance.Configure(musicSettings);
            return instance;
        }

        public void Play(NoteKind kind, int clipIndex, float volume)
        {
            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            var clip = kind == NoteKind.Single ? singleClips[clipIndex].Clip : pianoClips[clipIndex];
            if (recordsAudioEvents && clip.loadState != AudioDataLoadState.Loaded) audioEvents.Record(AudioEventKind.ClipLoadedOnPlay, clip.name, 0f);
            voice.Stop();
            voice.clip = clip;
            voice.volume = Mathf.Clamp01(volume);
            voice.Play();
            if (recordsAudioEvents) audioEvents.Record(AudioEventKind.PianoNote, clip.name, voice.volume);
        }

        void Configure(MusicSettings musicSettings)
        {
            settings = musicSettings;
            pianoClips = NaturalNameOrder.Sorted(settings.PianoClips.Where(clip => clip), clip => clip.name).ToArray();
            singleClips = SingleNoteOrder.Sorted(settings.SingleNoteClips);
            voices = Enumerable.Range(0, settings.PianoPoolSize).Select(_ => NewVoice()).ToArray();
            StartHitchLog();
            if (settings.PreloadClipsOnStart) PreloadClips();
            DeathHook = new DeathHookController(settings.DeathHook, this, () => Time.unscaledTime);
            Reaction = new HookGatedReaction(BuildReaction(), () => DeathHook.PianoAllowed);
            BassPulse = BuildBassPulse();
            StartLoop();
            WarnIfSilent();
            WarnAboutChart();
        }

        void StartHitchLog()
        {
            if (!HitchDetector.IsEnabled(settings.HitchLogThresholdMilliseconds)) return;
            var log = new AudioEventLog(AudioEventCapacity, () => Time.frameCount);
            audioEvents = log;
            recordsAudioEvents = true;
            gameObject.AddComponent<FrameHitchLog>().Configure(settings.HitchLogThresholdMilliseconds, log);
        }

        void PreloadClips()
        {
            foreach (var clip in pianoClips) Preload(clip);
            foreach (var single in singleClips) Preload(single.Clip);
            Preload(settings.DeathHook.HookClip);
        }

        void Preload(AudioClip clip)
        {
            if (!clip || clip.loadState == AudioDataLoadState.Loaded) return;
            if (!clip.LoadAudioData()) Debug.LogWarning($"{LogPrefix} Could not load the audio data of '{clip.name}' at the start: its first note will load it on the main thread.", clip);
        }

        PianoReaction BuildReaction()
        {
            var random = new UnityRandomSource();
            var chart = settings.ChordChart;
            var hasChords = chart && chart.Segments.Count > 0;
            var chordPitches = pianoClips.Select(clip => hasChords ? PitchesOf(chart, clip) : default).ToList();
            var singlePitches = singleClips.Select(single => PitchClassSet.Of(new[] { single.PitchClass })).ToList();
            return new PianoReaction(
                BuildPicker(chart, hasChords, random, chordPitches, "piano clip"),
                BuildPicker(chart, hasChords, random, singlePitches, "single note clip"),
                new DebounceGate(settings.ChordDebounceSeconds, settings.SinglesExtendDebounce),
                new PianoNoteVolume(random, settings.PianoVolume, settings.PianoVolumeVariance, settings.BlockedMoveVolumeShare, settings.MisclickVolumeShare),
                new ClickGate(settings.MinSecondsBetweenPianoNotes),
                this,
                () => Time.unscaledTime,
                hasChords ? ChordMomentNow(chart) : () => ChordMoment.Outside,
                message => Debug.LogWarning($"{LogPrefix} {message}", settings),
                settings.DissonantMisclick ? MisclickPickerFactory.Create(settings) ?? NoMisclickPicker : null);
        }

        static readonly IMisclickPicker NoMisclickPicker = new SilentMisclickPicker();

        IBassPulse BuildBassPulse()
        {
            var bass = settings.BassChart;
            var chart = settings.ChordChart;
            if (!bass || !bass.HasOnsets || !chart || !settings.LoopClip) return new NoBassPulse();
            return new BassPulseSource(bass.CreateClock(chart.LoopLengthBeats, settings.LoopClip.length), LoopSeconds);
        }

        IPianoPicker BuildPicker(ChordChart chart, bool hasChords, UnityRandomSource random, IReadOnlyList<PitchClassSet> pitches, string clipKind)
        {
            if (!hasChords || pitches.Count == 0) return new AnyClipPicker(settings.PianoOrder, random, pitches.Count);
            var palette = new ChordPalette(chart.Segments, pitches);
            var fitting = PianoPickers.For(settings.PianoOrder, random, palette, settings.ChordToneWeight);
            var warning = new WarnOnSilentChordPicker(fitting, palette, message => Debug.LogWarning($"{LogPrefix} {message}", chart), clipKind);
            return new GuaranteedClipPicker(warning, palette, settings.WhenNoClipFits);
        }

        Func<ChordMoment> ChordMomentNow(ChordChart chart)
        {
            var clock = chart.CreateClock();
            return () => clock.MomentAt(LoopSeconds());
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
            loopBaseVolume = settings.FadeInSeconds > 0f ? 0f : settings.LoopVolume;
            caseSource = NewVoice();
            hookSource = NewVoice();
            if (!loop.clip) return;
            audioEvents.Record(AudioEventKind.StemStart, loop.clip.name, 0f);
            loop.Play();
            StartHookStem();
            if (settings.FadeInSeconds > 0f) FadeLoopIn();
        }

        void FadeLoopIn() =>
            DOTween.To(() => loopBaseVolume, volume => loopBaseVolume = volume, settings.LoopVolume, settings.FadeInSeconds).SetUpdate(true).SetLink(gameObject);

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
            if (!settings.DeathHook.HookClip) Debug.LogWarning($"{LogPrefix} Music Settings has no Death Hook clip: dying changes nothing in the music. Run Window > Detective Dave > Set Up Music.", settings);
            if (pianoClips.Length == 0) Debug.LogWarning($"{LogPrefix} Music Settings has no Piano Clips: moves are silent, because there is nothing to play. Run Window > Detective Dave > Set Up Music.", settings);
            if (!settings.ChordChart || settings.ChordChart.Segments.Count == 0) Debug.LogWarning($"{LogPrefix} Music Settings has no Chord Chart with chords: piano notes ignore the chords and follow Piano Order over all clips. Run Window > Detective Dave > Set Up Chord Chart.", settings);
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
