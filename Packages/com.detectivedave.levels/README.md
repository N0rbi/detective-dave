# Detective Dave Levels (`com.detectivedave.levels`) 0.2.0

Levels as data. **A level is one prefab** in `Assets/Levels`: a `Level` component on the root (display name, Key Art,
background) and a `Shape` tilemap painted with **Tile Types**, one asset per kind of key. The game reads the shape straight
from the tilemap; nothing is baked or copied.

The package knows nothing about keyboards or input. It answers questions in *board space*; game code maps keys to board
positions, draws the keys and loads levels. Designer guide (no code): [Documentation~/index.md](Documentation~/index.md).

## Concepts

| Thing | What it is | Where |
|---|---|---|
| `TileType : Tile` | One kind of key **and** the tile you paint it with: editor colour, walkable flag, notes, plus the tile's own sprite/colour for painting. | `Assets/Tiles/<Kind>.asset` (Create > Detective Dave > Tile Type) |
| Level Tiles palette | The Tile Palette with every Tile Type. | `Assets/Tiles/Level Tiles.prefab` |
| `KeyArt` | How keys look in game: per Tile Type a tint and a sprite per key shape; a default background. | `Assets/Key Art/Default Key Art.config.asset` (Create > Detective Dave > Key Art) |
| `Level` | Root of a level prefab: `displayName`, `keyArt`, `shape` (Tilemap), `background` (SpriteRenderer). | `Assets/Levels/<Name>.prefab` |
| Level Template | What new levels are copied from (Grid cell 0.25, empty Shape, Background, Default Key Art). | `Assets/Levels/Templates/Level Template.prefab` |
| `LevelTerrain` | The shape as a lookup, built from the Shape tilemap when a level loads. | runtime only |

### Board space

A level prefab is placed under the keyboard object at its local origin, so the level root's parent's local space is board
space (`Level.BoardSpace`; the root itself while the prefab is open on its own). One key is one unit for a 15-unit-wide layout;
the keyboard grid in this project uses a 0.25 cell and stretches keys 2x taller than wide (`Key Height Stretch`, kept equal in Project Settings > Detective Dave > Levels and on the Keyboard Grid View), so a key is 4 x 8 cells. A level remembers the stretch it was painted with; when it differs, the editor redraws its paint to fit (on load, or with the **Fit paint to keys** check button). Key positions come from game code.

The migrated `Level 01` is the exception on purpose: its Grid keeps the 0.5 cell at (0, 1.04) it was painted with before 0.2.0,
so every key reads exactly what it read then. New levels (from the template) use 0.25 at the origin. The checks accept any
cell size; repainting `Level 01` with the Key brush on a 0.25 grid is a designer task, not a fix.

### One source of truth

The `Shape` tilemap inside the level prefab is the level's shape. `Level.BuildTerrain()` (or `LevelTerrain.FromTilemap`) reads
it when the level is loaded or changed. There is no bake step, no copy and no scene to keep in sync.

## Runtime API (`DetectiveDave.Levels`)

```csharp
// Level (MonoBehaviour, root of a level prefab)
string DisplayName { get; }          // field, else the file name (without "(Clone)")
KeyArt KeyArt { get; }
Tilemap Shape { get; }
SpriteRenderer BackgroundRenderer { get; }
Sprite Background { get; }            // the renderer's sprite, else KeyArt.DefaultBackground
Color BackgroundTint { get; }
Transform BoardSpace { get; }         // parent, or the root itself
LevelTerrain BuildTerrain();          // allocates: on load/change, not per frame

// KeyArt (ScriptableObject)
IReadOnlyList<TileStyle> TileStyles { get; }
TileStyle FindStyle(TileType type);
Sprite FindSprite(TileType type, Vector2 size, bool wholeKey = false);
Sprite DefaultBackground { get; }  Color DefaultBackgroundTint { get; }
event Action Changed;                  // raised from OnValidate (editor only)

// LevelTerrain
static LevelTerrain FromTilemap(Tilemap tilemap, Transform boardSpace);   // TileType tiles only; allocates
bool IsEmpty; int PaintedCellCount; RectInt CellBounds; Vector2 CellSize; IReadOnlyList<TileType> Types;
Vector2Int ToCell(Vector2 board); Vector2 CellCenter(Vector2Int cell);
MapTile GetTile(Vector2 board); MapTile GetTile(Vector2Int cell);
IReadOnlyList<Vector2> FindPath(Vector2 start, Vector2 goal, Func<MapTile, bool> passable = null);  // A*, 8 directions

// TileType : Tile — EditorColor, IsWalkable, Description.  MapTile — Type, Cell, IsEmpty, IsWalkable.
// TileStyle — Type, Tint, Shapes, FindSprite.  KeyShapeSprite — Label, Size, WholeKey, Sprite, Matches, SizeTolerance.
// PathWalker — walks a FindPath result.
```

Game code compares kinds by asset reference: keep a `[SerializeField] TileType gap;` and test `tile.Type == gap`.

## Editor API (`DetectiveDave.Levels.Editor`)

- `LevelValidator.Validate(Level)` runs every `ILevelCheck` found through `TypeCache` (any editor assembly; game code adds
  keyboard-layout checks this way, nothing to register). A check that throws is logged and reported as one error.
  `LevelValidator.Validate(KeyArt)` checks key art.
- `LevelIssue` (Severity, Message, Context, FixLabel, Fix) and `ILevelCheck { void Check(Level, List<LevelIssue>); }`.
- Built-in checks: no Key Art (fix: use Default Key Art), no Shape (fix: use the child tilemap / create one), no Background
  renderer (fix: add), level outside `Assets/Levels` (fix: `AssetDatabase.MoveAsset`), tiles that are not a kind of key or whose
  kind was deleted (fix: erase), painted kinds the Key Art cannot draw (fix: open the Key Art), kinds missing from the Level
  Tiles palette (fix: add them; creates the palette with `GridPaletteUtility.CreateNewPalette` if it is missing).
- Inspectors for `Level` and `KeyArt` show the checks with fix buttons; results are cached and recomputed only after
  `Tilemap.tilemapTileChanged`, undo/redo, hierarchy/project changes or an inspector edit.
- `LevelPalette` (DefaultPath, Find, Contains, Add, Create), `LevelAssets` (FindAllTileTypes, FindDefaultKeyArt, EnsureFolder,
  Create > Detective Dave > Standard Tile Types), `ShapeMerger.Merge` (old one-tilemap-per-kind levels into one Shape, logging
  every overlapping cell).

Level window, Key brush, key outlines, Play This Level, level order and the loader are game code (they need the keyboard).

`[HelpURL]` (the ? in the inspector header) points at this guide on GitHub (`LevelsDocs.GuideUrl`); it only opens when the
repository is reachable at that address, so update the constant if the repo moves.

## Assemblies

| Assembly | Platforms | References |
|---|---|---|
| `DetectiveDave.Levels` | all | Unity engine modules only (Tilemap) |
| `DetectiveDave.Levels.Editor` | Editor | `DetectiveDave.Levels`, `Unity.2D.Tilemap.Editor` (palette creation) |
| `DetectiveDave.Levels.Tests.Editor` | Editor, `UNITY_INCLUDE_TESTS` | both, test runner, NUnit |

No reference to the input package or game code.

## Tests

Edit mode: `FromTilemap` (board space, trimming, non-key tiles), `BuildTerrain` board space, Level and Key Art lookups, the
checks and their fixes, a throwing game check, `ShapeMerger` overlap logging, and `ProjectIntegrityTests` (no asset references a
file removed in 0.2.0; no missing scripts in any prefab or scene of the project).
