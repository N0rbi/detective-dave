using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>What happened to a key this frame.</summary>
    internal enum KeyTransitionKind
    {
        Pressed,
        Held,
        Released,
    }

    internal readonly struct KeyTransition
    {
        public readonly Key Key;
        public readonly KeyTransitionKind Kind;

        public KeyTransition(Key key, KeyTransitionKind kind)
        {
            Key = key;
            Kind = kind;
        }

        public override string ToString() => $"{Kind} {Key}";
    }

    /// <summary>One frame's view of the physical keys.</summary>
    internal interface IKeyReading
    {
        /// <summary>Is the physical key down now, and did it go down during this frame (true even when it is already up again)?</summary>
        void Read(Key key, out bool down, out bool pressedThisFrame);
    }

    /// <summary>
    /// The pure logic behind <see cref="KeyboardKeySource"/>: turns one reading per frame into pressed / held / released transitions
    /// for the keys of a layout. No Unity objects, no events: it only appends to the list it is given, so it is testable and
    /// nothing a listener does can reach back into it.
    /// <list type="bullet">
    /// <item>A layout key is down when its own key or any of its <c>alsoCodes</c> keys is down. An inert key never goes down.</item>
    /// <item>A tap shorter than a frame is already up when we look; <c>wasPressedThisFrame</c> still counts it as a press (pressed and released in one frame).</item>
    /// <item>The OS reports Caps Lock as a switch (on / off, never a key held down), so it is latched: the first reading is not a press,
    /// every change of the switch afterwards is a press, and the key stays down until another key is pressed.</item>
    /// </list>
    /// </summary>
    internal sealed class KeyStateTracker
    {
        // The keys the OS reports as a switch, not as a key held down.
        internal static readonly Key[] ToggleKeys = { Key.CapsLock };

        readonly Dictionary<Key, Key[]> sources = new Dictionary<Key, Key[]>();     // the physical keys that count as each layout key, in layout order
        readonly HashSet<Key> held = new HashSet<Key>();
        readonly Dictionary<Key, bool> switchState = new Dictionary<Key, bool>();   // switch keys: the OS state last frame
        readonly HashSet<Key> latched = new HashSet<Key>();                         // switch keys that count as down until another key is pressed

        public KeyStateTracker(KeyboardLayout layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            foreach (var def in layout.UniqueKeys())
                sources[def.InputKey.Value] = def.IsInert ? Array.Empty<Key>() : new[] { def.InputKey.Value }.Concat(def.AlsoKeys).ToArray();
        }

        /// <summary>Advances one frame. Appends this frame's transitions to <paramref name="output"/> in the order they happened.</summary>
        public void Step(IKeyReading reading, List<KeyTransition> output)
        {
            if (reading == null) throw new ArgumentNullException(nameof(reading));
            if (output == null) throw new ArgumentNullException(nameof(output));

            bool otherPressed = false;
            foreach (var entry in sources)
            {
                if (IsToggle(entry.Key)) continue;
                Read(reading, entry.Value, out bool down, out bool tapped);
                otherPressed |= tapped || (down && !held.Contains(entry.Key));
                Advance(entry.Key, down, tapped, output);
            }
            foreach (var key in ToggleKeys)
            {
                if (!sources.TryGetValue(key, out var keySources)) continue;
                Read(reading, keySources, out bool on, out _);
                Advance(key, Latch(key, on, otherPressed), false, output);
            }
        }

        /// <summary>
        /// Forgets all state, toggle switches included, so the next <see cref="Step"/> starts like the first one:
        /// a key still down then is a new press, and the Caps Lock switch is a first look again. Reports nothing; the caller knows what it published.
        /// </summary>
        public void Reset()
        {
            held.Clear();
            latched.Clear();
            switchState.Clear();
        }

        static bool IsToggle(Key key) => Array.IndexOf(ToggleKeys, key) >= 0;

        static void Read(IKeyReading reading, Key[] keySources, out bool down, out bool tapped)
        {
            down = false;
            tapped = false;
            foreach (var source in keySources)
            {
                reading.Read(source, out bool sourceDown, out bool sourceTapped);
                down |= sourceDown;
                tapped |= sourceTapped;
            }
        }

        // Every change of the OS switch (on or off) is a press, and the key stays down until another key is pressed.
        bool Latch(Key key, bool on, bool otherPressed)
        {
            if (!switchState.TryGetValue(key, out var before))
            {
                switchState[key] = on;    // first look: the switch may already be on, that is not a press
                return false;
            }
            if (on != before)
            {
                switchState[key] = on;
                latched.Add(key);
            }
            else if (otherPressed) latched.Remove(key);
            return latched.Contains(key);
        }

        // A tap shorter than a frame is already up by the time we look, so wasPressedThisFrame still counts as a press.
        void Advance(Key key, bool down, bool tapped, List<KeyTransition> output)
        {
            if (!held.Contains(key) && (down || tapped))
            {
                held.Add(key);
                output.Add(new KeyTransition(key, KeyTransitionKind.Pressed));
            }
            if (down) output.Add(new KeyTransition(key, KeyTransitionKind.Held));
            else if (held.Remove(key)) output.Add(new KeyTransition(key, KeyTransitionKind.Released));
        }
    }
}
