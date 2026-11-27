using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>
    /// The scene's <see cref="IKeyEventSource"/>: reads <see cref="Keyboard.current"/> once per frame and raises key events for the keys of a layout.
    /// <para>
    /// ONE-WAY. The only thing that goes into the source is its configuration, once: <see cref="Begin"/> with the layout to read.
    /// After that it only produces events. It never asks the game anything, holds no reference to game objects, and nothing a listener does
    /// changes what it reports (a frame's transitions are worked out before the first listener runs).
    /// </para>
    /// <para>
    /// Runs before default scripts (execution order <see cref="ExecutionOrder"/>), so every listener's own Update already sees this frame's events.
    /// No events are raised before <see cref="Begin"/>. A listener that throws is logged and does not stop the other listeners or the other keys.
    /// When the component is disabled (or destroyed while enabled) it raises <see cref="KeyReleased"/> for every key that is still down; once re-enabled,
    /// a key that is still down is pressed again.
    /// </para>
    /// </summary>
    [DefaultExecutionOrder(KeyboardKeySource.ExecutionOrder)]
    [DisallowMultipleComponent]
    [AddComponentMenu("Keyboard Input/Keyboard Key Source")]
    public sealed class KeyboardKeySource : MonoBehaviour, IKeyEventSource
    {
        /// <summary>The script execution order of the source: before default scripts.</summary>
        public const int ExecutionOrder = -100;

        [Tooltip("Log each physical key the layout does not have, once per key. That log is how unverified key codes get confirmed on hardware.")]
        [SerializeField] bool logUnmappedKeys = true;

        readonly List<KeyTransition> frame = new List<KeyTransition>();       // this frame's transitions, worked out before any listener runs
        readonly List<KeyTransition> releases = new List<KeyTransition>();    // the releases on disable
        readonly HashSet<Key> down = new HashSet<Key>();                      // keys whose press listeners have seen and whose release they have not
        readonly Dictionary<Key, KeyDef> mapped = new Dictionary<Key, KeyDef>();
        readonly HashSet<Key> reportedUnmapped = new HashSet<Key>();
        readonly DeviceReading reading = new DeviceReading();
        KeyboardLayout layout;
        KeyStateTracker tracker;
        int generation;                                                       // bumped by every release-all, see Publish

        readonly SafeEvent<Key> pressed = new SafeEvent<Key>();
        readonly SafeEvent<Key> held = new SafeEvent<Key>();
        readonly SafeEvent<Key> released = new SafeEvent<Key>();
        readonly SafeEvent<KeyPress> rawPressed = new SafeEvent<KeyPress>();

        /// <inheritdoc/>
        public event Action<Key> KeyPressed { add => pressed.Add(value); remove => pressed.Remove(value); }

        /// <inheritdoc/>
        public event Action<Key> KeyHeld { add => held.Add(value); remove => held.Remove(value); }

        /// <inheritdoc/>
        public event Action<Key> KeyReleased { add => released.Add(value); remove => released.Remove(value); }

        /// <inheritdoc/>
        public event Action<KeyPress> RawKeyPressed { add => rawPressed.Add(value); remove => rawPressed.Remove(value); }

        /// <summary>
        /// Starts reading the keys of <paramref name="keyboardLayout"/>. Call once, before or after the component is enabled; events start with the next frame's update.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="keyboardLayout"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The source has already begun. A source reads one layout for its whole life.</exception>
        public void Begin(KeyboardLayout keyboardLayout)
        {
            if (keyboardLayout == null) throw new ArgumentNullException(nameof(keyboardLayout));
            if (tracker != null) throw new InvalidOperationException($"{nameof(KeyboardKeySource)} already reads layout '{layout.Id}'.");
            layout = keyboardLayout;
            tracker = new KeyStateTracker(keyboardLayout);
            foreach (var def in keyboardLayout.UniqueKeys())
                mapped[def.InputKey.Value] = def;
        }

        void Update() => Tick();

        void OnDisable() => ReleaseHeld();

        // One frame of reading and publishing. Internal so EditMode tests can drive it (Update does not run in Edit Mode).
        internal void Tick()
        {
            if (tracker == null) return;
            var keyboard = Keyboard.current;
            reading.Keyboard = keyboard;
            frame.Clear();
            tracker.Step(reading, frame);
            int startedIn = generation;
            Publish(frame);
            if (keyboard != null && generation == startedIn) ReportRawPresses(keyboard, startedIn);
        }

        // Releases every key whose press was published. Internal so EditMode tests can drive it (OnDisable does not run in Edit Mode).
        internal void ReleaseHeld()
        {
            if (tracker == null) return;
            generation++;
            tracker.Reset();
            releases.Clear();
            foreach (var key in mapped.Keys)      // layout order
                if (down.Contains(key)) releases.Add(new KeyTransition(key, KeyTransitionKind.Released));
            Publish(releases);
        }

        // A listener may disable the source while a frame is being published: the releases of the keys published so far go out at once,
        // and the rest of that frame's transitions (now stale) are dropped.
        void Publish(List<KeyTransition> transitions)
        {
            int startedIn = generation;
            for (int index = 0; index < transitions.Count && generation == startedIn; index++)
            {
                var transition = transitions[index];
                if (transition.Kind == KeyTransitionKind.Pressed) down.Add(transition.Key);
                else if (transition.Kind == KeyTransitionKind.Released) down.Remove(transition.Key);
                switch (transition.Kind)
                {
                    case KeyTransitionKind.Pressed: pressed.Raise(transition.Key, this); break;
                    case KeyTransitionKind.Held: held.Raise(transition.Key, this); break;
                    case KeyTransitionKind.Released: released.Raise(transition.Key, this); break;
                }
            }
        }

        // Every physical key, mapped or not: unmapped ones are logged once each. Stops when a listener disables the source.
        void ReportRawPresses(Keyboard keyboard, int startedIn)
        {
            var keys = keyboard.allKeys;
            for (int index = 0; index < keys.Count; index++)
            {
                var control = keys[index];
                if (generation != startedIn) return;      // a listener disabled the source
                if (control == null || !control.wasPressedThisFrame) continue;
                mapped.TryGetValue(control.keyCode, out var def);
                if (def == null && logUnmappedKeys && reportedUnmapped.Add(control.keyCode))
                    Debug.Log($"[Keyboard] Not in layout '{layout.Id}': Key.{control.keyCode}, displayName '{control.displayName}', " +
                              $"keyboardLayout '{keyboard.keyboardLayout}', platform {Application.platform}", this);
                rawPressed.Raise(new KeyPress(control.keyCode, control.displayName, def), this);
            }
        }

        /// <summary>Reads the Input System keyboard; a missing keyboard reads as every key up.</summary>
        sealed class DeviceReading : IKeyReading
        {
            public Keyboard Keyboard;

            public void Read(Key key, out bool down, out bool pressedThisFrame)
            {
                if (Keyboard == null)
                {
                    down = false;
                    pressedThisFrame = false;
                    return;
                }
                var control = Keyboard[key];
                down = control.isPressed;
                pressedThisFrame = control.wasPressedThisFrame;
            }
        }
    }
}
