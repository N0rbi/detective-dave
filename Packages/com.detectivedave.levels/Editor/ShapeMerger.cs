using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// Merges an old-style level, painted as one tilemap per kind of key, into a single Shape tilemap. The layer list is the
    /// priority: the first layer with a tile in a cell decides it. Every cell two layers disagree on is logged.
    /// </summary>
    public static class ShapeMerger
    {
        /// <summary>One source tilemap and the kind of key its tiles stand for (null: the tile itself must be a <see cref="TileType"/>).</summary>
        public readonly struct Layer
        {
            /// <summary>The source tilemap.</summary>
            public readonly Tilemap Tilemap;

            /// <summary>What every tile on it means, or null to use the tile itself.</summary>
            public readonly TileType Type;

            /// <summary>A source layer.</summary>
            public Layer(Tilemap tilemap, TileType type)
            {
                Tilemap = tilemap;
                Type = type;
            }
        }

        /// <summary>
        /// Paints <paramref name="target"/> from <paramref name="layers"/>, highest priority first, in the same cell coordinates
        /// (the layers should share the target's Grid). Returns how many cells had a conflict; each is passed to <paramref name="log"/>.
        /// Tiles that are not a kind of key on a layer without a type are skipped.
        /// </summary>
        public static int Merge(IReadOnlyList<Layer> layers, Tilemap target, Action<string> log = null)
        {
            if (layers == null) throw new ArgumentNullException(nameof(layers));
            if (!target) throw new ArgumentNullException(nameof(target));
            var decided = new Dictionary<Vector3Int, (TileType type, string layer)>();
            int conflicts = 0;
            foreach (var layer in layers)
            {
                if (!layer.Tilemap) continue;
                foreach (var cell in layer.Tilemap.cellBounds.allPositionsWithin)
                {
                    var tile = layer.Tilemap.GetTile(cell);
                    if (!tile) continue;
                    var type = layer.Type ? layer.Type : tile as TileType;
                    if (!type) continue;
                    if (decided.TryGetValue(cell, out var kept))
                    {
                        if (kept.type == type) continue;
                        conflicts++;
                        log?.Invoke($"Cell {(Vector2Int)cell}: kept {kept.type.name} from '{kept.layer}', dropped {type.name} from '{layer.Tilemap.name}'.");
                        continue;
                    }
                    decided[cell] = (type, layer.Tilemap.name);
                }
            }
            foreach (var entry in decided) target.SetTile(entry.Key, entry.Value.type);
            return conflicts;
        }
    }
}
