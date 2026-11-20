using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace DetectiveDave.EditorTools
{
    public sealed class ShiftEndsCheck : ILevelCheck
    {
        public void Check(Level level, List<LevelIssue> issues) => Check(level, KeyboardLayouts.All, issues);

        internal void Check(Level level, IReadOnlyList<KeyboardLayout> layouts, List<LevelIssue> issues)
        {
            if (!level || !level.Shape) return;
            ShiftEnds.Of(level.StartKey, out var start, out var end);
            foreach (var layout in layouts) CheckLayout(level, layouts, layout, start, end, issues);
        }

        static void CheckLayout(Level level, IReadOnlyList<KeyboardLayout> layouts, KeyboardLayout layout, Key start, Key end, List<LevelIssue> issues)
        {
            var keys = layout.UniqueKeys().Where(def => def.InputKey.HasValue && !def.IsInert).ToDictionary(def => def.InputKey.Value);
            bool complete = true;
            foreach (var shift in new[] { start, end })
                if (!keys.ContainsKey(shift))
                {
                    complete = false;
                    issues.Add(new LevelIssue(MessageType.Warning, $"The {layout.Name} keyboard has no {SideName(shift)} Shift key: the level cannot be played on it.", level));
                }
            if (!complete) return;

            CheckPainted(level, layouts, layout, keys[start], start, start, issues);
            CheckPainted(level, layouts, layout, keys[end], end, start, issues);
            if (!IsReachable(level, layout, keys, start, end))
                issues.Add(new LevelIssue(MessageType.Error, $"On the {layout.Name} keyboard there is no way from {SideName(start)} Shift (Start) to {SideName(end)} Shift (End): walls or gaps block it.", level));
        }

        static void CheckPainted(Level level, IReadOnlyList<KeyboardLayout> layouts, KeyboardLayout layout, KeyDef def, Key shift, Key start, List<LevelIssue> issues)
        {
            var settings = LevelSettings.instance;
            var tile = TileOf(level, layout, def);
            if (tile == settings.FloorType && tile) return;
            var role = shift == start ? "Start" : "End";
            var floor = settings.FloorType;
            bool editable = PrefabStageUtility.GetPrefabStage(level.gameObject) != null && floor;
            var message = tile
                ? $"The {role} key ({SideName(shift)} Shift) is painted {tile.name}. The game ignores it: Shift keys are always the Start and the End."
                : $"The {role} key ({SideName(shift)} Shift) is not painted, so it is drawn empty.{(editable ? "" : " Use Paint Floor in Prefab Mode.")}";
            if (issues.Any(issue => issue.Message == message)) return;
            issues.Add(editable
                ? new LevelIssue(MessageType.Warning, message, level, tile ? "Make it Floor" : "Paint Floor", () => PaintFloor(level, layouts, shift, floor))
                : new LevelIssue(MessageType.Warning, message, level));
        }

        static bool IsReachable(Level level, KeyboardLayout layout, Dictionary<Key, KeyDef> keys, Key start, Key end)
        {
            var graph = new KeyGraph(layout);
            var seen = new HashSet<Key> { start };
            var queue = new Queue<Key>();
            queue.Enqueue(start);
            while (queue.Count > 0)
                foreach (var next in graph.Of(queue.Dequeue()))
                {
                    if (!keys.TryGetValue(next, out var def) || seen.Contains(next)) continue;
                    if (next != end && !IsPassable(TileOf(level, layout, def))) continue;
                    seen.Add(next);
                    queue.Enqueue(next);
                }
            return seen.Contains(end);
        }

        static bool IsPassable(TileType tile)
        {
            var settings = LevelSettings.instance;
            if (!tile) return false;
            return tile == settings.WallGutterType || (tile.IsWalkable && tile != settings.GapType);
        }

        static TileType TileOf(Level level, KeyboardLayout layout, KeyDef def) =>
            level.Shape.GetTile(KeyGeometry.ReadCell(level.Shape, level.BoardSpace, layout, def)) as TileType;

        internal static void PaintFloor(Level level, IReadOnlyList<KeyboardLayout> layouts, Key shift, TileBase floor)
        {
            var shape = level ? level.Shape : null;
            if (!shape) return;
            Undo.RegisterCompleteObjectUndo(shape, "Paint Shift Key Floor");
            foreach (var layout in layouts)
                foreach (var def in layout.UniqueKeys().Where(def => def.InputKey == shift))
                    foreach (var cell in KeyGeometry.Cells(shape, level.BoardSpace, layout, def)) shape.SetTile(cell, floor);
        }

        static string SideName(Key shift) => shift == Key.LeftShift ? "Left" : "Right";
    }
}
