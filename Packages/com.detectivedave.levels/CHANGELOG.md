# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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
