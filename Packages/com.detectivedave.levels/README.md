# Detective Dave Levels (`com.detectivedave.levels`)

Levels as data. A level's **shape** (which keys are floor, gap, wall, start, goal, ...) is painted in tilemaps and baked
into a **Level Definition** asset, which also holds the level's **background** and the **key sprites** for every tile type
and key shape. Tile types are assets too, so a new kind of terrain needs no code.

The package knows nothing about keyboards or input. It answers questions in *board space* (see below); game code maps keys
to board positions and draws the result.

Designer guide (no code): [Documentation~/index.md](Documentation~/index.md).

## Concepts

| Thing | What it is | Create with |
|---|---|---|
| `TileType` | One kind of terrain: editor colour, walkable flag, notes. | Assets > Create > Detective Dave > Level > Tile Type, or **Standard Tile Types** for the six the game started with |
| `TerrainTile` | A paintable tile that carries a `TileType`. | Assets > Create > Detective Dave > Level > Terrain Tile |
| `TileMapGroup` | On a Grid: the tilemaps that make up one level's shape, in priority order, and the level they bake into. | GameObject > Detective Dave > Level > Tile Map Group, or Add Component |
| `LevelDefinition` | One level: display name, background (+ tint), optional style template, one `TileStyle` per type (tint + sprite per key shape), and the baked `LevelTerrain`. | Assets > Create > Detective Dave > Level > Level Definition, or **Create Level…** on a group |
| `LevelTerrain` | The baked shape: a type per cell, in board space. Read-only at runtime. | Baked from a group |

### Groups

Any number of `TileMapGroup`s can exist, in any scenes or prefabs. **Each group is one level's shape and bakes into exactly
one `LevelDefinition`. Groups never merge.** Two open groups targeting the same level is an error, and neither is baked.

Within a group the **layer list is the priority order**: the first layer with a *typed* tile in a cell decides the cell. A tile is
typed when its layer has a `Type`, or when it is a `TerrainTile` with a type. An untyped tile is skipped and the next layer
decides. With no layers listed, every Tilemap under the group is read, in hierarchy order. All layers must share one Grid.

### Board space

Baked positions are in the **local space of the group's parent** (or of the group itself when it has no parent). Parent the
Grid to the keyboard object, at the origin, so one key is one unit and the terrain lines up with the keys. The keyboard grid
in this project uses a 0.25 cell size: one key is 4 x 4 tiles.

### One source of truth

At runtime only the baked `LevelTerrain` is read, never tilemaps. Bakes happen in the editor:

- the **Bake** button on the group (or **Bake Now** on the level),
- **Tools > Detective Dave > Levels > Bake All Groups In Open Scenes**,
- automatically before entering Play, when a scene is saved and when a prefab is saved (toggle: **Tools > Detective Dave > Levels > Auto-Bake Before Play And On Save**; off in batch mode).

A bake only writes when the shape actually changed. Both inspectors show **Out of date** when the tilemaps differ from the bake.

**First open:** `Assets/Game/Level/Levels/SampleLevel.asset` ships with an empty shape (it could not be baked outside Unity).
The Tile Map Group in `Assets/Scenes/KeyboardLevel.unity` targets it, so the first Play or scene save in that scene fills it;
or press Bake on that group.

Lookups on `LevelDefinition` walk the template chain and allocate a little: use them when a level is set up or redrawn, not every frame.

### Key art and templates

A `TileStyle` gives a type a tint and a list of `KeyShapeSprite`s. A shape is a size in key units (`1 x 1`, `2.25 x 1`, ...),
matched with a 0.01 tolerance. Multi-rectangle keys (the L-shaped Enter) can have a **whole key** sprite whose size is the
box around the key (`1.5 x 2`); without one, each rectangle uses the plain sprite for its own size.

Lookup order for a sprite, a style or the background: this level, then its **Style Template**, then that one's template, ...
Make one "key art" level, point every level at it, and override only what differs.

## Runtime API

```csharp
// LevelDefinition (ScriptableObject)
string DisplayName { get; }
Sprite Background { get; }                 // own, else inherited from the template chain; may be null
Color BackgroundTint { get; }
LevelDefinition StyleTemplate { get; }
IReadOnlyList<TileStyle> TileStyles { get; }
LevelTerrain Terrain { get; }              // never null; empty until baked
TileStyle FindStyle(TileType type);
Sprite FindSprite(TileType type, Vector2 size, bool wholeKey = false);

// LevelTerrain (baked, board space)
bool IsEmpty { get; }
int PaintedCellCount { get; }
RectInt CellBounds { get; }
Vector2 CellSize { get; }
IReadOnlyList<TileType> Types { get; }
Vector2Int ToCell(Vector2 boardPosition);
Vector2 CellCenter(Vector2Int cell);
MapTile GetTile(Vector2 boardPosition);
MapTile GetTile(Vector2Int cell);
IReadOnlyList<Vector2> FindPath(Vector2 start, Vector2 goal, Func<MapTile, bool> passable = null);  // A*, 8 directions

// MapTile (struct): TileType Type, Vector2Int Cell, bool IsEmpty, bool IsWalkable
// TileType: Color EditorColor, bool IsWalkable, string Description (subclass for game-specific data)
// TerrainTile : Tile: TileType Type
// TileMapGroup: LevelDefinition Level, IReadOnlyList<TileMapLayer> Layers, Transform BoardSpace (authoring only: the runtime reads the bake)
// TileMapLayer: Tilemap Tilemap, TileType Type
// TileStyle: TileType Type, Color Tint, IReadOnlyList<KeyShapeSprite> Shapes, Sprite FindSprite(Vector2, bool)
// KeyShapeSprite: Label, Size, WholeKey, Sprite, Matches(Vector2, bool), SizeTolerance
// PathWalker: walks a FindPath result at a speed
```

Editor API (`DetectiveDave.Levels.Editor.LevelBaker`): `Bake(group)`, `BakeInto(group, recordUndo)`, `BakeOpenScenes()`, `IsUpToDate(group)`.

Game code compares types by asset reference: keep a `[SerializeField] TileType goal;` and test `tile.Type == goal`.

## Assemblies

| Assembly | Platforms | References |
|---|---|---|
| `DetectiveDave.Levels` | all | Unity engine modules only (Tilemap) |
| `DetectiveDave.Levels.Editor` | Editor | `DetectiveDave.Levels` |
| `DetectiveDave.Levels.Tests.Editor` | Editor, `UNITY_INCLUDE_TESTS` | both, test runner, NUnit |

There is no reference to the input package or to game code, in either direction. Game code (Assembly-CSharp) references both
and does the joining: key position to board position, tile to key sprite.

## Tests

Edit mode tests in `Tests/Editor`: layer priority and fall-through, layer type override, board-space mapping, independent groups,
shared-level refusal, up-to-date detection, terrain queries and pathfinding, template fallback and loops, sprite tolerance,
sprite-name parsing. Run them from Window > General > Test Runner (the package is listed under `testables` in `Packages/manifest.json`).
