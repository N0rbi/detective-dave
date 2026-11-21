using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using DetectiveDave.PlayerPlay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    [RequireComponent(typeof(KeyboardBoard), typeof(KeyboardKeySource), typeof(KeyboardGridView))]
    [RequireComponent(typeof(KeyboardGridScreenFit))]
    public sealed class OverworldController : MonoBehaviour, ICaseLauncher
    {
        const float DeskOverscan = 3f;
        const int DeskBackdropSortingOrder = -50;

        [SerializeField] PlayerView playerPrefab;
        [SerializeField] PlayerMovementSettings playerSettings;
        [SerializeField] OverworldSettings overworldSettings;

        DeskLayout deskOverride;
        readonly Dictionary<string, CaseFile> casesById = new Dictionary<string, CaseFile>();
        KeyboardBoard board;
        KeyboardKeySource keys;
        KeyboardGridView gridView;
        DeskLayout desk;
        DeskLevel level;
        PlayerView view;
        PlayerGame game;
        CaseProgress progress;
        DeskCases deskCases;
        OverworldFlow flow;
        IntroSequence intro;
        SpiralScreenWipe wipeView;
        CameraZoomView zoomView;
        ZoomFocus focus;
        DwellRingView ringView;
        SpriteRenderer backdrop;
        string pendingReportCaseId;
        bool subscribed;

        public void UseDesk(DeskLayout prefab) => deskOverride = prefab;

        DeskLayout DeskToBuild => deskOverride ? deskOverride : overworldSettings.Desk;

        void Awake()
        {
            board = GetComponent<KeyboardBoard>();
            keys = GetComponent<KeyboardKeySource>();
            gridView = GetComponent<KeyboardGridView>();
        }

        void Start()
        {
            if (!Build())
            {
                enabled = false;
                return;
            }
            Subscribe();
            ResetToStart();
            intro.Begin(StartPlaying);
        }

        void Update() => flow?.Tick(Time.deltaTime);

        void LateUpdate() => FitBackdrop();

        void OnDisable()
        {
            Unsubscribe();
            intro?.Cancel();
            game?.Stop();
        }

        void OnDestroy()
        {
            if (wipeView) Destroy(wipeView.gameObject);
            if (zoomView) Destroy(zoomView.gameObject);
            if (backdrop) Destroy(backdrop.gameObject);
        }

        public void Launch(string caseId)
        {
            CaseSession.Begin(casesById[caseId]);
            wipeView.Out(view.transform.position, () => SceneLoader.Load(GameScene.Game));
        }

        bool Build()
        {
            if (!playerPrefab || !playerSettings || !overworldSettings)
            {
                Debug.LogError("OverworldController needs a player prefab, player settings and overworld settings.", this);
                return false;
            }
            if (!DeskToBuild)
            {
                Debug.LogError("Overworld Settings has no Desk. Assign one, or run Window > Detective Dave > Set Up Overworld.", overworldSettings);
                return false;
            }
            var missing = overworldSettings.MissingReferences;
            if (missing.Length > 0)
            {
                Debug.LogError("Overworld Settings has empty references: " + missing + ". Run Window > Detective Dave > Set Up Overworld.", overworldSettings);
                return false;
            }
            EnsureAudioListener();
            BuildBackdrop();
            BuildDesk();
            BuildProgress();
            BuildPlayer();
            BuildScreenEffects();
            BuildFlow();
            return true;
        }

        void BuildDesk()
        {
            desk = Instantiate(DeskToBuild, transform, false);
            foreach (var marker in desk.Markers.Where(candidate => candidate.CaseFile)) casesById[marker.CaseFile.Id] = marker.CaseFile;
        }

        static void EnsureAudioListener()
        {
            if (FindAnyObjectByType<AudioListener>() || !Camera.main) return;
            Camera.main.gameObject.AddComponent<AudioListener>();
            Debug.LogWarning("The Overworld scene had no Audio Listener, so one was added to the main camera.");
        }

        void BuildBackdrop()
        {
            backdrop = new GameObject("Desk Backdrop").AddComponent<SpriteRenderer>();
            backdrop.sprite = overworldSettings.DeskTopSprite;
            backdrop.sortingOrder = DeskBackdropSortingOrder;
            FitBackdrop();
        }

        void FitBackdrop()
        {
            if (!backdrop) return;
            var camera = Camera.main;
            var content = new Vector2(gridView.ContentWidth, gridView.ContentHeight);
            var view = camera && camera.orthographic ? new Vector2(2f * camera.orthographicSize * camera.aspect, 2f * camera.orthographicSize) : content;
            var size = Vector2.Max(view, content) * DeskOverscan;
            var spriteSize = backdrop.sprite.bounds.size;
            backdrop.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);
            backdrop.transform.position = camera ? new Vector3(camera.transform.position.x, camera.transform.position.y, 0f) : Vector3.zero;
        }

        void BuildProgress()
        {
            progress = new CaseProgress(JsonProgressStore.AtDefaultPath());
            if (CaseSession.TryTakeSolved(out var solvedId)) pendingReportCaseId = solvedId;
            var keyCentres = gridView.Cells.ToDictionary(cell => cell.Key, cell => (Vector2)cell.Value.Center);
            var map = new DeskKeyMap(desk.Markers.Where(marker => marker.CaseFile).Select(marker => marker.ToDeskMarker(transform)).ToArray(), keyCentres);
            deskCases = new DeskCases(map, StateOf);
            ReportDeskIssues(map);
            ShowCaseStates();
        }

        CaseState StateOf(string caseId) => progress.StateOf(caseId, casesById[caseId].RequiredIds);

        void ShowCaseStates()
        {
            foreach (var marker in desk.Markers.Where(candidate => candidate.CaseFile))
                marker.Show(StateOf(marker.CaseFile.Id), overworldSettings.FolderSprite, overworldSettings.SolvedSprite, overworldSettings.LockedShade);
        }

        void ReportDeskIssues(DeskKeyMap map)
        {
            level = new DeskLevel();
            level.TryGetEnds(out var startKey, out _);
            foreach (var issue in map.Issues(startKey)) Debug.LogWarning($"Desk: {issue.Kind} (case {issue.CaseId}, other {issue.OtherCaseId}, key {issue.Key}).", desk);
        }

        void BuildPlayer()
        {
            view = Instantiate(playerPrefab, transform, false);
            view.gameObject.SetActive(false);
            view.Configure(playerSettings, gridView.Scale);
            var gust = new GameObject("Dash Gust").AddComponent<DashGustView>();
            gust.transform.SetParent(transform, false);
            gust.Configure(playerSettings, gridView.Scale, view.transform);
            var animations = view.GetComponent<IPlayerAnimations>();
            game = new PlayerGame(board, level, animations, view, gust, new PlayerGameSettings(playerSettings.WalkFrames.Length, playerSettings.ClimbFrames.Length, playerSettings.SlipDistanceInKeys, playerSettings.DropMarginInKeys, playerSettings.BumpDistanceInKeys, playerSettings.ForgivingMoves));
        }

        void BuildScreenEffects()
        {
            zoomView = new GameObject("Camera Zoom").AddComponent<CameraZoomView>();
            zoomView.Configure(playerSettings, GetComponent<KeyboardGridScreenFit>());
            wipeView = new GameObject("Screen Wipe").AddComponent<SpiralScreenWipe>();
            wipeView.Configure(playerSettings, zoomView);
            focus = new ZoomFocus(board, playerSettings.EvidenceHandRiseInKeys, playerSettings.ZoomFocusHandShare);
            intro = new IntroSequence(board, level, wipeView, zoomView, focus);
        }

        void BuildFlow()
        {
            ringView = new GameObject("Dwell Ring").AddComponent<DwellRingView>();
            ringView.transform.SetParent(transform, false);
            ringView.Configure(view.transform, overworldSettings.RingSprite, overworldSettings.RingRadiusInKeys * board.KeyWorldSize, overworldSettings.RingWidthInKeys * board.KeyWorldSize, overworldSettings.RingTrackColor, overworldSettings.RingFillColor);
            var sound = new GameObject("Typewriter Sound").AddComponent<TypewriterSound>();
            sound.transform.SetParent(transform, false);
            sound.Configure(overworldSettings);
            var briefing = new GameObject("Briefing").AddComponent<BriefingView>();
            briefing.transform.SetParent(transform, false);
            briefing.Configure(overworldSettings, FindCase, transform, sound);
            flow = new OverworldFlow(deskCases, new DwellTimer(overworldSettings.DwellSeconds), ringView, briefing, this);
        }

        CaseFile FindCase(string caseId) => casesById[caseId];

        void ResetToStart()
        {
            ringView.Hide();
            wipeView.Cancel();
            zoomView.Restore();
            game.Restart();
        }

        void StartPlaying()
        {
            if (level.TryGetEnds(out var startKey, out _)) game.OnKeyPressed(startKey);
            PlayPendingReport();
        }

        void PlayPendingReport()
        {
            var caseId = pendingReportCaseId;
            pendingReportCaseId = null;
            if (string.IsNullOrEmpty(caseId)) return;
            if (!casesById.ContainsKey(caseId))
            {
                progress.MarkSolved(caseId);
                ShowCaseStates();
                return;
            }
            flow.PlayReport(caseId, () => CompleteCase(caseId));
        }

        void CompleteCase(string caseId)
        {
            progress.MarkSolved(caseId);
            ShowCaseStates();
        }

        void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            keys.KeyPressed += OnKeyPressed;
            game.Moved += flow.OnMoved;
            game.StateChanged += flow.OnPlayerStateChanged;
        }

        void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            keys.KeyPressed -= OnKeyPressed;
            game.Moved -= flow.OnMoved;
            game.StateChanged -= flow.OnPlayerStateChanged;
        }

        void OnKeyPressed(Key key)
        {
            if (!intro.IsRunning && flow.AcceptsMoves) game.OnKeyPressed(key);
        }
    }
}
