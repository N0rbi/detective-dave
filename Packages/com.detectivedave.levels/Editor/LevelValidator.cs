using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>One finding of the level checks, worded for designers: what is wrong and what to do about it.</summary>
    internal readonly struct LevelIssue
    {
        public readonly MessageType Severity;
        public readonly string Message;

        public LevelIssue(MessageType severity, string message)
        {
            Severity = severity;
            Message = message;
        }
    }

    /// <summary>The checks shown in the inspectors and run by Tools > Detective Dave > Levels > Validate All Levels.</summary>
    internal static class LevelValidator
    {
        /// <summary>Every level that at least one other level uses as its Style Template.</summary>
        public static HashSet<LevelDefinition> FindTemplates() =>
            new HashSet<LevelDefinition>(AssetDatabase.FindAssets("t:" + nameof(LevelDefinition))
                .Select(guid => AssetDatabase.LoadAssetAtPath<LevelDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(level => level && level.StyleTemplate)
                .Select(level => level.StyleTemplate));

        /// <summary>Checks a level. <paramref name="usedAsTemplate"/>: other levels use it as their Style Template, so it needs no shape of its own.</summary>
        public static List<LevelIssue> Validate(LevelDefinition level, bool usedAsTemplate = false)
        {
            var issues = new List<LevelIssue>();
            if (!level) return issues;

            if (level.HasTemplateLoop())
                issues.Add(Error("The Style Template chain loops back to a level already in it. Clear one of the templates."));

            var styles = level.TileStyles;
            for (int i = 0; i < styles.Count; i++)
                if (styles[i] == null || !styles[i].Type)
                    issues.Add(Error($"Style {i + 1} has no Tile Type. Pick one, or remove the style."));

            foreach (var duplicate in styles.Where(style => style != null && style.Type).GroupBy(style => style.Type).Where(group => group.Count() > 1))
                issues.Add(Warning($"'{duplicate.Key.name}' has {duplicate.Count()} styles. Only the first one is used: merge them."));

            foreach (var style in styles.Where(style => style != null))
                for (int i = 0; i < style.Shapes.Count; i++)
                {
                    var shape = style.Shapes[i];
                    if (shape != null && !shape.Sprite && style.Type)
                        issues.Add(Warning($"'{style.Type.name}': shape '{ShapeName(shape)}' has no sprite. Assign one, or remove the row."));
                }

            var terrain = level.Terrain;
            if (terrain.IsEmpty && !usedAsTemplate)
                issues.Add(Warning("No shape yet. Pick this level in a Tile Map Group's 'Level' field, paint, and press Bake."));

            foreach (var type in terrain.Types.Where(type => type))
            {
                if (level.FindStyle(type) == null)
                {
                    issues.Add(Warning($"'{type.name}' is painted in this level but has no style here or in the template: those keys are drawn plain. Add a style for it."));
                    continue;
                }
                var missing = KeyShapePresets.Standard
                    .Where(shape => !shape.WholeKey && !level.FindSprite(type, shape.Size))
                    .Select(shape => shape.Label)
                    .ToList();
                if (missing.Count > 0)
                    issues.Add(Warning($"'{type.name}' has no sprite for: {string.Join(", ", missing)}. Keys of that shape are drawn as flat squares."));
                var enter = KeyShapePresets.Standard.First(shape => shape.WholeKey);
                if (!level.FindSprite(type, enter.Size, true))
                    issues.Add(Info($"'{type.name}' has no whole-key Enter sprite: the L-shaped Enter is drawn from its two rectangle sprites."));
            }

            foreach (var style in styles.Where(style => style != null && style.Type && style.Tint.a <= 0f))
                issues.Add(Warning($"'{style.Type.name}' has a fully transparent Tint: its keys are invisible."));

            if (!level.Background)
                issues.Add(Info("No background sprite: the camera's background colour shows behind the keyboard."));

            return issues;
        }

        /// <summary>
        /// Checks a group. <paramref name="openGroups"/>: the other groups that are open, to catch two groups baking into one level.
        /// <paramref name="baked"/>: the group's bake if the caller already has it (saves baking again).
        /// </summary>
        public static List<LevelIssue> Validate(TileMapGroup group, IReadOnlyList<TileMapGroup> openGroups = null, LevelTerrain baked = null)
        {
            var issues = new List<LevelIssue>();
            if (!group) return issues;

            if (!group.Level)
                issues.Add(Error("No Level: pick the Level Definition this shape is baked into (or press Create Level)."));

            var configured = group.Layers;
            for (int i = 0; i < configured.Count; i++)
                if (configured[i] == null || !configured[i].Tilemap)
                    issues.Add(Warning($"Layer {i + 1} has no Tilemap: it is skipped. Assign one or remove the layer."));

            var layers = group.GetActiveLayers();
            if (layers.Count == 0)
            {
                issues.Add(Error("No tilemaps: add a Tilemap under this object (GameObject > 2D Object > Tilemap) or press 'Add Child Tilemaps'."));
                return issues;
            }

            var grid = layers[0].Tilemap.layoutGrid;
            foreach (var layer in layers.Where(layer => layer.Tilemap.layoutGrid != grid))
                issues.Add(Error($"Tilemap '{layer.Tilemap.name}' is under a different Grid than '{layers[0].Tilemap.name}'. All layers of a group must share one Grid."));

            foreach (var layer in layers.Where(layer => !layer.Tilemap.transform.IsChildOf(group.transform)))
                issues.Add(Warning($"Tilemap '{layer.Tilemap.name}' is not under this object. Keep a group's tilemaps under it, so moving the group moves its shape."));

            foreach (var layer in layers.Where(layer => !layer.Type))
            {
                int untyped = CountUntypedTiles(layer);
                if (untyped > 0)
                    issues.Add(Warning($"{untyped} tile(s) on '{layer.Tilemap.name}' have no type and are ignored. Set the layer's Type, or paint with Terrain Tiles that have a type."));
            }

            var frame = group.BoardSpace;
            var relative = Quaternion.Inverse(frame.rotation) * layers[0].Tilemap.transform.rotation;
            if (Quaternion.Angle(relative, Quaternion.identity) > 0.01f)
                issues.Add(Warning("The tilemaps are rotated relative to the board: the bake assumes straight rows. Reset their rotation."));

            if (openGroups != null && group.Level)
            {
                var others = openGroups.Where(other => other && other != group && other.Level == group.Level).ToList();
                if (others.Count > 0)
                    issues.Add(Error($"Another group also bakes into '{group.Level.name}' ({string.Join(", ", others.Select(LevelBaker.DescribeSource))}). Give each group its own level."));
            }

            if (group.Level)
            {
                var source = LevelBaker.DescribeSource(group);
                var bakedFrom = group.Level.BakedFrom;
                if (!string.IsNullOrEmpty(bakedFrom) && bakedFrom != source)
                    issues.Add(Info($"'{group.Level.name}' was last baked from {bakedFrom}. Baking here replaces that shape."));
            }

            if ((baked ?? LevelBaker.Bake(group)).IsEmpty)
                issues.Add(Info("Nothing typed is painted yet."));

            return issues;
        }

        static int CountUntypedTiles(TileMapLayer layer)
        {
            int count = 0;
            var area = layer.Tilemap.cellBounds;
            foreach (var position in area.allPositionsWithin)
            {
                var tile = layer.Tilemap.GetTile(position);
                if (tile && !layer.Resolve(tile)) count++;
            }
            return count;
        }

        static string ShapeName(KeyShapeSprite shape) =>
            string.IsNullOrWhiteSpace(shape.Label) ? KeyShapePresets.LabelFor(shape.Size, shape.WholeKey) : shape.Label;

        static LevelIssue Error(string message) => new LevelIssue(MessageType.Error, message);
        static LevelIssue Warning(string message) => new LevelIssue(MessageType.Warning, message);
        static LevelIssue Info(string message) => new LevelIssue(MessageType.Info, message);
    }
}
