using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using DetectiveDave.Levels.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// The checks that need keyboards, run by the level package's validator (it finds every <see cref="ILevelCheck"/>):
    /// <list type="bullet">
    /// <item>Start and Goal: exactly one key of each on every layout.</item>
    /// <item>Keys that read differently on another layout than on the preview layout (a key painted on UK that is empty on JIS, say).</item>
    /// <item>Keys of the preview layout that are only partly painted or mixed, with a fix that fills them with the kind at their centre.</item>
    /// </list>
    /// </summary>
    public sealed class KeyboardLayoutChecks : ILevelCheck
    {
        const int ListedKeys = 6;

        public void Check(Level level, List<LevelIssue> issues)
        {
            if (!level || !level.Shape) return;
            var layouts = KeyboardLayouts.All;
            var preview = KeyboardLayouts.Preview;
            if (layouts.Count == 0 || preview == null) return;
            var terrain = level.BuildTerrain();
            if (terrain == null || terrain.IsEmpty) return;      // the package reports an empty shape

            var settings = LevelSettings.instance;
            var start = settings.StartType;
            var goal = settings.GoalType;
            var previewRead = Read(terrain, preview);

            foreach (var layout in layouts)
            {
                var read = layout == preview ? previewRead : Read(terrain, layout);
                CheckCount(issues, level, layout, read, start, "Start");
                CheckCount(issues, level, layout, read, goal, "Goal");
                if (layout != preview) CheckDifferences(issues, level, layout, read, preview, previewRead);
            }
            CheckMixed(issues, level, preview);
        }

        /// <summary>What every (non-inert) key of a layout reads, by key.</summary>
        static Dictionary<UnityEngine.InputSystem.Key, (KeyDef def, TileType type)> Read(LevelTerrain terrain, KeyboardLayout layout)
        {
            var read = new Dictionary<UnityEngine.InputSystem.Key, (KeyDef, TileType)>();
            foreach (var key in layout.UniqueKeys())
            {
                if (key.IsInert) continue;
                read[key.InputKey.Value] = (key, terrain.GetTile(KeyGeometry.Center(layout, key) + KeyGeometry.CenterNudge).Type);
            }
            return read;
        }

        static void CheckCount(List<LevelIssue> issues, Level level, KeyboardLayout layout,
            Dictionary<UnityEngine.InputSystem.Key, (KeyDef def, TileType type)> read, TileType type, string what)
        {
            if (!type) return;
            var keys = read.Values.Where(entry => entry.type == type).Select(entry => entry.def.Label).ToList();
            if (keys.Count == 0)
                issues.Add(new LevelIssue(MessageType.Error, $"No {what} key on the {layout.Name} keyboard.", level));
            else if (keys.Count > 1)
                issues.Add(new LevelIssue(MessageType.Warning, $"{keys.Count} {what} keys on the {layout.Name} keyboard: {List(keys)}.", level));
        }

        static void CheckDifferences(List<LevelIssue> issues, Level level, KeyboardLayout layout,
            Dictionary<UnityEngine.InputSystem.Key, (KeyDef def, TileType type)> read,
            KeyboardLayout preview, Dictionary<UnityEngine.InputSystem.Key, (KeyDef def, TileType type)> previewRead)
        {
            var differ = new List<string>();
            foreach (var entry in previewRead)
            {
                if (!read.TryGetValue(entry.Key, out var other) || other.type == entry.Value.type) continue;
                differ.Add($"{entry.Value.def.Label} ({Name(other.type)} instead of {Name(entry.Value.type)})");
            }
            if (differ.Count == 0) return;
            int empty = previewRead.Count(entry => entry.Value.type && read.TryGetValue(entry.Key, out var other) && !other.type);
            var summary = empty == differ.Count
                ? $"{differ.Count} keys empty on the {layout.Name} keyboard that are painted on {preview.Name}"
                : $"{differ.Count} keys read differently on the {layout.Name} keyboard than on {preview.Name}";
            issues.Add(new LevelIssue(MessageType.Warning, $"{summary}: {List(differ)}.", level));
        }

        static void CheckMixed(List<LevelIssue> issues, Level level, KeyboardLayout preview)
        {
            var shape = level.Shape;
            var mixed = new List<KeyDef>();
            foreach (var key in preview.UniqueKeys())
            {
                TileBase first = null;
                bool any = false, differs = false;
                foreach (var cell in KeyGeometry.Cells(shape, level.BoardSpace, preview, key))
                {
                    var tile = shape.GetTile(cell);
                    if (!any) { first = tile; any = true; }
                    else if (tile != first) { differs = true; break; }
                }
                if (differs) mixed.Add(key);
            }
            if (mixed.Count == 0) return;
            var message = $"{mixed.Count} keys on the {preview.Name} keyboard are partly painted or mixed: {List(mixed.Select(key => key.Label).ToList())}.";
            bool editable = PrefabStageUtility.GetPrefabStage(level.gameObject) != null;
            issues.Add(editable
                ? new LevelIssue(MessageType.Warning, message, level, "Fill with centre kind", () => FillWithCentre(level, preview, mixed))
                : new LevelIssue(MessageType.Warning, message, level));
        }

        static void FillWithCentre(Level level, KeyboardLayout layout, List<KeyDef> keys)
        {
            var shape = level ? level.Shape : null;
            if (!shape) return;
            Undo.RegisterCompleteObjectUndo(shape, "Fill Keys With Centre Kind");
            foreach (var key in keys)
            {
                var tile = shape.GetTile(KeyGeometry.ReadCell(shape, level.BoardSpace, layout, key));
                foreach (var cell in KeyGeometry.Cells(shape, level.BoardSpace, layout, key)) shape.SetTile(cell, tile);
            }
        }

        static string Name(TileType type) => type ? type.name : "empty";

        static string List(List<string> items) =>
            items.Count <= ListedKeys ? string.Join(", ", items) : string.Join(", ", items.Take(ListedKeys)) + $" and {items.Count - ListedKeys} more";
    }
}
