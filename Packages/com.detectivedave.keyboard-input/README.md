# Keyboard Input

`com.detectivedave.keyboard-input` turns the physical keyboard into a **one-way stream of key events** for a board game played on the keys.
It reads keyboard layouts (where every key is, in key units) and raises pressed / held / released events per physical key.

**One-way.** The package only produces events. It never queries the game, takes no callbacks that return values, and nothing a listener does
changes what it reports. The single thing that goes in is configuration, once: the layout to read (`KeyboardKeySource.Begin`).
Game state (what a press means, where the player is) lives in the game.

## Install

Embedded package: the folder `Packages/com.detectivedave.keyboard-input` is picked up by Unity automatically.
Requires Unity 6000.6 and `com.unity.inputsystem` 1.20.1. The runtime assembly `DetectiveDave.KeyboardInput` is auto-referenced.

## Use

```csharp
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(KeyboardKeySource))]
public class Player : MonoBehaviour
{
    [SerializeField] TextAsset layoutJson;
    IKeyEventSource keys;

    void Awake()
    {
        var source = GetComponent<KeyboardKeySource>();
        source.Begin(KeyboardLayout.Parse(layoutJson));   // once
        keys = source;
    }

    void OnEnable() => keys.KeyPressed += OnKeyPressed;
    void OnDisable() => keys.KeyPressed -= OnKeyPressed;

    void OnKeyPressed(Key key) { /* move to the key */ }
}
```

## API

| Type | What it is |
| --- | --- |
| `IKeyEventSource` | The contract: `KeyPressed`, `KeyHeld`, `KeyReleased` (`Action<Key>`), `RawKeyPressed` (`Action<KeyPress>`). Events only. |
| `KeyboardKeySource` | The `MonoBehaviour` implementing it. `Begin(KeyboardLayout)` once; reads `Keyboard.current` every frame at execution order -100. |
| `KeyPress` | One physical key going down: `Key`, `DisplayName` (what the OS layout types), `Def` (null when the layout lacks the key), `IsMapped`. |
| `KeyboardLayout`, `KeyDef`, `KeyRect` | Read-only layout data, parsed from JSON with `KeyboardLayout.Parse`. |
| `KeyGraph` | Which keys share a border (neighbours), built from the layout rectangles. |

Everything else is internal.

## Behaviour

- A key is an Input System `Key`: a **physical** position (US-layout naming), whatever the OS keyboard layout types. `KeyPress.DisplayName` is the OS character.
- A layout key is down while its own key or any of its `alsoCodes` keys is down. An `inert` key never goes down itself (it can still stand in for another key through that key's `alsoCodes`, which is how the Hungarian macOS `0` / `í` swap is handled).
- A tap shorter than a frame still counts: pressed and released in the same frame, no held.
- Caps Lock is reported by the OS as a switch, so it is latched: the first reading is not a press; every switch on or off is a press that lasts until another key is pressed.
- Order within a frame: per key pressed, held, released; keys in layout-file order, Caps Lock last; then `RawKeyPressed` for every physical key that went down.
- A listener that throws is logged; the other listeners and keys still get their events.
- No events before `Begin`. Disabling the component releases every key that is down; after re-enabling, a key still down is pressed again.
- Keys the layout does not have are logged once each (`Log Unmapped Keys`), with the OS display name, `Keyboard.keyboardLayout` and platform.

See `Documentation~/keyboard-input.md` for the layout JSON format and design notes.
