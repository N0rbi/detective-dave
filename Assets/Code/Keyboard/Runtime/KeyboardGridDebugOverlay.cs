using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// Test readout for the grid scene (IMGUI): key labels over the cells, the last key presses, and a status line.
    /// Remove the component when the scene is no longer a test bench.
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView), typeof(KeyboardGridInput))]
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
        const float StatusTopPerScreen = 0.9f;
        const float StatusHeightPerScreen = 0.08f;

        [SerializeField] bool showLabels = true;

        readonly List<string> history = new List<string>();      // newest first
        KeyboardGridView view;
        KeyboardGridInput input;
        string lastKeyText = "-";
        string keyboardLayoutName = "?";
        GUIStyle labelStyle;
        GUIStyle historyStyle;
        GUIStyle statusStyle;

        void Awake()
        {
            view = GetComponent<KeyboardGridView>();
            input = GetComponent<KeyboardGridInput>();
        }

        void OnEnable()
        {
            input.KeyPressed += OnKeyPressed;
            input.CellClicked += OnCellClicked;
        }

        void OnDisable()
        {
            input.KeyPressed -= OnKeyPressed;
            input.CellClicked -= OnCellClicked;
        }

        void OnKeyPressed(KeyPress press)
        {
            keyboardLayoutName = Keyboard.current?.keyboardLayout ?? "?";
            lastKeyText = press.IsMapped ? $"{press.Cell.Def.code} '{press.Cell.Def.label}'" : $"{press.Key} (not in this layout)";
            var target = press.IsMapped ? $"cell '{press.Cell.Def.label}'" : "NOT IN LAYOUT";
            history.Insert(0, $"{press.Key}  ->  {target}   (OS says '{press.DisplayName}')");
            if (history.Count > HistoryLength) history.RemoveAt(history.Count - 1);
        }

        void OnCellClicked(KeyCell cell) => lastKeyText = $"mouse on {cell.Def.code}";

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
            float cellPixels = pixelsPerUnit * view.Scale;
            labelStyle.fontSize = Mathf.Max(MinLabelFontSize, Mathf.RoundToInt(cellPixels * LabelFontPerCell));
            foreach (var cell in view.Cells.Values)
            {
                var screen = cam.WorldToScreenPoint(view.LabelWorldPosition(cell));
                var area = new Rect(screen.x - cellPixels * 0.5f, Screen.height - screen.y - cellPixels * 0.5f, cellPixels, cellPixels);
                labelStyle.normal.textColor = cell.Active ? view.Palette.panel : view.Palette.text;
                GUI.Label(area, cell.Def.label, labelStyle);
            }
        }

        void DrawHistory()
        {
            if (history.Count == 0) return;
            historyStyle.fontSize = Mathf.RoundToInt(Screen.height * HistoryFontPerScreen);
            historyStyle.normal.textColor = new Color(1f, 1f, 1f, 0.8f);
            var area = new Rect(HistoryPadding, HistoryPadding, Screen.width * HistoryWidthPerScreen, Screen.height * HistoryHeightPerScreen);
            GUI.Label(area, string.Join("\n", history), historyStyle);
        }

        void DrawStatus()
        {
            statusStyle.fontSize = Mathf.RoundToInt(Screen.height * StatusFontPerScreen);
            statusStyle.normal.textColor = Color.white;
            var area = new Rect(0f, Screen.height * StatusTopPerScreen, Screen.width, Screen.height * StatusHeightPerScreen);
            GUI.Label(area, $"{view.Layout.name}  |  last key: {lastKeyText}  |  OS keyboard layout: {keyboardLayoutName}", statusStyle);
        }
    }
}
