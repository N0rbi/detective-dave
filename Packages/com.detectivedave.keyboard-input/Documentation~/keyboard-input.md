# Keyboard Input

## Contract

The package is ONE-WAY. Data flows from the keyboard, through the package, to the game:

```
Input System (Keyboard.current) --> KeyboardKeySource --> IKeyEventSource events --> game listeners
                                          ^
                     KeyboardLayout, once (Begin)
```

- The only call into the package after set-up is subscribing and unsubscribing.
- There are no providers, callbacks with return values, or queries into game state. A listener cannot veto, consume or modify an event.
- Within a frame the source first works out every transition (internal `KeyStateTracker`), then publishes them, so listeners cannot affect each other's view of the frame.
- `Begin` takes one layout for the life of the component: a second call throws `InvalidOperationException`, `null` throws `ArgumentNullException`.
- The data types (`KeyboardLayout`, `KeyDef`, `KeyRect`, `KeyGraph`) are read-only. Asking them questions (neighbours, rectangles) reads package data; it gives the package nothing.

## Physical and logical keys

`UnityEngine.InputSystem.Key` names the physical position of a key, using US-layout names (`Key.Semicolon` is the key right of `L`, whatever it types).
Layout files are written in those names. What the key types under the user's OS layout is `KeyControl.displayName`, reported as `KeyPress.DisplayName`;
the package never uses it to decide anything. Some platforms report a key at a different physical position than the keycap suggests (Hungarian ISO macOS swaps the key
left of `1` with the ISO key beside left Shift); layout files fix that with `alsoCodes` and `inert`.

## Layout JSON

```json
{
  "id": "hu", "name": "Hungarian", "form": "iso", "digit": 4,
  "unitsWide": 15, "unitsHigh": 4,
  "keys": [
    { "code": "Digit1", "label": "1", "rects": [ { "x": 1, "y": 0, "w": 1, "h": 1 } ], "center": { "x": 1.5, "y": 0.5 } },
    { "code": "OEM1", "label": "í", "rects": [ ... ], "center": { ... }, "alsoCodes": [ "Backquote" ] },
    { "code": "Backquote", "label": "0", "rects": [ ... ], "center": { ... }, "inert": true }
  ]
}
```

| Field | Meaning |
| --- | --- |
| `id`, `name` | identifier and display name |
| `form` | `ansi`, `iso` or `jis` |
| `digit` | number-row key that picks this layout in a picker (1 = `Digit1`) |
| `unitsWide`, `unitsHigh` | board size in key units |
| `keys[].code` | `Key` enum name; entries with an unknown name are kept in `Keys` but skipped by `UniqueKeys()` and the source |
| `keys[].label` | the legend on the key |
| `keys[].rects` | rectangles in key units, origin top-left, y down; two for an L-shaped Enter |
| `keys[].center` | anchor point, key units |
| `keys[].alsoCodes` | other physical keys that count as this key |
| `keys[].inert` | the key never goes down itself |
| `keys[].unverified`, `keys[].note` | authoring metadata |

When a code appears twice, the first entry wins.

## Neighbours

Two keys are neighbours when their rectangles share a border of any length (within 1e-4 key units); touching at a corner does not count.
Inert keys are on the board and have neighbours. The relation is symmetric and never includes the key itself.

## Tests

EditMode tests live in `Tests/Editor` (assembly `DetectiveDave.KeyboardInput.Tests`, needs the package in the project manifest's `testables`).
The component tests use the Input System's `InputTestFixture` with a virtual keyboard.
