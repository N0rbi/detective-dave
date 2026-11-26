using System.Linq;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.LevelPlay;
using DetectiveDave.MusicPlay;
using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.DogPlay
{
    [RequireComponent(typeof(PlayerController), typeof(KeyboardBoard), typeof(LevelCellClassifier))]
    [RequireComponent(typeof(KeyboardGridView), typeof(LevelLoader))]
    public sealed class DogController : MonoBehaviour
    {
        [SerializeField] DogSettings settings;

        PlayerController player;
        KeyboardBoard board;
        LevelCellClassifier classifier;
        KeyboardGridView gridView;
        DogDirector director;
        float clock;
        bool subscribed;
        bool warnedAboutBass;

        const float SecondsBeforeBassWarning = 2f;

        void Awake()
        {
            player = GetComponent<PlayerController>();
            board = GetComponent<KeyboardBoard>();
            classifier = GetComponent<LevelCellClassifier>();
            gridView = GetComponent<KeyboardGridView>();
        }

        void Start()
        {
            if (!settings || !settings.HasValidFrames)
            {
                Debug.LogError("DogController needs Dog Settings with at least one angry and one calm sprite. Run Window > Detective Dave > Set Up Dogs.", this);
                enabled = false;
                return;
            }
            director = new DogDirector(board, CreateView, new UnityRandomSource(), settings.Pacing);
            Subscribe();
            Rebuild();
        }

        void OnEnable()
        {
            if (director == null) return;
            Subscribe();
            Rebuild();
        }

        void OnDisable()
        {
            Unsubscribe();
            director?.Clear();
        }

        void Update()
        {
            clock += Time.deltaTime;
            if (!player.HasPlayer) return;
            if (UsesBass(out var pulse)) director.HearBassNotes(pulse.Poll(), player.PlayerKey);
            else director.ShowStep(DogFrames.StepAt(clock, settings.FramesPerSecond), player.PlayerKey);
        }

        bool UsesBass(out IBassPulse pulse)
        {
            pulse = null;
            if (settings.StepMode != DogStepMode.BassNotes) return false;
            pulse = MusicDirector.CurrentBassPulse;
            if (pulse != null && pulse.IsAvailable) return true;
            WarnAboutMissingBass();
            return false;
        }

        void WarnAboutMissingBass()
        {
            if (warnedAboutBass || clock < SecondsBeforeBassWarning) return;
            warnedAboutBass = true;
            Debug.LogWarning("[Dogs] Dog Settings steps the dogs on bass notes, but Music Settings has no Bass Chart with notes, so the dogs use the timer. Drop the upright bass .aif or .wav into Assets/Staging, run tools/bass_onsets.py, then Window > Detective Dave > Set Up Bass Chart.", settings);
        }

        IDogView CreateView() => DogView.Create(transform, settings, gridView.Scale);

        void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            player.WorldReset += Rebuild;
            player.PlayerMoved += ShowMoods;
        }

        void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            player.WorldReset -= Rebuild;
            player.PlayerMoved -= ShowMoods;
        }

        void Rebuild()
        {
            if (director == null || !player.HasPlayer) return;
            director.Spawn(GapKeys(), player.PlayerKey);
        }

        void ShowMoods(Key playerKey) => director.Draw(playerKey);

        Key[] GapKeys() => gridView.Cells.Keys.Where(key => classifier.KindOf(key) == CellKind.Gap).ToArray();
    }
}
