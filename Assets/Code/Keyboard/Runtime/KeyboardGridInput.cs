using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// Decides which cells are held: keys on the keyboard, and the left mouse button on a cell.
    /// Writes the result to <see cref="KeyCell.Active"/> and reports presses. Draws nothing.
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView))]
    public class KeyboardGridInput : MonoBehaviour
    {
        [SerializeField, Min(0f)] float togglePulse = 0.25f;     // how long a toggle key (Caps Lock) stays lit after a state change

        readonly HashSet<Key> reportedUnmapped = new HashSet<Key>();
        KeyboardGridView view;

        /// <summary>Raised for every key going down, including keys the layout does not have.</summary>
        public event Action<KeyPress> KeyPressed;

        /// <summary>Raised when the mouse button goes down on a cell.</summary>
        public event Action<KeyCell> CellClicked;

        void Awake() => view = GetComponent<KeyboardGridView>();

        void Update()
        {
            if (view.Layout == null) return;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            var mouseCell = mouse != null && mouse.leftButton.isPressed ? view.CellAt(mouse.position.ReadValue()) : null;
            if (mouseCell != null && mouse.leftButton.wasPressedThisFrame) CellClicked?.Invoke(mouseCell);
            if (keyboard != null) ReportPresses(keyboard);

            foreach (var cell in view.Cells.Values)
                cell.Active = !cell.Def.inert && (IsHeld(keyboard, cell) || cell == mouseCell);
        }

        bool IsHeld(Keyboard keyboard, KeyCell cell)
        {
            if (keyboard == null) return false;
            if (cell.Def.toggle) return IsPulsing(keyboard[cell.InputKey], cell);
            return keyboard[cell.InputKey].isPressed || cell.Def.AlsoKeys.Any(other => keyboard[other].isPressed);
        }

        // Caps Lock only reports its on/off state, never a real hold, so it pulses on each change.
        bool IsPulsing(KeyControl control, KeyCell cell)
        {
            if (control.wasPressedThisFrame || control.wasReleasedThisFrame)
                cell.PulseUntil = Time.unscaledTime + togglePulse;
            return Time.unscaledTime < cell.PulseUntil;
        }

        // Keys the layout does not have are logged once each: that log is how unverified key codes get confirmed.
        void ReportPresses(Keyboard keyboard)
        {
            foreach (var control in keyboard.allKeys.Where(candidate => candidate.wasPressedThisFrame))
            {
                view.Cells.TryGetValue(control.keyCode, out var cell);
                if (cell == null && reportedUnmapped.Add(control.keyCode))
                    Debug.Log($"[Keyboard] Not in layout '{view.Layout.id}': Key.{control.keyCode}, displayName '{control.displayName}', " +
                              $"keyboardLayout '{keyboard.keyboardLayout}', platform {Application.platform}");
                KeyPressed?.Invoke(new KeyPress(control.keyCode, control.displayName, cell));
            }
        }
    }
}
