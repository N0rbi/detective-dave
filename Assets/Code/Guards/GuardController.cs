using System.Linq;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.LevelPlay;
using DetectiveDave.PlayerPlay;
using UnityEngine.InputSystem;
using UnityEngine;

namespace DetectiveDave.GuardPlay
{
    [RequireComponent(typeof(PlayerController), typeof(KeyboardBoard), typeof(LevelCellClassifier))]
    [RequireComponent(typeof(KeyboardGridView), typeof(LevelLoader))]
    public sealed class GuardController : MonoBehaviour
    {
        [SerializeField] GuardSettings settings;

        PlayerController player;
        KeyboardBoard board;
        LevelCellClassifier classifier;
        KeyboardGridView gridView;
        LevelLoader loader;
        GuardDirector director;
        GuardPassability passability;
        KeyPathfinder pathfinder;
        bool subscribed;

        void Awake()
        {
            player = GetComponent<PlayerController>();
            board = GetComponent<KeyboardBoard>();
            classifier = GetComponent<LevelCellClassifier>();
            gridView = GetComponent<KeyboardGridView>();
            loader = GetComponent<LevelLoader>();
        }

        void Start()
        {
            if (!settings || !settings.HasValidFrames)
            {
                Debug.LogError("GuardController needs Guard Settings with at least one walk and one chase sprite.", this);
                enabled = false;
                return;
            }
            passability = new GuardPassability(classifier);
            pathfinder = new KeyPathfinder(board, passability.CanStandOn);
            director = new GuardDirector(board, pathfinder, CreateView);
            director.Caught += OnCaught;
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
            if (player.GuardsMayMove) director.Tick(Time.deltaTime, player.PlayerKey);
        }

        IGuardView CreateView() => GuardView.Create(transform, settings, gridView.Scale);

        void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            player.WorldReset += Rebuild;
        }

        void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            player.WorldReset -= Rebuild;
        }

        void Rebuild()
        {
            if (director == null) return;
            var level = loader.Current;
            if (!level)
            {
                director.Clear();
                return;
            }
            var patrols = level.GetComponentsInChildren<GuardPatrol>().Select(patrol => patrol.ToData());
            director.Spawn(GuardSpawns.Build(patrols, pathfinder, passability, settings.PatrolSecondsPerKey, settings.ChaseSecondsPerKey));
        }

        void OnCaught(Key guardKey) => player.CatchPlayer();
    }
}
