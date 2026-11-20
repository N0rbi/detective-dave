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
        const float DeskOverscan = 2f;

        [SerializeField] DeskLayout deskPrefab;
        [SerializeField] PlayerView playerPrefab;
        [SerializeField] PlayerMovementSettings playerSettings;
        [SerializeField] OverworldSettings overworldSettings;

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
        bool subscribed;

        public void UseDesk(DeskLayout prefab) => deskPrefab = prefab;

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
            game.Restart();
            intro.Begin(StartPlaying);
        }

        void Update() => flow?.Tick(Time.deltaTime);

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
        }

        public void Launch(string caseId)
        {
            CaseSession.Begin(casesById[caseId]);
            wipeView.Out(view.transform.position, () => SceneLoader.Load(GameScene.Game));
        }

        bool Build()
        {
            if (!deskPrefab || !playerPrefab || !playerSettings || !overworldSettings)
            {
                Debug.LogError("OverworldController needs a desk, a player prefab, player settings and overworld settings.", this);
                return false;
            }
            BuildDesk();
            BuildProgress();
            BuildPlayer();
            BuildScreenEffects();
            BuildFlow();
            return true;
        }

        void BuildDesk()
        {
            desk = Instantiate(deskPrefab, transform, false);
            desk.DrawTop(new Vector2(gridView.ContentWidth, gridView.ContentHeight) * DeskOverscan, overworldSettings.DeskColor);
            foreach (var marker in desk.Markers.Where(candidate => candidate.CaseFile)) casesById[marker.CaseFile.Id] = marker.CaseFile;
        }

        void BuildProgress()
        {
            progress = new CaseProgress(JsonProgressStore.AtDefaultPath());
            if (CaseSession.TryTakeSolved(out var solvedId)) progress.MarkSolved(solvedId);
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
            gust.Configure(playerSettings, gridView.Scale);
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
            ringView.Configure(view.transform, overworldSettings.RingRadiusInKeys * board.KeyWorldSize, overworldSettings.RingWidthInKeys * board.KeyWorldSize, overworldSettings.RingTrackColor, overworldSettings.RingFillColor);
            var sound = new GameObject("Typewriter Sound").AddComponent<TypewriterSound>();
            sound.transform.SetParent(transform, false);
            sound.Configure(overworldSettings.ClickVolume);
            var briefing = new GameObject("Briefing").AddComponent<BriefingView>();
            briefing.transform.SetParent(transform, false);
            briefing.Configure(overworldSettings, FindCase, transform, sound);
            flow = new OverworldFlow(deskCases, new DwellTimer(overworldSettings.DwellSeconds), ringView, briefing, this);
        }

        CaseFile FindCase(string caseId) => casesById[caseId];

        void StartPlaying()
        {
            if (level.TryGetEnds(out var startKey, out _)) game.OnKeyPressed(startKey);
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
