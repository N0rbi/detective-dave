# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added
- `Level.Lighting` (`LevelLightingSettings`): intensity, colour tint and an optional flicker per level. `FlickerPattern` / `FlickerPoint` timelines, `FlickerPicker` (weighted random pick), `FlickerScheduler` (random waits between flickers), `LightingMath`, `LightingProblems`.
- Editor: the Lighting section of the Level inspector with the timeline widget and preview, `LevelLightingCheck`, `LightingPreview` and `LightingPreviewHost`.

### Changed (breaking)
- Level gained Start key (Left/Right Shift); the Start and Goal tile types were retired.

## [0.2.0] - 2026-11-17

Breaking: a level is now one prefab read directly by the game. Data in this project was migrated (see below).

### Removed (breaking)
- `LevelDefinition` as a level asset, `TileMapGroup`, `TileMapLayer`, `TerrainTile`, `LevelBaker`, `LevelAutoBake`
  (Bake button, Bake All, auto-bake toggle), `TileMapGroupEditor`, `LevelDefinitionEditor`, the style template chain
  (`styleTemplate`; a level now references one Key Art, duplicate it to vary).
- `LevelTerrain` is no longer serialized; `LevelTerrain.SameAs` is gone.

### Changed (breaking)
- **Reversal of 0.1.0's "the runtime reads the baked shape only" rule.** The runtime now derives the shape from the level
  prefab's Shape tilemap (`LevelTerrain.FromTilemap`, `Level.BuildTerrain`) when a level is loaded. There is no baked copy to keep
  in sync.
- `TileType` now derives from `Tile` (script GUID f032b887 kept, fields kept): the painted tile is the kind of key. It carries
  the sprite/colour it is painted with.
- `LevelDefinition.cs` became `KeyArt.cs` (script GUID 562a59b6 kept; serialized names `tileStyles`, `background`,
  `backgroundTint` kept), so the existing Default Key Art asset loads as Key Art. Its background is the default for levels without one.
- `package.json` now depends on `com.unity.2d.tilemap` (the Editor assembly references `Unity.2D.Tilemap.Editor` to create the palette).
- Namespace and assembly names unchanged. Create menus moved to Assets > Create > Detective Dave > {Tile Type, Key Art, Standard Tile Types}.

### Added
- `Level` component (`DisplayName`, `KeyArt`, `Shape`, `BackgroundRenderer`, `Background`, `BackgroundTint`, `BoardSpace`, `BuildTerrain`).
- `LevelTerrain.FromTilemap(Tilemap, Transform)`, `LevelTerrain.PaintedCellCount`.
- `KeyArt.Changed` event (raised from `OnValidate`), `KeyArt.DefaultBackground`, `KeyArt.DefaultBackgroundTint`.
- Editor: `ILevelCheck` extension point (found through `TypeCache`), public `LevelIssue` with one-click fixes,
  `LevelValidator.Validate(Level)` / `Validate(KeyArt)`, `Level` and `KeyArt` inspectors, `LevelPalette`, `LevelAssets`,
  `ShapeMerger`. `[HelpURL]` on the public components and assets.
- Tests: FromTilemap, checks, ShapeMerger, project integrity (removed GUIDs, missing scripts).

### Migrated in this project (plain file+meta moves, GUIDs kept; YAML edits)
- `Assets/Game/Level/Types/*.asset` -> `Assets/Tiles/*.asset`, each merged with its Terrain Tile's look (`m_Sprite`, `m_Color`,
  `m_Transform`, `m_Flags`, ...). Every reference to a `Data/*.asset` Terrain Tile was swapped to the matching Tile Type (11 references).
- `Assets/Game/Level/Art/tile_*.png` -> `Assets/Tiles/Sprites/`.
- `Assets/Game/Level/LevelData.prefab` -> `Assets/Tiles/Level Tiles.prefab` (the palette, guid f3017d39 kept; root renamed).
- `Assets/Game/Level/Levels/DefaultKeyArt.asset` -> `Assets/Key Art/Default Key Art.asset` (guid b6c099b0 kept);
  `Assets/Game/Level/View/<Kind>/` -> `Assets/Key Art/<Kind>/`.
- The sample level (six tilemaps of `Assets/Level/Grid.prefab` plus the overrides in `KeyboardLevel.unity`, baked into
  `SampleLevel.asset`) -> `Assets/Levels/Level 01.prefab` with one Shape tilemap, merged by the old layer priority: 270 cells,
  12 overlapping cells resolved (Start/Goal over Walkable), identical to the old bake. Its Grid keeps the old 0.5 cell at
  (0, 1.04) so every key reads what it read before.
- New `Assets/Levels/Templates/Level Template.prefab` and empty `Assets/Backgrounds/`.
- `KeyboardLevel.unity`: the Grid prefab instance and its Tile Map Group were removed.
- Deleted after a reference check: `Assets/Game/Level/{Data,Levels/SampleLevel,Levels/TestLevel}`, `Assets/Level`
  (Grid.prefab, duplicate palette), `Assets/Scenes/LevelDesign.unity`, `Assets/_Recovery`, `Assets/Code/Level/Obsolete~`.

## [0.1.0] - 2026-11-15

### Added
- `TileType` asset: tile types are data (editor colour, walkable flag, notes). Replaces the `TileKind` enum.
- `LevelDefinition` asset: display name, background sprite and tint, style template chain, one `TileStyle` per tile type
  (tint and a sprite per key shape, including whole-key sprites for the L-shaped Enter), baked `LevelTerrain`.
- `LevelTerrain`: baked, board-space shape with tile lookup and A* pathfinding.
- `TileMapGroup`: any number of groups, each bakes one level; ordered layers with fall-through; keyboard outline gizmo.
- Editor: inspectors with checks, previews and fix-up buttons; Bake button and out-of-date status; auto-bake before Play and
  on scene/prefab save; Tools > Detective Dave > Levels menu (Bake All, Validate All, Auto-Bake toggle);
  Create > Detective Dave > Level > Standard Tile Types; GameObject > Detective Dave > Level > Tile Map Group.
- Edit mode tests.

### Migrated from `Assets/Code/Level` (script GUIDs kept, so existing assets and scenes still resolve)
- `TerrainMap.cs` -> `Runtime/TileMapGroup.cs` (guid `fd9efa57ae214a5b9c1b0e3ea4c4be18`). Serialized `layers[].kind` (enum int)
  became `layers[].type` (TileType reference); `level` field added.
- `Models/TerrainLayer.cs` -> `Runtime/TileMapLayer.cs` (`kind` -> `type`).
- `TerrainTile.cs` -> `Runtime/TerrainTile.cs` (guid `10b4356668214dd5859817766c56f79e`). `kind` (enum int) -> `type` (TileType reference).
- `TerrainPalette.cs` -> `Runtime/LevelDefinition.cs` (guid `562a59b6b994464eb444f2e5eedf3fa6`), so the old palette asset is now a
  Level Definition. `Models/TerrainStyle.cs` -> `Runtime/TileStyle.cs`; per-kind `color` -> `tint`, `lShape` -> a whole-key shape entry.
- `Models/KeyShapeSprite.cs`, `Models/MapTile.cs` (`Kind` -> `Type`), `Models/PathWalker.cs`, `Utils/GridPathfinder.cs` (now internal).
- Namespace `DetectiveDave.Level` -> `DetectiveDave.Levels`.

### Data migrated by hand in the project (YAML)
- `Assets/Game/Level/Data/{Walkable,Gap,Wall,WallGutter,Start,Goal}.asset`: `kind: n` -> `type:` reference to `Assets/Game/Level/Types/<same name>.asset`.
- `Assets/Game/Level/View/TerrainPalette.asset` -> `Assets/Game/Level/Levels/DefaultKeyArt.asset` (guid kept), rewritten as a Level Definition
  with the same sprites (8 shapes per type, Enter as a whole-key shape).
- `Assets/Scenes/KeyboardLevel.unity`: the group's six layers `kind: 5/6/1/3/4/2` -> Start/Goal/Walkable/Wall/WallGutter/Gap type references, and `level` set.

### Changed
- Start and Goal types are walkable (the old `MapTile.IsWalkable` was true for Walkable only).
- The runtime reads the bake only; painting tiles while playing no longer changes the terrain.
