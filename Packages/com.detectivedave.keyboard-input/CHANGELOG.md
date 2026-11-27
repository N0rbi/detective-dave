# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the package uses [Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-11-15

### Added
- First release, extracted from the game's `Assets/Code/Keyboard`.
- `IKeyEventSource`: one-way key event contract (`KeyPressed`, `KeyHeld`, `KeyReleased`, `RawKeyPressed`).
- `KeyboardKeySource`: reads `Keyboard.current` for the keys of a layout, configured once with `Begin(KeyboardLayout)`. Takes over the key logic of the game's `KeyboardBoard` (aliases, inert keys, sub-frame taps, latched Caps Lock, per-listener exception isolation, execution order -100) and the unmapped-key log of `KeyboardGridInput`.
- `KeyboardLayout`, `KeyDef`, `KeyRect`, `KeyGraph`, `KeyPress` moved from the game (script GUIDs kept).
- `KeyboardLayout.Parse(string)` next to `Parse(TextAsset)`, both validating their input.
- EditMode tests for layout parsing, neighbours, the key state machine and the component.

### Changed
- Layout data is read-only: public fields became private serialized fields (same JSON names) behind properties (`KeyRect.X/Y/Width/Height`, `KeyDef.Code/Label/Rects/...`, `KeyboardLayout.Id/Name/...`). Collections are exposed as `IReadOnlyList`.
- Events are named `KeyPressed` / `KeyHeld` / `KeyReleased` (were `OnKeyPressed` / `OnKeyHeld` / `OnKeyReleased` on the game's `IKeyboardBoard`).
- `KeyPress` carries the layout's `KeyDef` instead of the game's `KeyCell`; its constructor is internal.
- Raising an event no longer allocates (cached listener arrays); the per-frame LINQ over `Keyboard.allKeys` is gone.
- Key codes are parsed as names only: numeric strings and `None` no longer resolve to a `Key`.
- Disabling the source releases every key that is down (the game's `KeyboardBoard` left them held in listeners).

### Removed
- `KeyRect.Contains(Vector2)` (no callers).
- Board queries that are game or level concerns (`GetTile`, `GetPath`, `TryGetPosition`) are not part of the package.
