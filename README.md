# Input Handling

The game does not use regular input. Every key in the main block of the keyboard is a position on the game's x-y plane, and pressing a key moves the player there. This section covers how keyboards are described, how the player tells the game which one they have, and what is still unverified.

## Scaffolding

Code is centralized. Everything else is domain driven: art and data live next to the domain they belong to.

```
Assets/
  Code/                          all C# lives here, and nothing else
    Keyboard/
      KeyboardGlyph.shader       draws the glyph masks with palette colors
      KeyboardGlyphShader.cs     code mirror of that shader: one entry per shader property
      KeyboardCell.shader        flat unlit sprite for the grid cells
      Models/                    plain data classes, one per file (layout JSON, grid cell, selection choice)
      Utils/                     GridUtils (key units / local / screen conversions), SpriteFactory
      Runtime/                   the MonoBehaviours and the palette asset type
    Models/                      GameScene enum (shared across domains)
    AssetDependencies/           code that depends on specific assets: SceneLoader, and an editor-only build-list registrar
      Authoring~/                Python tools, ignored by Unity (validate.py, render_glyphs.py, out/)
  Game/
    Keyboard/                    the keyboard domain
      Data/                      one JSON file per layout (us, uk, de, hu, fr, jp) and KeyboardPalette.asset
      Art/                       generated glyph masks keyboard_<id>.png, KeyboardGlyph and KeyboardCell materials
```

A new domain follows the same pattern: `Assets/Game/<Domain>/{Art,Data,...}` for content, `Assets/Code/<Domain>/` for code.

## Layouts

Six layouts, selected on the number row. Only the physical form affects the engine; the language only changes the labels baked into the glyph.

| Digit | Layout | File | Form |
|---|---|---|---|
| 1 | US English | `us.json` | ANSI |
| 2 | UK English | `uk.json` | ISO |
| 3 | German | `de.json` | ISO |
| 4 | Hungarian | `hu.json` | ISO |
| 5 | French (AZERTY) | `fr.json` | ISO |
| 6 | Japanese | `jp.json` | JIS |

Each file is self-contained: `id`, `name`, `digit`, `form`, `unitsWide`, `unitsHigh` and a `keys` list. A key has:

- `code`: the Unity Input System `Key` name, for example `Digit1`, `Backslash`, `OEM1`. Keys are physical positions, so `Y` on a German board is the `Y` key's position, labelled `Z`.
- `label`: one legend, the unshifted character. This is what the glyph shows.
- `rects`: one or more `{x, y, w, h}` in key units, origin top-left of the main block. The ISO and JIS Enter keys have two rects (an L shape).
- `center`: where the player stands on the key. Always present.
- `alsoCodes` (optional): other key names that also light this key. `hu` uses it so both keys of the `0`/`í` pair light the `í` cell.
- `toggle` (optional): stateful key. Caps Lock only reports on and off, so its cell lights for a short pulse on every change instead of while held. All layouts set it on `CapsLock`.
- `inert` (optional): the cell never lights itself. `hu` uses it on the `0` key.
- `unverified` and `note`: only on keys whose Unity code has not been confirmed on hardware. A missing `unverified` means verified.

The files avoid nested arrays and optional booleans on purpose, so Unity's `JsonUtility` can read them.

To add a layout: copy the closest file, change `id`, `name`, `digit` and the labels, then run the two tools below. The tools pick up new files automatically.

## Tools

Needs Python 3 and Pillow (`pip install pillow`), plus a font covering Latin-1 and Hungarian and one covering Japanese. The script searches common Linux, macOS and Windows locations, or pass `--latin-font` and `--cjk-font`.

```
python3 Assets/Code/Keyboard/Authoring~/validate.py
python3 Assets/Code/Keyboard/Authoring~/render_glyphs.py
```

- `validate.py` checks that every key name is a real Unity `Key`, no keys overlap, every row tiles exactly to 15 units, both Shifts and the form's extra keys exist, digits are unique, and Digit1 to Digit9 sit in the same place in every layout. Unverified keys are listed but are not errors.
- `render_glyphs.py` writes the six mask PNGs to `Assets/Game/Keyboard/Art` and a preview contact sheet (default colors) to `Authoring~/out/contact_sheet.png`. 64 pixels per key unit.

## Recoloring the glyphs

The glyph PNGs are masks, not pictures. Each channel holds the coverage of one color role: R = edge (keycap outline), G = key (keycaps, modifiers included), B = text (labels and layout name), A = accent (number badge). The panel is the background, and the shader cuts its rounded corners. Opened in an image viewer they look like colored noise, which is expected.

To recolor, edit `Assets/Game/Keyboard/Data/KeyboardPalette.asset` (panel, key, edge, text, accent). Glyphs in the scene repaint immediately, in edit mode too. Make another palette with Assets > Create > Game > Keyboard > Palette.

Per glyph in the scene: a GameObject with a SpriteRenderer using `keyboard_<id>` as sprite and `Art/KeyboardGlyph.mat` as material, plus the `KeyboardGlyph` component with the palette assigned. Without a palette the shader's defaults (the original colors) are used.

The PNGs must stay uncompressed, linear and full-rect, otherwise the channels get mangled. The `.meta` files already have this: sRGB off, Alpha Is Transparency off, Mesh Type Full Rect, Compression None. Keep those settings if you re-import. Corner radius is 18 texture pixels in both `render_glyphs.py` (`PANEL_RADIUS`) and the material (`Corner Radius`).

## Test scenes

Both live in `Assets/Scenes`. They load each other through the `GameScene` enum and `SceneLoader`, never by a string. Entering Play Mode adds any `GameScene` that is missing to the build list (`BuildSceneRegistrar`, editor only). In a player build a missing scene stops the game at startup with an error. To add a scene: add its name to `GameScene` (new number, never renumber) and create `<Name>.unity`.

- **KeyboardSelect**: six glyphs, on three components: `KeyboardLayoutSelector` (logic), `LayoutChoiceView` (dims the glyphs) and `KeyboardSelectionOverlay` (the prompt). Press 1 to 6 to lock a layout; the chosen glyph stays lit, the rest dim, and KeyboardGrid loads after 0.8 s. If a layout is already locked the scene just moves on.
- **KeyboardGrid**: `KeyboardGridView` (draws), `KeyboardGridInput` (decides what is held), `KeyboardGridDebugOverlay` (labels and readout, delete it when done testing) and `KeyboardLayoutGate` (sends you to KeyboardSelect when nothing is locked) sit on one object. It builds the locked layout as cells inside a rectangle (the white outline in the Scene view, `Size` on the component). A cell lights up while its key is held, or while the left mouse button is held on it. Labels and the last pressed key are drawn with IMGUI, which is only meant for testing. Opening it with nothing locked sends you to KeyboardSelect, so press Play from either scene.

The lock lives in a static, so it is gone after leaving Play Mode, and there is no way to change it while the game runs.

## Implementation notes

- Code is in `Assets/Code/Keyboard`, namespace `DetectiveDave.KeyboardInput` (not `Keyboard`, which would hide the Input System's `Keyboard` class). `KeyboardLayout` (Models) reads the JSON, `KeyboardSession` holds the lock, `KeyboardLayoutSelector` and `KeyboardGridView` are the two scene behaviours, `KeyboardPalette`, `KeyboardGlyph` and `KeyboardGlyphShader` do the recoloring. Data classes live in `Models`, one per file, never nested in a behaviour. Behaviours are split by job: logic components raise events or write state (`KeyCell.Active`), view components only draw it, and IMGUI lives in its own overlay component.

- Input comes from the Input System `Key` enum, which names keys by physical position. The OS language never changes positions. `displayName` is not used, because labels are baked into the glyphs.
- Layout selection happens when the player enters the world: `Key.Digit1` to `Key.Digit6`, number row only (no numpad). The first press locks the layout immediately. The choice is not saved, so the screen shows on every launch, and the lock lasts until the game quits. There is no auto-detection.
- Keys are independent. There is no chord handling, and a key's cell is active while the key is held.
- A key that is not in the locked layout is ignored and logged once with its key name, display name, `Keyboard.keyboardLayout` and the platform. That log is how unverified codes get confirmed.
- Hungarian keyboards print `0` on the key left of `1` and `ö` on the key right of `9`. The selection screen therefore uses only Digit1 to Digit9.
- `KeyboardLayoutSelector` and `KeyboardGridView` read `Keyboard.current` directly for now. Splitting that into a key source can wait until the player movement needs it.

## Limitations

- **Nothing is verified on hardware yet.** The ISO extra key beside left Shift (`OEM1`) is only known to be `OEM1` on Windows, from a Unity staff reply and an old issue report. macOS and Linux are unknown.
- On ISO Macs the key left of `1` and the extra key beside left Shift may be swapped (seen in the earlier browser prototype, and confirmed on a Hungarian ISO MacBook: `0` and `í` are on each other's positions). In `hu.json` the `í` key lists `Backquote` in `alsoCodes`, so either physical key lights the `í` cell, and the `0` key is `inert`, so its cell never lights. `0` is never used in the game. ISO layouts flag `Backquote` for this.
- Linux: a Unity issue reports the key between left Shift and Z not registering with some international layouts. Its status is unknown.
- JIS yen and ro keys (`OEM2`, `OEM3`) are guesses. Unity may not expose them at all.
- Choosing the wrong form (ANSI on an ISO board) leaves the ISO extra key unmapped. The glyph shape is the only guard.
- An active IME on Japanese or Chinese systems may swallow digit presses on the selection screen. Not handled.
- Scope is the four main rows: number row to bottom row, with Tab, Caps Lock, Backspace, Enter and both Shifts. The space row, F-row, navigation keys and numpad are not mapped. Caps Lock is a stateful key: the OS reports it switching on and off, never held, so it can only pulse (see `toggle`). A player cannot "stand" on the Caps Lock cell by holding it.
- Glyphs show one legend per key, so second legends are missing. Modifier keys share the key color, so there is no separate color for them. Five color roles is the limit of one RGBA mask: a sixth role needs a second texture. Unity backends generally route all attached keyboards to a single device.

References: [Unity forum: detect ANSI or ISO](https://forum.unity.com/threads/is-there-a-way-to-detect-ansi-or-iso-keyboard-layout.1192684/), [issue: key named backslash](https://issuetracker.unity3d.com/issues/input-system-number-key-is-named-backslash-in-the-input-debug-for-keyboard), [issue: Linux international layouts](https://issuetracker.unity3d.com/issues/linux-input-system-does-not-register-some-keys-when-using-international-keyboard-layouts), [issue: yen key](https://issuetracker.unity3d.com/issues/the-y-equals-yen-key-is-not-recognized-when-japanese-keyboard-layout-is-used).
