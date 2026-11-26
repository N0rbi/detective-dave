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
  Key Art/                       Default Key Art.config.asset (the style template every level falls back to) and one folder per kind (Walkable, Gap, Wall, WallGutter) with a 9-sliced Box sprite and an Enter sprite (per-size sprites stay as optional shape overrides).
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

Cases (the overworld's clients and stories) are `<Name>.case.asset` files made with Assets > Create > Detective Dave > Case. They carry the `Case` label: search `l:Case`. A case holds the client name, suspects (`{key}` in any text is replaced by the value, `{client}` by the client name), the description, the accept and decline messages, the findings, the response, the level to play and the cases that must be solved first. Before the level the left paper shows the description and the right paper the accept or decline letter. After the level the left paper shows the description with the findings typed in below it, and the right paper is the response the player types. The inspector previews the letters and both papers, and warns when the two messages do not diverge or when the findings or the response are empty. Solved cases are saved in `progress.json` in Unity's persistent data folder.

The desk (the overworld) is plain logic so far: `DeskMarker` (a case file's centre, size and rotation in world units), `DeskKeyMap` (a key belongs to the marker whose rectangle holds the key's centre; it also reports a marker over no key, a key under two markers and a marker over the start Shift), `DeskCases` (the state of the case under a key) and `DeskLevel` (every key is floor, the magnifier starts on Left Shift, there is no goal).

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
style's tint. Each kind needs two sprites in the key art: a 9-sliced **Box** for every key and an **Enter** for the L-shaped key; starter
canvases for the artist are in `ArtTemplates/KeyArt` (see its README). The old per-size sprites remain as optional **Shape Overrides**. Painted art dropped in `Assets/Staging/Tiles` is placed and imported by `Window > Detective Dave > Set Up Key Art`: Walkable gets a Box and an Enter (`keyboard_stealth_platform.png`, `keyboard_stealth_platform_enter.png`); Wall (`keyboard_stealth_brick.png`) and Wall Gutter (`keyboard_stealth_climb.png`) get a Box only, with tint white and their per-size sprites cleared, while their Enter sprites stay as they are. Gap needs no art of its own: the dog stands on the Walkable box, so `Set Up Key Art` gives the Gap style the Walkable box (tint white, no per-size sprites; running it again changes nothing). Because Gap keys then look like floor, a level open in Prefab Mode marks them with an orange DOG outline; the **Show Dog Markers** toggle in the Levels window (on by default, remembered per computer) turns it off. `keyboard_stealth_pipe.png` is assigned to nothing: it is already drawn into the climb art, whose top rim (172 px since the repainted climb art, pipe included) is the pipe strip. When a staged file already has its target (the art was repainted), `Set Up Key Art` copies the new pixels over the target, keeping its `.meta` and so every link to it, re-applies the border and moves the staged copy to `_to_delete` in the project folder; the dialog says `updated: wallgutter_box.png`. Running it again changes nothing. The 9-slice scale follows the thinnest border of a sprite, so a taller rim stays unstretched without changing the pixel size of the art.

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
  `LevelBackgroundView` draws the level's background behind everything. It stays in place in the world and covers everything the camera can show on the level: the normal view, the zoom out at the start and the zoom in on the player at the goal, wherever the player stands (`BackgroundCoverage` works that area out, plus an `Overscan Share`; it follows window resizes). Select the object to see the area as a dashed yellow outline while playing. `Max Upscale` logs a warning when the picture has to be drawn much larger than its imported size.
- The glue only reads the keyboard (layout, key centers, events) and writes only to the grid view; it never calls into the input package.

## Level lighting

Every level can set how the 2D lights of the Game scene look while it plays. Open the level prefab and find **Lighting** in the Level inspector:

- **Intensity** multiplies the brightness of every light in the scene (1 leaves them as set up in the scene, 0 turns them off). **Color Tint** multiplies their colours (white leaves them). **Final Look** shows the result.
- **Flicker** turns on a flicker. The lights wait a random time (**Wait At Least** to **Wait At Most**), play one flicker pattern and wait again.
- A **flicker pattern** is a short timeline: time runs left to right, brightness bottom (dark) to top (full). Click empty space to add a point, drag a point to move it, right-click a point (or select it and press Delete) to remove it, **Snap To Grid** keeps points on the grid. **Start From** fills a pattern with Blink, Stutter, Dim and recover, Short out or Buzz. **Weight** says how often a pattern is picked compared with the others; each flicker picks one pattern at random.
- **Preview** plays one pattern once; **Preview Like In Game** plays random waits and random patterns. Both drive the lamp in the inspector and the lights of the open scenes, and put them back when stopped, on Play Mode and before a scene is saved.
- The level checks list flicker without patterns, reversed waits, patterns without a duration or with every weight at 0, points after the end of a pattern, and lights at intensity 0.

`LevelLightingView` (on the object with `LevelLoader`, added by **Window > Detective Dave > Set Up Lighting**, also run by Set Up Player and Play This Level) reads the lighting of the current level, restarts the flicker whenever a level loads and puts the lights back when it is disabled. By default it lights every `Light2D` of the scene; turn **Use All Scene Lights** off and fill **Target Lights** to light only some. It uses scaled game time.

The keys react to the 2D lights through the material `Assets/Game/Keyboard/Art/KeyboardCellLit.mat` (shader Universal Render Pipeline/2D/Sprite-Lit-Default), set on the keyboard grid's **Cell Material** by Set Up Lighting. Lit now: the keys and the keyboard backdrop of Game.unity. Still unlit: the level background, the player, guards, decorations, the evidence picture, the dash gust and the spiral wipe; the Overworld is unchanged.

## Reactive music
A jazz loop plays all the time, and a piano note plays on every move. The loop is `Assets/Audio/Music/JazzLoop.wav`, the notes are the files in `Assets/Audio/Music/Piano`, and both are referenced by `Assets/Audio/Music/Music Settings.config.asset`. Run **Window > Detective Dave > Set Up Music** (Set Up Player, Set Up Overworld and Play This Level run it too): it fills the empty references, makes the loop import as uncompressed streaming audio when it still has Unity's default import, and adds a `MusicLink` to the object that holds the `PlayerController` in Game.unity and the `OverworldController` in Overworld.unity.

`MusicLink` creates one `MusicDirector` that survives scene changes, so the loop never restarts when a level restarts or the overworld loads the game, and two scenes never start two loops. It listens to the `MoveAttempted` event of the controller (`IMoveAttemptSource`), raised by `PlayerGame` for every key the player presses while able to move: a step, gutter, gap fall or goal is `Moved`; a wall, the key the player stands on, a neighbour wall or the key the player stands on is `Blocked`; a key that is not next to the player (the key that also stuns the player, `MoveOutcome.Stun`, also for the magnifying glass and its start hint) is `WrongKey`. Presses before the start, while moving, dying, in the spiral and in the briefing raise nothing.

The reaction (`IMusicReaction`, today `PianoReaction`) picks the note by `Piano Order` on the settings asset. Every mode chooses only among the clips whose notes all lie in the scale of the chord that sounds at the moment of the key press (see Chord chart below): `Chord Aware` (default) picks among the fitting clips at random, never the same clip twice in a row, and makes a clip made of the chord's own notes `Chord Tone Weight` times as likely on strong beats (beats 1 and 3 of a bar) and at the first beat of a chord, halfway on the other beats; `In Order` sorts the clips by name once (`NaturalNameOrder`, so `Piano 2` comes before `Piano 10`, whatever order they are listed in) and each move takes the next clip that fits, going back to the first after the last, and a move that does not move the player also takes the next clip; `Random` picks a random fitting clip, never the same twice in a row. The place in the order is kept across levels and scenes. The note starts in the same frame as the key press: nothing is delayed or placed on the beat of the music. A move always plays a clip: when no clip fits the chord, `When No Clip Fits` decides (`Play Closest To Chord`, the default, plays the clip with the most notes inside the chord's scale, never the one played just before, the lowest name winning a tie; `Play Next Clip` plays the next clip in name order), and the same happens when the loop position is outside every chord. Without a Chord Chart (or a chart without chords) the Piano Order is applied to all clips and one warning is logged. Only with no piano clips at all is a move silent. The note plays at `Piano Volume` with a random change of up to `Piano Volume Variance`; a blocked move plays at `Blocked Move Volume Share` (0.5, half) of that. To try another way of reacting, write another `IMusicReaction` and return it from `MusicDirector`.

### Chord chart
**Wrong key: the most dissonant clip.** When the attempt is `WrongKey` and `Dissonant Misclick` is on (default), the piano plays the clip that clashes most with the chord at that moment, among all chord clips and all single notes (`MisclickPicker`, `DissonanceScorer`). A note scores 0 if it is a chord tone, `Scale Non Chord Tone` (0.1) if it is in the chord's scale only, otherwise a clash weight by its interval to the nearest chord tone: a semitone above `Minor Second` 1.0, a semitone below `Major Seventh` 0.95, six semitones `Tritone` 0.8, two above `Major Second` 0.5, two below `Minor Seventh` 0.45, anything else `Other` 0.3 (all on `Dissonance Weights` on Music Settings). A clip scores the sum of its notes (`Normalize By Note Count` averages instead). The highest score wins; tied clips take turns in order (chord clips by name, then singles by pitch), so the same clip never plays twice in a row while another is tied. It plays at `Misclick Volume Share` (0.5) of the piano volume with the usual variance. The debounce does not apply to it: a misclick neither starts nor restarts the debounce wait and does not move the In Order sequence. Without a Chord Chart, outside every chord, or with the setting off, a wrong key plays a normal note (one warning is logged in the first two cases). Music Settings shows the winner per chord and its score. Current chart: Am7, Dm7 and Bm7b5 rotate over Piano 5, 6, 11, 12 (Bm7b5 also the single Gs4); E7 rotates over Piano 1 and 7.

The 12 piano clips are the chord hits of the comping, so the game must know which chord every clip belongs to and which chord the loop plays at every moment. That is the `Assets/Audio/Music/Chord Chart.config.asset` (`ChordChart`), referenced from Music Settings (`Chord Chart`; without it the chords are ignored, a warning is logged and the notes follow Piano Order over all clips). It holds the tempo (75 BPM), beats per bar (4), loop length in bars (8, so 25.6 s, the length of `JazzLoop.wav`), `Chart Beat At Loop Start` (the beat of the chart that sounds when the clip starts; 0 when the clip starts on the first chord, change it when the clip starts on another bar), the chord segments (name, start beat, length, chord tones, scale) and, per piano clip, the notes that sound in it. Notes are numbers 0 to 11 (C=0, C#=1, D=2, D#=3, E=4, F=5, F#=6, G=7, G#=8, A=9, A#=10, B=11), the octave does not matter (`PitchClassSet`).

Run **Window > Detective Dave > Set Up Chord Chart** (Set Up Music runs it too): it creates the asset, fills it from `Assets/Audio/Music/chord_chart.json` when it is empty (it asks before replacing a filled one), links it on Music Settings and, the first time, sets Piano Order to Chord Aware. The file holds the chords read from the MIDI the jazz kit was built from (Am7, Dm7, Bm7b5, E7, twice, one chord per bar) and the notes found in each of the 12 samples by analysing the audio: clips 1 and 7 are C E G, 2, 3, 8 and 9 are F A C, 4 and 10 are D F A, 5, 6, 11 and 12 are D G# B. The scales are suggestions to tweak by ear: Am7 A B C D E G, Dm7 D dorian, Bm7b5 B locrian, E7 E phrygian dominant (E F G# A B C D, the fifth mode of A harmonic minor). Select the chart to see the chords on a timeline, a warning for every problem (gaps or overlaps between chords, a chord without a fitting clip, a clip without notes, a loop length that does not match the clip) and a list in words of which chords every clip plays over. The same problems are logged when the game starts.

When a move happens, `MusicDirector` reads the playing position of the loop (`AudioSource.timeSamples`) and `ChordClock` turns it into the chord, beat and bar of that moment; the picker uses it only to choose a clip that fits that chord, and the note then plays at once. The chart's tempo, bars and `Chart Beat At Loop Start` are needed for that lookup (the chord at a position of the loop), not for any timing of the notes. A chord with no fitting clip logs one warning naming it and `When No Clip Fits` plays a clip.

### Debounce and single notes

A run of moves would be a run of chords, which is heavy. The piano therefore has two kinds of notes: the chord clips (the 12 comping hits in `Piano Clips`) and the single note clips (one piano note each, in `Single Note Clips` on Music Settings). The first note after a pause is always a chord clip. Every move within `Chord Debounce Seconds` (default 0.6, slider 0 to 3 s) of the previous piano note, a blocked move included, plays a single note instead, and the wait starts again at every note, so a continuous run of moves stays single notes until the player pauses for that long. With `Singles Extend Debounce` off, the wait is measured from the last chord only, so a chord returns that many seconds after the last chord even while the player keeps moving. 0 turns the debounce off: every note is a chord. A move that the click gate (`Min Seconds Between Piano Notes`) skips, or that plays nothing, does not restart the wait. The time is the unscaled game time, like the rest of the music code (`DebounceGate` takes it from the clock of `PianoReaction`).

A single note is chosen like a chord clip: only a clip whose note lies in the scale of the chord that sounds at the key press, by `Piano Order` (`Chord Aware` favours chord tones on strong beats, `In Order` goes up through the clips sorted by pitch and back to the lowest, `Random` never plays the same clip twice in a row) and `When No Clip Fits` when none fits. The note of a single clip is its MIDI pitch on the entry (60 is C4, 69 is A4); only its pitch class counts, so the octave does not matter for the chord check. The blocked move volume applies to both kinds. While `Single Note Clips` is empty every note is a chord clip and one warning is logged: a move is never silent while a clip exists.

Export the single notes from GarageBand as `Piano Single - <Note><Octave>.wav`, sharps written with a lowercase s and no flats: `Piano Single - A3.wav`, `Piano Single - Gs3.wav` (G#3). Middle C is C4 (MIDI 60), the octave goes from -1 to 9 (`SinglePitch`). Drop them into `Assets/Staging` and run **Window > Detective Dave > Set Up Music** (Set Up Chord Chart does the same): it moves every `Piano Single - *` file with its `.meta` to `Assets/Audio/Music/Piano/Single`, adds each new clip to `Single Note Clips` with the pitch from its file name (entries you edited are kept, and running it again changes nothing), and the dialog says how many clips were found, which files it moved and which names it could not read. Music Settings shows the debounce line (`First note after a pause plays a chord; moves within 0.6 s play single notes.`), the list of notes (`Gs3 -> G#3`) and a warning while the list is empty. The pitch of a single clip is kept on its Music Settings entry, not on the Chord Chart, so the pitch has one home.

## Cheat codes

Type a code word on the keyboard, in the game or in the overworld, and nothing else is needed: no terminal, no menu.

| Code | What it does |
|---|---|
| `resetplayer` | Forgets every solved case (the same as `Window > Detective Dave > Reset Overworld Progress`), drops the case in progress, shows "Progress reset" on screen and loads the overworld fresh. |

- Run `Window > Detective Dave > Set Up Cheats` once (Set Up Player, Set Up Overworld and Play This Level run it too). It creates `Assets/Cheats/Cheat Settings.config.asset` and adds a `CheatLink` next to the Keyboard Key Source in Game.unity and Overworld.unity.
- The letters are read from the keys the OS says they type, so the code works on any keyboard layout. Case does not matter. Backspace removes the last letter. A pause longer than `Timeout Seconds` (3) forgets the letters typed so far. Other keys are ignored.
- The code is a normal word on normal keys: while typing it the player also moves, or types into an open paper. The cheat fires anyway. Progress is deleted without a confirmation.
- Cheat Settings: `Cheats Enabled` (turn off for a release build), `Timeout Seconds`, `Entries` (code word and action, so the word can be changed or more entries added), `Reset Player Message`, `Message Seconds`, `Message Font` (filled with the Notice Font), `Message Size Share`, `Message Color`, `Message Background Color`.
- Code: `CheatCodeMatcher` (letters to a matched code), `CheatDispatcher` (code to `IGameCheat`), `ResetPlayerCheat` behind `ICheatEnvironment`, `CheatDirector` (one persistent object, draws the message), `CheatLink` (listens to the key source's raw key events without changing them). A new cheat is a new `CheatAction`, an `IGameCheat` and a line in `CheatDirector.CheatFor`.

## Player

The player is shown on the level's start Shift key and the game does not start until that Shift is pressed; then press neighbouring keys to move. The other Shift is the End. All code is in `Assets/Code/Player` (namespace `DetectiveDave.PlayerPlay`, assembly `DetectiveDave.Game`).

- `PlayerController`: thin glue on the Keyboard Grid object; wires board, key events, level, view and animations to the game.
- `PlayerGame`: the rules (start on the start Shift, move, bump walls, stun on a wrong key, respawn after a catch or a dog, lock on the end Shift). No Unity types beyond plain data; unit tested.
- `PlayerMoveRule`, `PlayerStateMachine`, `PlayerFacing`, `WalkCycle`: small pure helpers (what a step does, state, flip, yoyo frame order).
- `LevelCellClassifier`: maps only Gap and WallGutter tiles; the two Shifts (`ShiftEnds` helper) always count as Floor.
- `PlayerView`: the sprite on screen. `PlayerAnimations`: step, stun wobble in the death pose, fall flat when caught, drop in from above, wall bump (lean, squash, return), idle breathing (DOTween).
- `PlayerMovementSettings` (asset): all timings and sizes, in `Assets/Player`.
- A level may have no goal key (the overworld): the player then faces right and never reaches a goal. With `forgivingMoves` on, a key that is not a neighbour does nothing instead of stunning. `flipsWithFacing` off and a single walk frame give a player with no left or right, like the magnifying glass.
- `EndDecorationsDirector` (rules), `EndDecorationsView` (sprites, spinning star), `DecorationScale`: the drain grate on the start key, the folder icon on the goal key and the star that spins behind the player at the goal. Pictures in `Assets/Decorations`, sizes and speed in the player settings.
- `DashGustView` (behind `IDashGust`), `GustGeometry`, `GustLine`: every move (a step, a wall bump, a step onto a dog) draws wind lines that follow the player: each line's head stays at the player and its tail trails behind along the distance travelled so far, so a bump gives short lines and the lines fade only after the player arrives; `PlayerGame` calls `IDashGust.Show(from, to, climbing)` at the start of every leg (a wall bump has two, the lean and the way back). The gust sits on a band of the player's body: `Gust Band Center In Heights` (default 0.55) lifts it above the feet so its top is at the shoulders and its bottom at the upper legs; while the climb pose is drawn on a gutter the band drops by `Climb Drop In Heights` with the player (`GustGeometry.Lift`). Each line has its own offset from the middle of that band, thickness and length (in player heights and a share of the distance travelled), plus colour and fade time, in the "Dash gust" section of the player settings. The default is 3 lines.
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
- **KeyboardGrid**: `KeyboardGridView` (draws), `KeyboardKeySource` (the key events), `KeyboardBoard` (the board API), `KeyboardGridInput` (lights cells from the source's events, and tints the neighbours of lit keys, `Show Neighbours`), `KeyboardGridDebugOverlay` (labels and readout: each press shows the neighbour count the board reports; delete it when done testing) and `KeyboardLayoutGate` (sends you to KeyboardSelect when nothing is locked) sit on one object. It builds the locked layout as cells inside a rectangle (the white outline in the Scene view, `Size` on the component). A cell lights up from the source's pressed event until its released event, except Caps Lock: the OS keeps it pressed while it is switched on, so it is drawn lit (with its neighbours) for at most `Caps Lock Max Lit Seconds` (default 0.6, 0 = only the press frame) after each press (`CapsLockLitTimer`, unit tested), and a new toggle restarts the time. This is drawing only: the source still raises the same events for Caps Lock, so gameplay is unchanged. Labels and the last pressed key are drawn with IMGUI, which is only meant for testing. Opening it with nothing locked sends you to KeyboardSelect, so press Play from either scene.

- **Fit to the screen**: add `KeyboardGridScreenFit` to the grid object and the keyboard (with its backdrop) spans the screen width, edge to edge, at any window size, keeping its proportions (it fits the height instead when the stretched keyboard would be taller than the screen). It scales the whole object, so tilemaps that are children of it stay lined up with the keys; `Side Margin` leaves a share of the width empty. Without it the grid keeps the fixed `Size`.
- **Game**: plays levels. The Keyboard Grid object of KeyboardGrid plus `LevelLoader` (plays the first level of `Assets/Levels/Level Sequence.config.asset`, or the level Play This Level hands it), `KeyTerrainView` (draws each key in its kind's Key Art), `LevelBackgroundView`, `LevelMessageView` (a red banner when there is no level or its shape is empty), and, disabled, `KeyboardGridDebugOverlay` and `KeyboardBoardSampler` (test benches: enable them to log tiles and spawn path-following markers). KeyboardSelect leads here after the layout is picked. Designers never open it: use Window > Detective Dave > Levels and Play This Level (F5). If no layout is locked it sends you to KeyboardSelect and back (`KeyboardSession.ReturnTo`).

Making a level (Levels window, Key brush, Play This Level, live reload) is described in `Packages/com.detectivedave.levels/README.md` and checked by hand with `Assets/Tests/Manual Checklist.md`. Game code is in the `DetectiveDave.Game` assembly (`Assets/Code`), its editor tools in `DetectiveDave.Game.Editor` (`Assets/Code/Editor`); tests in `Assets/Tests`.

Guards: open a level, press **Add Guard** in the Levels window (or **Edit Patrol** on a Guard in the Inspector). The Scene view turns orange: click a key for START, then keys for END. Right-click, Esc or Exit leaves; picking another guard in the Hierarchy edits that one. Warnings show in the Scene view and Levels window (every keyboard layout is checked).

A guard occupies exactly one key: the key it is leaving, until it reaches the middle of the next key. It looks for the player every frame from that key only: standing next to the occupied key starts the chase at once (it finishes its current step at chase speed, then follows the shortest key path), standing next to the key it is walking towards changes nothing until it arrives, and only the player on the occupied key is caught. Walking towards the player never catches; the catch happens on arrival.

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

## Dogs, skyline and the overworld board

- **Dogs step with the upright bass.** Dogs never share a frame: each dog starts on its own random frame and may skip a step (`Skip Chance`). `Dog Step Mode` is `Bass Notes` (default) or `Timer`. On `Bass Notes` every dog advances one step of its 1-2-3-2-1 loop when the upright bass starts a note in the loop; an angry dog steps on every note, a calm dog on every `Calm Every Notes`-th. Without a Bass Chart the dogs use the timer and one warning is logged. The notes come from an audio export of the bass track: drop the `.aif` or `.wav` (it must start at the very start of the loop, bar 1) into `Assets/Staging`, move it to `Assets/Audio/Music/Bass`, run `python3 tools/bass_onsets.py "Assets/Audio/Music/Bass/<file>" Assets/Audio/Music/Bass/bass_onsets.json` (python 3 and numpy; it reads AIFF and WAV itself; `python3 tools/test_bass_onsets.py` is its self-test), then `Window > Detective Dave > Set Up Bass Chart` (`Set Up Music` runs it too when the json exists). It rounds note starts to `Quantize Beats` (0.25) and wraps them into the loop; the chart tempo and the file length are compared: within one beat the loop's own tempo is used, otherwise a dialog offers the audio tempo. `Bass Start Offset Beats` on `Bass Chart.config.asset` corrects a bass file that does not start with the loop. At run time `BassPulseClock` turns the loop position (`MusicDirector.BassPulse`) into the number of notes since the last frame. Current bass file: 19 notes in 26.02 s, loop 25.6 s at 75 BPM.
- **Case music.** Every case can have its own music over the jazz loop (`Music` group on the case: `Music Clip`, `Fade In Seconds` 1.5, `Fade Out Seconds` 2, `Volume` 0.8, `Loop Volume While Playing` 0.3, `Sync To Loop` on). It fades in when the briefing of an open case opens (a solved case's review never starts it), plays through the letter, the level and the scene changes, and fades out when the request is declined, the case has no level, the findings start typing, or the overworld starts with nothing to report. The jazz loop is turned down to `Loop Volume While Playing` meanwhile. With `Sync To Loop` the clip starts at the loop's position and is pulled back when it drifts more than 40 ms, so a clip longer than the loop is cut at every loop end (the case inspector warns when the clip is more than a second longer). The `resetplayer` cheat stops it at once. The code is `CaseMusicCues` (called by `OverworldFlow`), `MusicDirector` (the player) and `LoopFollower`. To try it: set `Music Clip` to `Upright Bass` on the Kidnapped case, the last case of the chain.
- **Death hook (muted trumpet stem).** `Assets/Audio/Music/Stingers/Muted trumpet hook.aif` (26.0 s, stereo, 44.1 kHz, 24 bit) is a stem that runs the whole time, looping, muted (volume 0) and in lock-step with the jazz loop, like a layer in a DAW. Nothing starts on a death: a death (a guard catches the player, a dog bites them) only fades the stem's volume up and the jazz loop, the case music and the piano down; when the player can move again after the respawn the stem fades back to 0 and the music up. The wrong-key stumble and reaching the goal never touch it. The stem starts together with the loop at the loop's position plus `Hook Sync Offset Seconds`, wraps at the loop length (25.6 s, so the 0.4 s tail of the file never plays), is pulled back by the pure `LoopFollower` whenever it drifts more than 40 ms, and keeps running across level restarts and scene loads (`MusicDirector` is persistent). The hook file must start at bar 1 of the loop. Settings are the `Death hook` group on `Music Settings`: `Hook Clip`, `Hook Volume` 0.9, `Fade In Seconds` 0.25, `Fade Out Seconds` 1, `Loop Volume While Hook Plays` 0, `Case Music Volume While Hook Plays` 0, `Piano While Hook Plays` off, `Start Delay Seconds` 0, `Hook Sync Offset Seconds` 0 (shift the stem against the loop for export delay). Fades are linear and use unscaled time. The loop's volume is its own volume times the case music's turn-down times the death share (a product, so all can turn it down). A second death while the stem fades out fades it up again from where it is; the `resetplayer` cheat and a scene change silence it at once and restore all volumes, and the stem keeps running muted. Without a Hook Clip no stem is created. The state machine is `DeathHookController` (Idle, FadingIn, Playing, FadingOut), which only produces volumes through `IDeathHookOutput`; `PlayerGame` raises `Died` and `Respawned`, `IPlayerLifeSource` forwards them to `MusicLink`, and `MusicDirector` owns the stem's audio source. `Set Up Music` moves a file with `trumpet` in its name from `Assets/Staging` to `Assets/Audio/Music/Stingers` and assigns it when `Hook Clip` is empty; Music Settings warns while the clip is empty.
- **Background and the goal zoom.** The goal zoom moves the main camera and shrinks its size to `Camera Zoom Factor` towards the end key; it never scales a parent. The background lies in world space on its own root and is fitted to the camera's home view, the board enlarged by the zoomed half view, and the live camera view (so it can only grow). `LevelBackgroundView > Trace Background` writes `Temp/background-trace.log` (semicolon separated: time, camera, orthographic size, aspect, view, planned coverage, picture rectangle, keyboard panel shown) for every frame the camera is away from its home view, and ends a line with `OUTSIDE` when the view shows beyond the picture.
- **Dogs replace gaps.** A Gap tile is a dog now (`DogController` on the object with the `PlayerController`, set up by `Window > Detective Dave > Set Up Dogs`; Set Up Player runs it). The dog does not move. It is angry while the player stands on a key next to it and calm otherwise; its frames play 1-2-3-2-1 and loop (`DogFrames`). The player who steps onto the dog's key walks there and dies like a caught player (death sprite, fall flat, respawn); the dog is calm while the player lies on it. The player never falls into a hole any more: `FadeIntoGap` and `Gap Fade Seconds` are gone. Art: `Assets/Dogs/Art/stealth_dog_angry_cycle_1..3.png` and `stealth_dog_calm_cycle_1..3.png`. `Dog Settings.config.asset` (Assets/Dogs): `Height In Keys`, `Offset Y In Keys`, the two frame lists, `Frames Per Second`, `Hide Key Art` (hides the Gap key art under the dog).
- **Skyline background.** `Assets/Key Art/Background/new_york_skyline_moody.png` is the default background of every Key Art that has none (`Window > Detective Dave > Set Up Background`; Set Up Player runs it). A level's own Background sprite still wins (the level checks warn when it is a `placeholder_*` sprite that hides the Key Art's background). The dark backdrop that the keyboard draws behind the keys is hidden while a level shows a background (`Hide Backdrop Over Background` on `KeyTerrainView`, on by default), so the picture is seen; with no background the backdrop stays. Each level has a **Background Placement** on its Level component: Visible, Offset (0 centred; 1 is one screen width or height, never further than the picture allows), Zoom (1 just covers everything the camera can show, also while it zooms on the player), Flip X, Flip Y. `BackgroundCoverage` and `BackgroundFit` hold the maths.
- **Overworld board.** `keyboard_stealth_board.png` (Assets/Overworld/Art) replaces `placeholder_desk_top.png` as `Desk Top Sprite` and covers the screen at its own proportions. The board is fitted by `BoardFit` (pure) and drawn from the `Board (desk top picture)` group on `Overworld Settings.config.asset` (Assets/Overworld): `Board Fit Mode` (`Fit Inside`, default, shows the whole picture inside the camera view with its aspect kept, at any window shape; `Cover` fills the view and may cut the edges), `Board Margin Share` (0.03, free space at each edge as a share of the view), `Board Scale Multiplier` (1; below 1 smaller, above 1 bigger) and `Board Offset Share` (0 centred; share of the view size, x right, y up). Papers and cases are not affected. `Set Up Overworld` replaces the placeholder only; a sprite you chose yourself stays. `Window > Detective Dave > Set Up Overworld Board` does only this step (it also imports the picture as a single sprite), so a problem elsewhere in `Set Up Overworld` cannot block it; running either twice changes nothing the second time. The board picture must be in `Assets/Overworld/Art`, not `Assets/Staging`. If the overworld still shows the placeholder, run `Set Up Overworld Board`, then `Check Overworld Sprites`.
- **Smoking detective.** `SmokingDetective` (one component, Assets/Code/Overworld) sits on the `Smoking Detective` object of Overworld.unity, with the body sprite on it and the arm sprite under an `Arm Pivot` child. Every `Seconds Between Puffs` the pivot moves by `Raised Offset` and turns by `Raised Angle`, stays `Smoke Seconds` with a tilt of `Puff Tilt Angle`, then returns (`Raise Seconds`, `Lower Seconds`). Move the pivot object to change where the arm turns. `Window > Detective Dave > Set Up Smoking Detective` creates or repairs only this object (`Set Up Overworld` runs it too); its dialog lists what it did. It creates it once in the place of the old `stealth_detective_walk_cycle_2` object (same position, scale and sorting), fills only empty sprites, and never moves anything you placed.
- **Overworld transitions.** Group `Overworld transitions` on `Overworld Settings.config.asset` (Assets/Overworld). The Sprite Light 2D of Overworld.unity (already in the scene; the light is found by its type) is set to 0 when the overworld starts and fades up to `Light Full Intensity` (1) in `Light Fade In Seconds` (0.8). When a case is accepted the light fades down to 0 in `Light Fade Out Seconds` (0.6) together with the spiral wipe, and Game.unity loads only when both have finished (`CompletionLatch`). When a message opens (briefing letter, report or solved review) `SmokingDetective` (the whole detective, arm included) drops `Detective Drop Distance` (3) world units with `Detective Drop Ease` (`OutBounce`; `OutBack` also works) in `Detective Drop Seconds` (0.9), and stays down for as long as the message view is open (letter, typing, report, review; a hand-over from one view to the next never lets him up). When the view closes to the board (accept, decline, case finished, refused request) he comes back up with `Detective Return Ease` in `Detective Return Seconds` (0.9). If it closes while he is still going down, the way down is stopped and he comes back up from where he is. A new message while he is moving drops to the same low height again, never lower, because the height is always the rest height minus the distance (`DetectiveDrop`, pure and unit tested). Leaving the scene or disabling the detective puts him back at his rest height. The Console logs an error when the scene has no Sprite Light 2D or no Smoking Detective. Leaving by the `resetplayer` cheat does not fade the light.
- **Papers inside the camera.** `PaperFit` (pure) shrinks and moves each briefing paper so its rotated rectangle lies inside the camera's view, keeping `Paper Screen Margin Share` (Overworld Settings, default 0.03) free at every edge. It runs each time a briefing opens, so a new window size applies from the next briefing. Each letter is also capped to its row height (`GlyphCell.CharacterSize`) so rows cannot overlap. `Window > Detective Dave > Check Overworld Sprites` lists empty or `placeholder_` sprites on Overworld Settings.

## Overworld (desk with case files)

How to make a desk of cases and play it:

1. Make a case: `Assets > Create > Detective Dave > Case`, and save it in `Assets/Cases`. Find every case with the search `l:Case`.
2. Fill the case: client name, suspects, description, accept and decline messages, findings and response. Use the `Pick a level` dropdown to choose the level it plays, and `Requires` for the cases that must be solved first. The inspector warns about a missing level, a duplicate id and texts that do not fit. The id is filled once and never follows a rename: do not change it once players have progress.
3. Run `Window > Detective Dave > Set Up Overworld` once. It makes the scene, a sample case and a sample desk, and fills the `Desk` field of `Overworld Settings.config.asset` (in `Assets/Overworld`) only when it is empty. That desk is the one the game starts on.
4. Make your own desk with `Window > Detective Dave > Create Overworld Desk`, then open it in Prefab Mode (double-click it). Point the `Desk` field of `Overworld Settings.config.asset` at it when it should be the starting desk.
5. In Prefab Mode put a case on the desk in one of three ways: press `Add to desk` next to it in the desk inspector's `Cases` list (it lands on the first free keys), drag the case asset from the Project window onto the Scene view, or use `GameObject > Detective Dave > Case Marker` and drag your case onto its `Case File` field.
6. Move, rotate and scale the marker with the normal tools. The keys under it belong to its case. Keep the desk root at position 0 and scale 1: the inspector warns and has a `Reset root transform` button.
7. Read the Scene view: every key of the preview keyboard is drawn; free keys are light grey (a case file can go there) and a legend in the corner names the colours. Each case has its own colour on the keys it covers and its client name. Red keys are shared by two cases. A case that covers no key, has no case or has no level says so. The blue START is Left Shift and must stay free. The desk inspector also lists problems with the chain of required cases.
8. Press `F5` with the desk open or selected to play it; `Shift+F5` with nothing selected plays the desk in Overworld Settings. `Window > Detective Dave > Reset Overworld Progress` forgets the solved cases.

The `Preview as` dropdown on the desk inspector shows the case files as Open, Locked, Solved or by the progress file, in the Scene view only; the desk asset is not changed.

Playing it:

1. Move the magnifier (Left Shift starts) onto a case file and wait 2 seconds (`dwellSeconds`), then type the accept or decline answer. Spaces and line breaks type themselves: nobody has to press Space. Until the player chooses, the accept answer waits in green and the decline answer in red (`Accept Pending Text Color`, `Decline Pending Text Color`); once chosen, the other answer fades out in place (`Unchosen Fade Seconds`) and the chosen one stays grey and black. If one answer is the start of the other (for example `...` and `..............`), type on for the longer one or press Enter or Space after the short one to choose it; the case inspector shows a warning about that overlap.
2. The description types at a constant speed (`descriptionSecondsPerCharacter`); any key speeds the rest up by `descriptionSkipSpeedMultiplier`. The letter paper appears only when the description is done, and typing is ignored until then.
3. After the goal the left paper shows the description with the findings typing in below it (same speed, sound and key skip as the description). The right paper appears only when the findings are done; the player then types the response on it (`responseUnit`: Character, Word or Sentence). Typed letters are black (a space after a finished word or sentence counts as typed, so the next word is already shown), only the current unit is shown in grey and the following text stays invisible, so nothing is spoiled. Only when the response is finished is the case saved as solved; its folder then shows the solved picture. Walking onto a solved case shows the description with the findings on the left and the response on the right, read-only: no typing, and any key slides the papers away.
3b. Paper text shrinks to fit: the description plus findings (left paper) and the letter or response (right paper) start at `Text Height Share` and shrink by `Text Fit Step Share` down to `Min Text Height Share`; if still too long a warning is logged. `Text Line Height Ratio` tunes the row height.
4. The magnifier always starts on Left Shift when the overworld opens. Its size is `heightInKeys` in `Magnifier Movement Settings.config.asset`. The dash gust is the `Dash gust enabled` flag on a movement settings asset: on for the detective, off for the magnifier.

Desk look and hints (sections `Keyboard look`, `Floating case files` and `Route hint` on `Overworld Settings.config.asset`):

1. The two Shift keys are drawn as keycaps (`Shift Keycap Sprite`, `Shift Keycap Color`, `Shift Keycap Size Share`), so the desk reads as a keyboard and the magnifier visibly starts on Left Shift. The placeholder is `placeholder_keycap.png`; run `Set Up Overworld` once to fill the field.
2. Case files drift gently. Every file rolls its own timing, distance and tilt between the Min and Max values (`Case Float ...`), so they never move together. The keys a case covers and the dwell use the resting position, so the drift never changes which case you are on. `Case Float Enabled` turns it off.
3. A dashed line follows the keys from the magnifier to the nearest open case, using the same pathfinding as the guards. `Route Hint Visibility`: `UntilCaseReached` keeps the line, shortening it from where the player stands, until an open case is reached; `UntilFirstMove` hides it at the first move. It shows again each time the desk starts, and is hidden while a briefing is open. Dash length, gap, width, flow speed and colour are on the same settings asset; `Route Hint Enabled` turns it off.

Fonts (section `Fonts` on `Overworld Settings.config.asset`, files in `Assets/Overworld/Fonts`):

1. `Request Font` (`jmh_typewriter.ttf`) is used for the request: the client's message on the left paper, on every screen. A case can set `Description Font Override` (for example `1942_report.ttf` on the Kidnapped case) to draw its own description in another font; the findings are never affected.
2. `Placeholder Font` (`jmh_typewriter.ttf`) is used for the grey text that waits to be typed.
3. `Typed Font` (`moms_typewriter.ttf`) is used for the black text the player has typed, so a typed letter replaces its grey placeholder in a different font.
4. `Findings Font` (`jmh_typewriter.ttf`) is used for the investigation findings on the left paper, after the level and in the solved case review.
5. `Notice Font` (`1942_report.ttf`) is used for the notice shown on the paper when a case has no level yet.

Text colours (section `Look` on a case, global defaults on `Overworld Settings.config.asset`): every case can set its own colour for the description, the findings, the typed letters, the grey placeholder and the green and red pending answers. Each line has a `Use` box: unticked, the case takes the colour from Overworld Settings (`Description Text Color`, `Findings Text Color`, `Typed Text Color`, `Pending Text Color`, `Accept Pending Text Color`, `Decline Pending Text Color`); ticked, the colour next to it is used on every screen of that case (letter, report, solved review). Under the fields, `Look preview` shows a sample line per colour in the real font on `Paper Reference Color`, and warns when a colour is below `Minimum Text Contrast` on that paper colour. The reference colour only drives the preview and the warning; it does not change the paper picture.

Text fit: the left paper is fitted as a whole (description plus findings) before anything is typed, so the size never changes while the findings type in. The findings are drawn at `Findings Size Share` (default 0.85) of the description size. Text shrinks down to `Hard Min Text Height Share`; a word longer than a line also forces a smaller size. If it still does not fit at that size, one warning naming the case is logged. `Set Up Overworld` fills the empty font fields; the old `Client And Placeholder Font` value moves to `Placeholder Font` by itself. `Set Up Overworld` only fills empty fields: a `Request Font` or `Findings Font` set earlier to another font must be reassigned to `jmh_typewriter.ttf`.

Papers are drawn as a character grid: every letter sits in its own cell, as wide as `Text Advance Ratio` times the text size, and every row is `Text Line Height Ratio` high. Each glyph is centred in its cell and squeezed sideways if it is wider than the cell, whichever font draws it, so the placeholder and the typed letter that replaces it occupy exactly the same cell and nothing runs past the right edge of the paper. If one font sits higher on the row than the other, use `Typed Font Baseline Offset Share` and `Placeholder Font Baseline Offset Share` on `Overworld Settings.config.asset`. Monospaced fonts still look best. Licences: JMH Typewriter is free for personal use only (buy a licence before selling the game), Mom's Typewriter and 1942 report need their readme or text file kept with them. The unused weights stay in `Assets/Staging`.

All art and audio are references on settings assets; placeholders are files named `placeholder_*` in `Assets/Overworld/Art`, `Assets/Overworld/Audio` and `Assets/Overworld/Fonts`. Replace a placeholder by pointing the reference on `Overworld Settings.config.asset` at your own file. Nothing is generated at runtime; `Set Up Overworld` only fills references that are empty and shows what it changed.

Typewriter sound plays on every valid letter key, on Backspace, and softly while the description types in. Each strike picks a random clip (never the same twice in a row), pitch and volume within the ranges on `Overworld Settings.config.asset` (Sound section); description clicks are never closer than `Min Seconds Between Clicks`. If the Console reports an empty click list, a missing Audio Listener or a zero volume, that is why it is silent.

### Wrong key stun and death pose

A key that is not a neighbour of the player stuns the detective: the player stays on its key, shows the death pose (`Death Sprite`, `stealth_detective_death.png` in `Assets/Player/Art`) with a short wobble for `Wrong Key Stun Seconds` (default 0.8, 0 turns the stun off), ignores keys meanwhile, then stands up again. A guard can still catch a stunned player. Being caught shows the same pose, popped by `Death Pop Scale` and settling, before the spiral restart. The magnifying glass (`Forgiving Moves`) never stuns and needs no death sprite. Run `Window > Detective Dave > Set Up Player` to import the sprite and assign it. The old settings `Slip Seconds`, `Slip Distance In Keys`, `Slip Tilt Degrees` and `Fall Forward Degrees` are gone.

### Overworld start hint

Before the magnifying glass has moved for the first time, pressing a key that is not the start Shift and not next to it punches the start Shift keycap (`ShiftKeycapsView.Punch`) and lights the pressed key with a red, fading copy of the keycap picture (`WrongKeyGhostView`), so the player learns where to go without a tutorial. `StartHintGate` decides what counts as wrong (armed when the overworld starts, disarmed by the first move or a restart; unit tested). The "Start hint" section of Overworld Settings holds the on/off switch, punch size and time, the colour and the fade time. Existing settings assets get the defaults automatically.
