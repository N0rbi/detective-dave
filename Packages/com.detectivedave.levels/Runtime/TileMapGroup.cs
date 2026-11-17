using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// The painted shape of one level: a set of tilemaps under one Grid, read in priority order, baked into one <see cref="LevelDefinition"/>.
    /// Put it on the Grid object. A scene or prefab may hold any number of groups; each bakes into its own level, groups never merge.
    /// <para>
    /// Priority: the first layer (top of the list) that has a typed tile in a cell decides what the cell is. A tile without a type
    /// (no layer type and not a <see cref="TerrainTile"/> with a type) is skipped, so the next layer gets a say.
    /// With no layers listed, every Tilemap under this object is read, in hierarchy order.
    /// </para>
    /// <para>
    /// Board space: positions in the baked terrain are in the local space of this object's parent (or of this object when it has no parent).
    /// Parent the Grid to the keyboard object, so the terrain lines up with the keys. The runtime never reads the tilemaps: it reads the bake.
    /// </para>
    /// </summary>
    [AddComponentMenu("Detective Dave/Level/Tile Map Group")]
    [DisallowMultipleComponent]
    public sealed class TileMapGroup : MonoBehaviour
    {
        static readonly Color GuideColor = new Color(1f, 1f, 1f, 0.35f);

        [Tooltip("The level this group's shape is baked into. Each level should be baked from exactly one group.")]
        [SerializeField] LevelDefinition level;

        [Tooltip("The tilemaps of this level, highest priority first: the first layer with a typed tile in a cell decides the cell. " +
                 "Leave empty to read every Tilemap under this object.")]
        [SerializeField] TileMapLayer[] layers = Array.Empty<TileMapLayer>();

        [Tooltip("Size of the keyboard outline drawn in the Scene view, with one line per key row, in board units (15 x 4 is the main block at one unit per key). 0 hides it.")]
        [SerializeField] Vector2 boardGuide = new Vector2(15f, 4f);

        /// <summary>The level this group bakes into, or null when none is set.</summary>
        public LevelDefinition Level => level;

        /// <summary>The layers as configured, highest priority first. May contain entries without a tilemap.</summary>
        public IReadOnlyList<TileMapLayer> Layers => Array.AsReadOnly(layers ?? Array.Empty<TileMapLayer>());

        /// <summary>The transform whose local space is board space: the parent, or this object at the root.</summary>
        public Transform BoardSpace => transform.parent ? transform.parent : transform;

        /// <summary>
        /// The layers that are actually read, highest priority first: the configured ones that have a tilemap,
        /// or every Tilemap under this object when none is configured. Bake-time helper: reads live tilemaps and allocates.
        /// </summary>
        internal IReadOnlyList<TileMapLayer> GetActiveLayers()
        {
            var active = new List<TileMapLayer>();
            foreach (var layer in layers ?? Array.Empty<TileMapLayer>())
                if (layer != null && layer.Tilemap) active.Add(layer);
            if (active.Count > 0) return active;
            foreach (var tilemap in GetComponentsInChildren<Tilemap>(true))
                active.Add(new TileMapLayer(tilemap));
            return active;
        }

        /// <summary>The type painted in a tilemap cell of <paramref name="activeLayers"/>, by layer priority, or null when no layer has a typed tile there.</summary>
        internal static TileType GetTileType(IReadOnlyList<TileMapLayer> activeLayers, Vector2Int cell)
        {
            var position = new Vector3Int(cell.x, cell.y, 0);
            foreach (var layer in activeLayers)
            {
                var type = layer.Resolve(layer.Tilemap.GetTile(position));
                if (type) return type;
            }
            return null;
        }

        void OnValidate()
        {
            layers ??= Array.Empty<TileMapLayer>();
            boardGuide = Vector2.Max(boardGuide, Vector2.zero);
        }

        // A keyboard-sized outline with one line per key row (rows are one unit high; keys within a row are staggered, so no columns).
        void OnDrawGizmos()
        {
            if (boardGuide.x <= 0f || boardGuide.y <= 0f) return;
            var previous = Gizmos.matrix;
            Gizmos.matrix = BoardSpace.localToWorldMatrix;
            Gizmos.color = GuideColor;
            var half = boardGuide * 0.5f;
            Gizmos.DrawLine(new Vector3(-half.x, -half.y), new Vector3(-half.x, half.y));
            Gizmos.DrawLine(new Vector3(half.x, -half.y), new Vector3(half.x, half.y));
            for (float y = -half.y; y <= half.y + 1e-3f; y += 1f)
                Gizmos.DrawLine(new Vector3(-half.x, y), new Vector3(half.x, y));
            Gizmos.matrix = previous;
        }
    }
}
