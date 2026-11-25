using System.Linq;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.LevelPlay;
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
            director = new DogDirector(board, CreateView);
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
            if (player.HasPlayer) director.ShowStep(DogFrames.StepAt(clock, settings.FramesPerSecond), player.PlayerKey);
        }

        void LateUpdate()
        {
            if (settings.HideKeyArt) HideKeyArtUnderDogs();
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

        void HideKeyArtUnderDogs()
        {
            foreach (var key in director.Keys)
            {
                if (!gridView.Cells.TryGetValue(key, out var cell)) continue;
                foreach (var part in cell.Parts) part.enabled = false;
            }
        }
    }
}
