using System.Collections.Generic;
using System.IO;
using System.Linq;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Test readout for the grid scene (IMGUI): key labels over the cells, the last key presses, and a status line.
    /// Remove the component when the scene is no longer a test bench.
    /// </summary>
    [DefaultExecutionOrder(100)]     // after the grid view, so the colors it reads are this frame's
    [RequireComponent(typeof(KeyboardGridView), typeof(KeyboardBoard), typeof(KeyboardKeySource))]
    public class KeyboardGridDebugOverlay : MonoBehaviour
    {
        const int HistoryLength = 6;
        const int MinLabelFontSize = 8;
        const float LabelFontPerCell = 0.42f;            // label font size as a share of the cell size in pixels
        const float HistoryFontPerScreen = 0.022f;       // the rest are shares of the screen height or width
        const float StatusFontPerScreen = 0.03f;
        const float HistoryWidthPerScreen = 0.6f;
        const float HistoryHeightPerScreen = 0.25f;
        const float HistoryPadding = 12f;
        const float StatusTopPerScreen = 0.86f;
        const float StatusHeightPerScreen = 0.13f;
        const float FrameWindowSeconds = 1f;

        [SerializeField] bool showLabels = true;
        [SerializeField, Min(0)] int targetFrameRate = 60;      // 0 leaves the frame rate alone. The trace showed ~450 fps in the editor, which may be what makes keys look unlit
        [SerializeField] bool writeTrace;      // write Temp/key-trace.log in the project: every change of a key's state, to find lighting bugs. Each line is flushed at once, so keep it off outside a lighting hunt

        readonly List<string> history = new List<string>();      // newest first
        readonly HashSet<Key> sourceDown = new HashSet<Key>();     // between the source's pressed and released events
        KeyboardGridView view;
        KeyboardBoard board;
        KeyboardKeySource source;
        string lastKeyText = "-";
        string keyboardLayoutName = "?";
        readonly Dictionary<Key, int> traced = new Dictionary<Key, int>();
        KeyTraceWriter trace;
        float windowStart;
        float worstFrame;
        float shownWorstFrame;
        GUIStyle labelStyle;
        GUIStyle historyStyle;
        GUIStyle statusStyle;

        void Awake()
        {
            if (targetFrameRate > 0)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = targetFrameRate;
            }
            view = GetComponent<KeyboardGridView>();
            board = GetComponent<KeyboardBoard>();
            source = GetComponent<KeyboardKeySource>();
            // A scene saved before these components existed lacks them (RequireComponent only adds on Add Component): say how to fix it, do not throw.
            if (!board || !source)
            {
                Debug.LogError($"{GetType().Name} on '{name}' needs {nameof(KeyboardBoard)} and {nameof(KeyboardKeySource)}: add {nameof(KeyboardBoard)} to this object (it brings {nameof(KeyboardKeySource)}).", this);
                enabled = false;
            }
        }

        void OnEnable()
        {
            if (writeTrace) StartTrace();
            source.KeyPressed += OnSourcePressed;
            source.KeyReleased += OnSourceReleased;
            source.RawKeyPressed += OnRawKeyPressed;
        }

        void OnDisable()
        {
            source.KeyPressed -= OnSourcePressed;
            source.KeyReleased -= OnSourceReleased;
            source.RawKeyPressed -= OnRawKeyPressed;
            trace?.Dispose();
            trace = null;
        }

        // The slowest frame of the last second: a long frame is how a short tap can be missed.
        void Update()
        {
            worstFrame = Mathf.Max(worstFrame, Time.unscaledDeltaTime);
            if (Time.unscaledTime - windowStart < FrameWindowSeconds) return;
            shownWorstFrame = worstFrame;
            worstFrame = 0f;
            windowStart = Time.unscaledTime;
        }

        // Per key one line whenever one of these changes: the Input System says it is down, the key source holds it, the input component marks it lit,
        // its sprite really has the lit color, it is tinted as a neighbour, its first sprite is visible and drawn 9-sliced. A key that stays dark or blinks shows up as a line where one of them disagrees.
        void LateUpdate()
        {
            if (trace == null || view.Layout == null) return;
            var keyboard = Keyboard.current;
            foreach (var cell in view.Cells.Values)
            {
                var part = cell.Parts[0];
                int state = (IsRawDown(keyboard, cell) ? 1 : 0) | (sourceDown.Contains(cell.InputKey) ? 2 : 0)
                          | (cell.Active ? 4 : 0) | (part.color == view.Palette.accent ? 8 : 0)
                          | (cell.Neighbour ? 16 : 0) | (part.enabled ? 32 : 0) | (part.drawMode == SpriteDrawMode.Sliced ? 64 : 0);
                traced.TryGetValue(cell.InputKey, out var before);
                if (state == before) continue;
                traced[cell.InputKey] = state;
                trace.Log($"{cell.InputKey,-12} raw={state & 1} board={(state >> 1) & 1} lit={(state >> 2) & 1} drawn={(state >> 3) & 1} neighbour={(state >> 4) & 1} visible={(state >> 5) & 1} sliced={(state >> 6) & 1}");
            }
        }

        void OnApplicationFocus(bool focused) => trace?.Log($"application focus: {focused}");

        void OnSourcePressed(Key key)
        {
            sourceDown.Add(key);
            trace?.Log($"{key,-12} board event: pressed");
        }

        void OnSourceReleased(Key key)
        {
            sourceDown.Remove(key);
            trace?.Log($"{key,-12} board event: released");
        }

        static bool IsRawDown(Keyboard keyboard, KeyCell cell) =>
            keyboard != null && (keyboard[cell.InputKey].isPressed || AnyPressed(keyboard, cell.Def.AlsoKeys));

        static bool AnyPressed(Keyboard keyboard, IReadOnlyList<Key> keys)
        {
            foreach (var key in keys)
                if (keyboard[key].isPressed) return true;
            return false;
        }

        void StartTrace()
        {
            var file = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "key-trace.log"));
            trace = new KeyTraceWriter(file);
            trace.Log($"trace started, Unity {Application.unityVersion}, OS layout '{Keyboard.current?.keyboardLayout}'");
        }

        void OnRawKeyPressed(KeyPress press)
        {
            keyboardLayoutName = Keyboard.current?.keyboardLayout ?? "?";
            lastKeyText = press.IsMapped ? $"{press.Def.Code} '{press.Def.Label}'" : $"{press.Key} (not in this layout)";
            var target = press.IsMapped ? $"cell '{press.Def.Label}'  [{Describe(press.Key)}]" : "NOT IN LAYOUT";
            history.Insert(0, $"{press.Key}  ->  {target}   (OS says '{press.DisplayName}')");
            if (history.Count > HistoryLength) history.RemoveAt(history.Count - 1);
        }

        // What the board says about a key: how many keys touch it. (Its tile is level data: the level glue can show that.)
        string Describe(Key key) => $"{board.GetNeighbours(key).Count} neighbours";

        void OnGUI()
        {
            var cam = Camera.main;
            if (view.Layout == null || !cam) return;
            EnsureStyles();
            if (showLabels) DrawLabels(cam);
            DrawHistory();
            DrawStatus();
        }

        void EnsureStyles()
        {
            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, wordWrap = false, clipping = TextClipping.Overflow,
                padding = new RectOffset(), margin = new RectOffset()
            };
            historyStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft, wordWrap = false };
            statusStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        }

        void DrawLabels(Camera cam)
        {
            float pixelsPerUnit = Screen.height / (2f * cam.orthographicSize);   // the camera shows 2 x orthographicSize units vertically
            float cellPixels = pixelsPerUnit * view.Scale * view.transform.lossyScale.x;
            labelStyle.fontSize = Mathf.Max(MinLabelFontSize, Mathf.RoundToInt(cellPixels * LabelFontPerCell));
            foreach (var cell in view.Cells.Values)
            {
                var screen = cam.WorldToScreenPoint(view.CenterWorldPosition(cell));
                float cellHeightPixels = cellPixels * view.HeightStretch;
                var area = new Rect(screen.x - cellPixels * 0.5f, Screen.height - screen.y - cellHeightPixels * 0.5f, cellPixels, cellHeightPixels);
                // IMGUI swaps in the hover / active / focused text color when the cursor is over a label, so set all of them.
                var textColor = cell.Active ? view.Palette.panel : view.Palette.text;
                labelStyle.normal.textColor = labelStyle.hover.textColor = labelStyle.active.textColor = labelStyle.focused.textColor = textColor;
                GUI.Label(area, cell.Def.Label, labelStyle);
            }
        }

        void DrawHistory()
        {
            if (history.Count == 0) return;
            historyStyle.fontSize = Mathf.RoundToInt(Screen.height * HistoryFontPerScreen);
            historyStyle.normal.textColor = historyStyle.hover.textColor = new Color(1f, 1f, 1f, 0.8f);
            var area = new Rect(HistoryPadding, HistoryPadding, Screen.width * HistoryWidthPerScreen, Screen.height * HistoryHeightPerScreen);
            GUI.Label(area, string.Join("\n", history), historyStyle);
        }

        void DrawStatus()
        {
            statusStyle.fontSize = Mathf.RoundToInt(Screen.height * StatusFontPerScreen);
            statusStyle.normal.textColor = statusStyle.hover.textColor = Color.white;
            var area = new Rect(0f, Screen.height * StatusTopPerScreen, Screen.width, Screen.height * StatusHeightPerScreen);
            GUI.Label(area, $"{view.Layout.Name}  |  last key: {lastKeyText}  |  OS keyboard layout: {keyboardLayoutName}\n" +
                            $"board holds: [{string.Join(", ", sourceDown)}]  |  drawn lit: [{string.Join(", ", view.Cells.Values.Where(cell => cell.Active).Select(cell => cell.Def.Label))}]\n" +
                            $"slowest frame in the last second: {shownWorstFrame * 1000f:0} ms", statusStyle);
        }
    }
}
