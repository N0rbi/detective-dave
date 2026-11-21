using System.Collections.Generic;
using System.Linq;
using DetectiveDave.GuardPlay;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using UnityEngine.InputSystem;

namespace DetectiveDave.EditorTools
{
    public static class GuardPlacements
    {
        static readonly Dictionary<KeyboardLayout, KeyGraphBoard> Boards = new Dictionary<KeyboardLayout, KeyGraphBoard>();

        public static GuardPlacement Judge(Level level, GuardPatrol patrol, KeyboardLayout layout)
        {
            var keys = layout.UniqueKeys().Where(def => def.InputKey.HasValue).ToDictionary(def => def.InputKey.Value);
            ShiftEnds.Of(level.StartKey, out var playerStart, out _);
            return GuardPlacement.Judge(patrol.StartKey, patrol.EndKey, keys.ContainsKey, key => KindOf(level, layout, keys, key), BoardOf(layout), playerStart);
        }

        public static string LabelOf(KeyboardLayout layout, Key key)
        {
            if (key == Key.None) return "not placed";
            var def = layout.UniqueKeys().FirstOrDefault(candidate => candidate.InputKey == key);
            return def != null ? def.Label : key.ToString();
        }

        static KeyGraphBoard BoardOf(KeyboardLayout layout)
        {
            if (!Boards.TryGetValue(layout, out var board)) Boards[layout] = board = new KeyGraphBoard(layout);
            return board;
        }

        static CellKind KindOf(Level level, KeyboardLayout layout, Dictionary<Key, KeyDef> keys, Key key)
        {
            if (key == Key.LeftShift || key == Key.RightShift) return CellKind.Floor;
            if (!keys.TryGetValue(key, out var def) || !level.Shape) return CellKind.Wall;
            var tile = level.Shape.GetTile(KeyGeometry.ReadCell(level.Shape, level.BoardSpace, layout, def)) as TileType;
            var settings = LevelSettings.instance;
            if (!tile) return CellKind.Wall;
            if (tile == settings.GapType) return CellKind.Gap;
            if (tile == settings.WallGutterType) return CellKind.Gutter;
            return tile.IsWalkable ? CellKind.Floor : CellKind.Wall;
        }
    }
}
