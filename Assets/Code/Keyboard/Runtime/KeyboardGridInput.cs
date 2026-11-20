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

        readonly HashSet<Key> pressed = new HashSet<Key>();      // between the source's pressed and released events
        readonly List<Key> lit = new List<Key>();
        KeyboardGridView view;
        KeyboardBoard board;
        KeyboardKeySource source;

        void Awake()
        {
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
                cell.Active = !cell.Def.IsInert && pressed.Contains(cell.InputKey);
            MarkNeighbours();
        }

        void OnKeyPressed(Key key) => pressed.Add(key);

        void OnKeyReleased(Key key) => pressed.Remove(key);

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
