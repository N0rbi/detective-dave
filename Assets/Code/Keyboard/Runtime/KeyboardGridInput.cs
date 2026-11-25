using System.Collections.Generic;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// The test bench's reading of the board, driven only by the key source's events: a cell is lit from <c>KeyPressed</c> until <c>KeyReleased</c>
    /// and optionally the neighbours of lit cells are tinted.
    /// Writes <see cref="KeyCell.Active"/> and <see cref="KeyCell.Neighbour"/> (view state), draws nothing.
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView), typeof(KeyboardBoard), typeof(KeyboardKeySource))]
    public class KeyboardGridInput : MonoBehaviour
    {
        [SerializeField] bool showNeighbours = true;             // tint the keys that touch a lit key
        [SerializeField, Min(0f)] [Tooltip("Caps Lock stays pressed while it is switched on, so it is drawn lit, with its neighbours, for at most this many seconds after each press, like every other key. 0 lights it only on the press frame. Drawing only: the key events are not changed.")] float capsLockMaxLitSeconds = 0.6f;

        readonly HashSet<Key> pressed = new HashSet<Key>();      // between the source's pressed and released events
        readonly List<Key> lit = new List<Key>();
        KeyboardGridView view;
        KeyboardBoard board;
        KeyboardKeySource source;
        CapsLockLitTimer capsLock;

        void Awake()
        {
            view = GetComponent<KeyboardGridView>();
            board = GetComponent<KeyboardBoard>();
            source = GetComponent<KeyboardKeySource>();
            capsLock = new CapsLockLitTimer(() => Time.unscaledTime, capsLockMaxLitSeconds);
            // A scene saved before these components existed lacks them (RequireComponent only adds on Add Component): say how to fix it, do not throw.
            if (!board || !source)
            {
                Debug.LogError($"{GetType().Name} on '{name}' needs {nameof(KeyboardBoard)} and {nameof(KeyboardKeySource)}: add {nameof(KeyboardBoard)} to this object (it brings {nameof(KeyboardKeySource)}).", this);
                enabled = false;
            }
        }

        void OnEnable()
        {
            source.KeyPressed += OnKeyPressed;
            source.KeyReleased += OnKeyReleased;
        }

        void OnDisable()
        {
            source.KeyPressed -= OnKeyPressed;
            source.KeyReleased -= OnKeyReleased;
        }

        void Update()
        {
            if (view.Layout == null) return;
            foreach (var cell in view.Cells.Values)
                cell.Active = !cell.Def.IsInert && IsDrawnLit(cell.InputKey);
            MarkNeighbours();
            capsLock.EndFrame();
        }

        bool IsDrawnLit(Key key) => pressed.Contains(key) && (key != Key.CapsLock || capsLock.IsLit);

        void OnKeyPressed(Key key)
        {
            pressed.Add(key);
            if (key == Key.CapsLock) capsLock.OnPressed();
        }

        void OnKeyReleased(Key key)
        {
            pressed.Remove(key);
            if (key == Key.CapsLock) capsLock.OnReleased();
        }

        void MarkNeighbours()
        {
            lit.Clear();
            foreach (var cell in view.Cells.Values)
                if (cell.Active) lit.Add(cell.InputKey);
            foreach (var cell in view.Cells.Values)
                cell.Neighbour = showNeighbours && !cell.Active && TouchesAny(cell.InputKey);
        }

        bool TouchesAny(Key key)
        {
            foreach (var other in lit)
                if (board.IsNeighbour(other, key)) return true;
            return false;
        }
    }
}
