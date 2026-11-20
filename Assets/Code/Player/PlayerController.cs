using System;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using DetectiveDave.LevelPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.PlayerPlay
{
    [RequireComponent(typeof(KeyboardBoard), typeof(KeyboardKeySource), typeof(LevelCellClassifier))]
    [RequireComponent(typeof(KeyboardGridView), typeof(LevelMessageView), typeof(LevelLoader))]
    public sealed class PlayerController : MonoBehaviour
    {
        const string EndsMissingMessage = "This keyboard is missing a Shift key, so this level cannot start.";

        [SerializeField] PlayerView playerPrefab;
        [SerializeField] PlayerMovementSettings settings;

        IKeyboardBoard board;
        IKeyEventSource keys;
        IPlayerLevel level;
        LevelMessageView messages;
        KeyboardGridView gridView;
        PlayerView view;
        IPlayerAnimations animations;
        PlayerGame game;
        EndDecorationsDirector decorations;
        GoalSequence goalSequence;
        IntroSequence introSequence;
        SpiralScreenWipe wipeView;
        CameraZoomView zoomView;
        bool subscribed;

        public event Action GoalReached;

        void Awake()
        {
            board = GetComponent<KeyboardBoard>();
            keys = GetComponent<KeyboardKeySource>();
            level = GetComponent<LevelCellClassifier>();
            messages = GetComponent<LevelMessageView>();
            gridView = GetComponent<KeyboardGridView>();
        }

        void Start()
        {
            if (!Build())
            {
                enabled = false;
                return;
            }
            RestartGame();
        }

        void OnEnable()
        {
            if (game == null) return;
            RestartGame();
        }

        void OnDisable()
        {
            Unsubscribe();
            goalSequence?.Cancel();
            game?.Stop();
            decorations?.Clear();
        }

        void OnDestroy()
        {
            if (wipeView) Destroy(wipeView.gameObject);
            if (zoomView) Destroy(zoomView.gameObject);
        }

        void RestartGame()
        {
            Subscribe();
            game.Restart();
            decorations.Refresh(game.IsActive);
            UpdateMessage();
            introSequence.Begin();
        }

        bool Build()
        {
            if (!playerPrefab || !settings)
            {
                ReportProblem("PlayerController needs a player prefab and player settings.");
                return false;
            }
            if (!settings.HasValidWalkFrames)
            {
                ReportProblem("PlayerMovementSettings: walkFrames needs at least two non-null sprites.");
                return false;
            }
            if (!settings.HasValidClimbFrames)
            {
                ReportProblem("PlayerMovementSettings: climbFrames needs at least two non-null sprites.");
                return false;
            }
            CreateView();
            if (!ResolveAnimations())
            {
                Destroy(view.gameObject);
                return false;
            }
            BuildGame();
            BuildDecorations();
            BuildGoalSequence();
            return true;
        }

        void ReportProblem(string problem)
        {
            Debug.LogError(problem, this);
            if (messages) messages.PlayerMessage = problem;
        }

        void CreateView()
        {
            view = Instantiate(playerPrefab, transform, false);
            view.gameObject.SetActive(false);
            view.Configure(settings, gridView.Scale);
        }

        bool ResolveAnimations()
        {
            animations = view.GetComponent<IPlayerAnimations>();
            if (animations != null) return true;
            ReportProblem("The player prefab has no IPlayerAnimations component.");
            return false;
        }

        void BuildGame()
        {
            game = new PlayerGame(board, level, animations, view, new PlayerGameSettings(settings.WalkFrames.Length, settings.ClimbFrames.Length, settings.SlipDistanceInKeys, settings.DropMarginInKeys, settings.BumpDistanceInKeys));
            game.GoalReached += ForwardGoalReached;
        }

        void BuildDecorations()
        {
            var decorationsView = new GameObject("End Decorations").AddComponent<EndDecorationsView>();
            decorationsView.transform.SetParent(transform, false);
            decorationsView.Configure(settings, gridView.Scale);
            decorations = new EndDecorationsDirector(board, level, decorationsView, settings.GoalBurstCentreInKeys);
        }

        void BuildGoalSequence()
        {
            var evidenceView = new GameObject("Evidence View").AddComponent<EvidenceView>();
            evidenceView.transform.SetParent(transform, false);
            evidenceView.Configure(settings, gridView.Scale);
            zoomView = new GameObject("Camera Zoom").AddComponent<CameraZoomView>();
            zoomView.Configure(settings, GetComponent<KeyboardGridScreenFit>());
            wipeView = new GameObject("Screen Wipe").AddComponent<SpiralScreenWipe>();
            wipeView.Configure(settings, zoomView);
            var focus = new ZoomFocus(board, settings.EvidenceHandRiseInKeys, settings.ZoomFocusHandShare);
            introSequence = new IntroSequence(board, level, wipeView, zoomView, focus);
            goalSequence = new GoalSequence(board, level, decorations, evidenceView, zoomView, focus, wipeView, new LoaderAdvance(GetComponent<LevelLoader>()), introSequence, settings.EvidenceHandRiseInKeys);
        }

        void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            keys.KeyPressed += OnKeyPressed;
            level.LevelChanged += OnLevelChanged;
        }

        void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            keys.KeyPressed -= OnKeyPressed;
            level.LevelChanged -= OnLevelChanged;
        }

        void OnKeyPressed(Key key)
        {
            if (!goalSequence.IsRunning && !introSequence.IsRunning) game.OnKeyPressed(key);
        }

        void OnLevelChanged()
        {
            goalSequence.HandleLevelChanged();
            game.Restart();
            decorations.Refresh(game.IsActive);
            UpdateMessage();
            if (!goalSequence.IsAdvancing) introSequence.Begin();
        }

        void UpdateMessage()
        {
            if (messages) messages.PlayerMessage = game.EndsMissing ? EndsMissingMessage : null;
        }

        void ForwardGoalReached()
        {
            goalSequence.Begin();
            GoalReached?.Invoke();
        }
    }
}
