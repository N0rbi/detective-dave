# Input Handling

The game does not use regular input. Every key in the main block of the keyboard is a position on the game's x-y plane, and pressing a key moves the player there. This section covers how keyboards are described, how the player tells the game which one they have, and what is still unverified.

## Scaffolding

Code is centralized. Everything else is domain driven: art and data live next to the domain they belong to. Reusable code lives in local packages under `Packages/`; game code depends on them, never the other way round.

```
Packages/
  com.detectivedave.keyboard-input/   the keyboard as a one-way stream of key events (see its README): layout data, KeyGraph, KeyboardKeySource
  com.detectivedave.levels/           levels as data (see its README and Documentation~): TileType, TerrainTile, TileMapGroup, LevelDefinition, baked LevelTerrain, editor tools
Assets/
  Code/                          all C# lives here, and nothing else
    Keyboard/                    shaders, Models/ (plain data), Utils/ (GridUtils, SpriteFactory), Runtime/ (MonoBehaviours, IKeyboardBoard, KeyboardBoard), Authoring~/ (Python tools, ignored by Unity)
    Level/Runtime/               glue between the levels package and the keyboard grid (namespace DetectiveDave.LevelPlay): TileMapKeyboardMapper, KeyTerrainView, LevelBackgroundView, LevelLoader, LevelMessageView, KeyboardBoardSampler (test bench)
    Player/                      the player (namespace DetectiveDave.PlayerPlay), see Player below
    Editor/                      editor tools (assembly DetectiveDave.Game.Editor): Levels window, key brush, Play This Level
    Models/                      GameScene enum (shared across domains)
    AssetDependencies/           code that depends on specific assets: SceneLoader, and an editor-only build-list registrar
  Game/Keyboard/                 Data/ (one JSON per layout, KeyboardPalette.config.asset) and Art/ (glyph masks keyboard_<id>.png, glyph and cell materials)
  Key Art/                       Default Key Art.config.asset (the style template every level falls back to) and one folder per kind (Walkable, Gap, Wall, WallGutter) with a sprite for every key shape.
  Tiles/                         the Terrain Tile assets you paint with: Walkable, Gap, Wall, WallGutter (each carries its Tile Type), Level Tiles.prefab (the palette), Sprites/ (16 x 16 tile sprites, 64 pixels per unit: one tile is a quarter of a key)
  Levels/                        one prefab per level (the Level Definition), Level Sequence.config.asset, Templates/
  Player/                        Player.prefab, Player Movement Settings.config.asset, the walk and climb cycle PNGs
  Decorations/                   pictures drawn on top of keys: the start grate, the goal folder (stealth_goal_both, plus its folder and glass parts) and the goal star
  Staging/                       temporary drop folder for new art: the asset step moves everything out to its proper folder, so it is normally empty
  Plugins/Demigiant/             DOTween (third party)
  Resources/                     DOTweenSettings.asset
  Scenes/                        KeyboardSelect, KeyboardGrid, Game
  Tests/                         Editor and PlayMode tests, and Manual Checklist.md
```

Config assets (tuning values a designer edits) are named `<Name>.config.asset` and carry the `Config` label: search `l:Config` in the Project window to list them all. New ones get the label automatically.

A new domain follows the same pattern: content in its own folder under `Assets/`, code in `Assets/Code/<Domain>/`.

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

To recolor, edit `Assets/Game/Keyboard/Data/KeyboardPalette.config.asset` (panel, key, edge, text, accent). Glyphs in the scene repaint immediately, in edit mode too. Make another palette with Assets > Create > Game > Keyboard > Palette.

Per glyph in the scene: a GameObject with a SpriteRenderer using `keyboard_<id>` as sprite and `Art/KeyboardGlyph.mat` as material, plus the `KeyboardGlyph` component with the palette assigned. Without a palette the shader's defaults (the original colors) are used.

The PNGs must stay uncompressed, linear and full-rect, otherwise the channels get mangled. The `.meta` files already have this: sRGB off, Alpha Is Transparency off, Mesh Type Full Rect, Compression None. Keep those settings if you re-import. Corner radius is 18 texture pixels in both `render_glyphs.py` (`PANEL_RADIUS`) and the material (`Corner Radius`).

## Board API

Two things on the Keyboard Grid object (next to `KeyboardGridView`) are what game code talks to. A key is the Input System `Key`, its physical position, and for the player a key is a tile.

- `IKeyEventSource`, implemented by `KeyboardKeySource` from the keyboard-input package: the key events. It is one-way: it only produces events and takes nothing back from the game, so state (where the player is, what a press means) belongs in your own domain code. `KeyboardBoard` starts it with the locked layout.
- `IKeyboardBoard`, implemented by `KeyboardBoard` (game code): geometry questions about the locked layout.

```csharp
// IKeyEventSource (KeyboardKeySource)
event Action<Key> KeyPressed;        // went down (also for a tap shorter than a frame)
event Action<Key> KeyReleased;       // came up
event Action<Key> KeyHeld;           // every frame it is down, including the frame it went down
event Action<KeyPress> RawKeyPressed; // diagnostics: every physical key going down, in the layout or not, with the OS display name

// IKeyboardBoard (KeyboardBoard)
bool IsNeighbour(Key key, Key other);
IReadOnlyCollection<Key> GetNeighbours(Key key);
bool TryGetPosition(Key key, out Vector2 world);          // where the player stands on that key (a hair above-right of its center)
float KeyWorldSize { get; }                               // world units per key unit (grid scale x transform scale)
```

- **Events** come from the locked layout only, once per key and frame, in `Update` (execution order -100, so before your scripts). Disabling the source releases every key that is down. Keys are independent, there are no chords. A key the layout does not have fires nothing. The Hungarian `0`/`í` swap is already folded in: either physical key fires `í`, and `0` fires nothing. Caps Lock is reported by the OS as a switch, so every switch (on or off) fires a press that stays held until another key is pressed.
- **Neighbours.** Two keys are neighbours when their rectangles share a border of any length, so the quarter-offset keys of the QWERTY and home rows count. Directions and the key you stand on do not matter, and edge keys just have fewer neighbours. At most 6, exactly 6 for the inside keys of the letter rows (checked on all six layouts), and the relation is symmetric. It is computed from the layout rectangles (`KeyGraph`), there is no matrix to maintain.
- **Tiles and paths** are not part of the board: they are level data. Ask `TileMapKeyboardMapper` (see Levels below).
- Get the board with `GetComponent<IKeyboardBoard>()` on its object or `FindFirstObjectByType<KeyboardBoard>()`, and the events with `GetComponent<KeyboardKeySource>()` on the same object (subscribe in `OnEnable`, unsubscribe in `OnDisable`).

## Levels

Levels are data, made with the `com.detectivedave.levels` package: a **Level Definition** asset per level holds its name, its
background, the key sprites per tile type and key shape, and its shape, painted in tilemaps and baked into the asset. The
step-by-step guide for designers and artists is `Packages/com.detectivedave.levels/Documentation~/index.md`; the package README
has the API. In short:

1. A **Tile Map Group** (GameObject > Detective Dave > Level > Tile Map Group, as a child of the Keyboard Grid object at 0, 0, 0,
   cell size 0.25 so a key is 4 tiles wide and 8 tall: keys are drawn `Key Height Stretch` (default 2) times taller than wide, so the climbing player fits inside a gutter key) holds the tilemaps of one level. Its **Layers** list is the priority order: the first
   layer with a typed tile in a cell decides the cell. A layer can give all its tiles one **Type**, or leave it empty and let each
   `TerrainTile` carry its own. Any number of groups can exist; each bakes into its own level.
2. Paint with the tiles in `Assets/Tiles` (Window > 2D > Tile Palette). Paint the middle of each key: that is what the
   key becomes. Keys with nothing painted are hidden in the game.
3. **Bake** on the group writes the shape into its level. Entering Play and saving the scene bake too. The runtime only reads the
   bake, never the tilemaps, so tilemap renderers can stay off in a build.
4. On the Keyboard Grid object, `TileMapKeyboardMapper > Level` is the level that is played. That is the only wiring.

Tile types are assets (each Terrain Tile in `Assets/Tiles` carries one), so a new kind of key needs no code: create a Tile Type, a Terrain Tile (or a
layer) for it, and give it sprites in the key art. The key art lives in `Assets/Key Art/Default Key Art.config.asset`; a level
points at it as its **Style Template** and overrides only what it wants to change. Sprites are grayscale and multiplied by each
style's tint; see the designer guide for the naming convention and the **Fill From Folder…** button.

Game code asks the mapper, which sits next to `KeyboardGridView`:

```csharp
LevelDefinition Level { get; }  event Action LevelChanged;  void Load(LevelDefinition level);
MapTile GetTile(Key key);                                 // the tile under the key's center (nudged by 0.01 off the tile border)
MapTile GetTile(Vector2 world);
IReadOnlyList<Vector2> GetPath(Vector2 start, Vector2 goal, Func<MapTile, bool> passable = null);
```

- `MapTile` carries the `Type` (a `TileType` asset, null where nothing is painted), `IsWalkable` and the tilemap cell. Compare types
  by asset: keep a `[SerializeField] TileType gutter` and test `tile.Type == gutter`.
- **Paths** are A* over the baked cells: 8 directions, no cutting corners past blocked cells, walkable tiles only unless you pass
  your own `passable`. The result is cell centers in world space, start and goal included, empty when there is no way. Points are
  0.25 units apart, so enemies follow them as waypoints. Enemies that can climb gutters would pass
  `tile => tile.IsWalkable || tile.Type == gutter`.
- `KeyTerrainView` draws the level on the keys: each key takes its type's tint (`KeyCell.Background`) and the sprite for its key
  shape (`SetPartSprite`, or `SetKeySprite` for a whole-key Enter sprite). Missing sprites log one warning and stay flat squares.
  `LevelBackgroundView` draws the level's background behind everything, covering the camera.
- The glue only reads the keyboard (layout, key centers, events) and writes only to the grid view; it never calls into the input package.

## Player

The player is shown on the level's start Shift key and the game does not start until that Shift is pressed; then press neighbouring keys to move. The other Shift is the End. All code is in `Assets/Code/Player` (namespace `DetectiveDave.PlayerPlay`, assembly `DetectiveDave.Game`).

- `PlayerController`: thin glue on the Keyboard Grid object; wires board, key events, level, view and animations to the game.
- `PlayerGame`: the rules (start on the start Shift, move, bump walls, respawn after slips and gaps, lock on the end Shift). No Unity types beyond plain data; unit tested.
- `PlayerMoveRule`, `PlayerStateMachine`, `PlayerFacing`, `WalkCycle`: small pure helpers (what a step does, state, flip, yoyo frame order).
- `LevelCellClassifier`: maps only Gap and WallGutter tiles; the two Shifts (`ShiftEnds` helper) always count as Floor.
- `PlayerView`: the sprite on screen. `PlayerAnimations`: step, slip backwards then drop in from above, wall bump (lean, squash, return), idle breathing, gap fade (DOTween).
- `PlayerMovementSettings` (asset): all timings and sizes, in `Assets/Player`.
- `EndDecorationsDirector` (rules), `EndDecorationsView` (sprites, spinning star), `DecorationScale`: the drain grate on the start key, the folder icon on the goal key and the star that spins behind the player at the goal. Pictures in `Assets/Decorations`, sizes and speed in the player settings.
- `GoalSequence`, `IntroSequence` (rules), `EvidenceView`, `CameraZoomView` (behind `ICameraZoom`), `SpiralScreenWipe` (behind `IScreenWipe`), `SpiralGeometry`: at the goal the evidence pops up above the player's head, the camera zooms onto the player holding it (the grid's `KeyboardGridScreenFit` is paused while the camera is zoomed and resumes when it is back), a snail-house spiral of large strips winds from the outer screen edge inwards to the player and covers the screen, `LevelLoader.Advance` loads the next level (wrapping to the first; the same level after Play This Level). Every level start (the first load, F5, after a goal) runs the same `IntroSequence`: the screen starts covered and zoomed on the player at the start key, then the spiral unwinds outwards while the camera zooms back out; keys are ignored until it is done. Zoom factor, zoom time, zoom aim, timings, spiral segments, turns (at least 2) and colour are in the "Goal sequence" section of the player settings.

F5 (Play This Level) is the single entry point; the player is wired automatically.

**DOTween** (`Assets/Plugins/Demigiant`, `Assets/Resources/DOTweenSettings.asset`) drives the animations. `DetectiveDave.Game.asmdef` sets Override References and lists `DOTween.dll`, because DOTween is a precompiled DLL that an asmdef assembly does not see otherwise (it is not auto-referenced).

Designer workflow:

1. Open `Window > Detective Dave > Levels` and select or create a level.
2. Pick the level's Start key (Left or Right Shift); the other Shift is the End. Paint the keys in between.
3. Paint Gap and WallGutter keys, plus walkable and Wall keys around them.
4. Press F5 (Play This Level). Press the Start Shift first: nothing else responds until then.
5. Tune `Assets/Player/Player Movement Settings.config.asset` and replay.

## Test scenes

Both live in `Assets/Scenes`. They load each other through the `GameScene` enum and `SceneLoader`, never by a string. Entering Play Mode adds any `GameScene` that is missing to the build list (`BuildSceneRegistrar`, editor only). In a player build a missing scene stops the game at startup with an error. To add a scene: add its name to `GameScene` (new number, never renumber) and create `<Name>.unity`.

- **KeyboardSelect**: six glyphs, on three components: `KeyboardLayoutSelector` (logic), `LayoutChoiceView` (dims the glyphs) and `KeyboardSelectionOverlay` (the prompt). Press 1 to 6 to lock a layout; the chosen glyph stays lit, the rest dim, and Game (the first level) loads after 0.8 s. If a layout is already locked the scene just moves on.
- **KeyboardGrid**: `KeyboardGridView` (draws), `KeyboardKeySource` (the key events), `KeyboardBoard` (the board API), `KeyboardGridInput` (lights cells from the source's events, and tints the neighbours of lit keys, `Show Neighbours`), `KeyboardGridDebugOverlay` (labels and readout: each press shows the neighbour count the board reports; delete it when done testing) and `KeyboardLayoutGate` (sends you to KeyboardSelect when nothing is locked) sit on one object. It builds the locked layout as cells inside a rectangle (the white outline in the Scene view, `Size` on the component). A cell lights up from the source's pressed event until its released event. Labels and the last pressed key are drawn with IMGUI, which is only meant for testing. Opening it with nothing locked sends you to KeyboardSelect, so press Play from either scene.

- **Fit to the screen**: add `KeyboardGridScreenFit` to the grid object and the keyboard (with its backdrop) spans the screen width, edge to edge, at any window size, keeping its proportions (it fits the height instead when the stretched keyboard would be taller than the screen). It scales the whole object, so tilemaps that are children of it stay lined up with the keys; `Side Margin` leaves a share of the width empty. Without it the grid keeps the fixed `Size`.
- **Game**: plays levels. The Keyboard Grid object of KeyboardGrid plus `LevelLoader` (plays the first level of `Assets/Levels/Level Sequence.config.asset`, or the level Play This Level hands it), `KeyTerrainView` (draws each key in its kind's Key Art), `LevelBackgroundView`, `LevelMessageView` (a red banner when there is no level or its shape is empty), and, disabled, `KeyboardGridDebugOverlay` and `KeyboardBoardSampler` (test benches: enable them to log tiles and spawn path-following markers). KeyboardSelect leads here after the layout is picked. Designers never open it: use Window > Detective Dave > Levels and Play This Level (F5). If no layout is locked it sends you to KeyboardSelect and back (`KeyboardSession.ReturnTo`).

Making a level (Levels window, Key brush, Play This Level, live reload) is described in `Packages/com.detectivedave.levels/README.md` and checked by hand with `Assets/Tests/Manual Checklist.md`. Game code is in the `DetectiveDave.Game` assembly (`Assets/Code`), its editor tools in `DetectiveDave.Game.Editor` (`Assets/Code/Editor`); tests in `Assets/Tests`.

The lock lives in a static, so it is gone after leaving Play Mode, and there is no way to change it while the game runs.

## Implementation notes

- Reading the keyboard is the `com.detectivedave.keyboard-input` package (assembly and namespace `DetectiveDave.KeyboardInput`): `KeyboardLayout`, `KeyDef`, `KeyRect` read the JSON, `KeyGraph` works out neighbours, `KeyboardKeySource` raises the events. The game side is in `Assets/Code/Keyboard`, namespace `DetectiveDave.KeyboardGrid` (not `Keyboard`, which would hide the Input System's `Keyboard` class): `KeyboardSession` holds the lock, `KeyboardLayoutSelector` and `KeyboardGridView` are the two scene behaviours, `KeyboardPalette`, `KeyboardGlyph` and `KeyboardGlyphShader` do the recoloring. Data classes live in `Models`, one per file, never nested in a behaviour. Behaviours are split by job: logic components raise events or write state (`KeyCell.Active`), view components only draw it, and IMGUI lives in its own overlay component.

- Input comes from the Input System `Key` enum, which names keys by physical position. The OS language never changes positions. `displayName` is not used, because labels are baked into the glyphs.
- Layout selection happens when the player enters the world: `Key.Digit1` to `Key.Digit6`, number row only (no numpad). The first press locks the layout immediately. The choice is not saved, so the screen shows on every launch, and the lock lasts until the game quits. There is no auto-detection.
- Keys are independent. There is no chord handling, and a key's cell is active while the key is held.
- A key that is not in the locked layout is ignored and logged once (by `KeyboardKeySource`, `Log Unmapped Keys`) with its key name, display name, `Keyboard.keyboardLayout` and the platform. That log is how unverified codes get confirmed.
- Hungarian keyboards print `0` on the key left of `1` and `ö` on the key right of `9`. The selection screen therefore uses only Digit1 to Digit9.
- `KeyboardKeySource` is the only reader of `Keyboard.current` besides `KeyboardLayoutSelector` (and the debug overlay's trace): the selector runs before any layout exists and must only count a press during this frame, so it reads `wasPressedThisFrame` on the digit keys directly.

## Limitations

- **Nothing is verified on hardware yet.** The ISO extra key beside left Shift (`OEM1`) is only known to be `OEM1` on Windows, from a Unity staff reply and an old issue report. macOS and Linux are unknown.
- On ISO Macs the key left of `1` and the extra key beside left Shift may be swapped (seen in the earlier browser prototype, and confirmed on a Hungarian ISO MacBook: `0` and `í` are on each other's positions). In `hu.json` the `í` key lists `Backquote` in `alsoCodes`, so either physical key lights the `í` cell, and the `0` key is `inert`, so its cell never lights. `0` is never used in the game. ISO layouts flag `Backquote` for this.
- Linux: a Unity issue reports the key between left Shift and Z not registering with some international layouts. Its status is unknown.
- JIS yen and ro keys (`OEM2`, `OEM3`) are guesses. Unity may not expose them at all.
- Choosing the wrong form (ANSI on an ISO board) leaves the ISO extra key unmapped. The glyph shape is the only guard.
- An active IME on Japanese or Chinese systems may swallow digit presses on the selection screen. Not handled.
- Scope is the four main rows: number row to bottom row, with Tab, Caps Lock, Backspace, Enter and both Shifts. The space row, F-row, navigation keys and numpad are not mapped.
- Glyphs show one legend per key, so second legends are missing. Modifier keys share the key color, so there is no separate color for them. Five color roles is the limit of one RGBA mask: a sixth role needs a second texture. Unity backends generally route all attached keyboards to a single device.

References: [Unity forum: detect ANSI or ISO](https://forum.unity.com/threads/is-there-a-way-to-detect-ansi-or-iso-keyboard-layout.1192684/), [issue: key named backslash](https://issuetracker.unity3d.com/issues/input-system-number-key-is-named-backslash-in-the-input-debug-for-keyboard), [issue: Linux international layouts](https://issuetracker.unity3d.com/issues/linux-input-system-does-not-register-some-keys-when-using-international-keyboard-layouts), [issue: yen key](https://issuetracker.unity3d.com/issues/the-y-equals-yen-key-is-not-recognized-when-japanese-keyboard-layout-is-used).
