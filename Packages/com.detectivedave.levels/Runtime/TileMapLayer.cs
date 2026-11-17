using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels
{
    /// <summary>One tilemap of a <see cref="TileMapGroup"/>, and what its tiles mean.</summary>
    [Serializable]
    public sealed class TileMapLayer
    {
        [Tooltip("A Tilemap under the same Grid as the group's other layers.")]
        [SerializeField] Tilemap tilemap;

        [Tooltip("Every tile painted on this tilemap becomes this type. Leave empty to use the type of each Terrain Tile instead.")]
        [SerializeField] TileType type;

        /// <summary>For serialization.</summary>
        public TileMapLayer() { }

        /// <summary>A layer reading <paramref name="tilemap"/>; <paramref name="type"/> null lets each <see cref="TerrainTile"/> decide.</summary>
        public TileMapLayer(Tilemap tilemap, TileType type = null)
        {
            this.tilemap = tilemap;
            this.type = type;
        }

        /// <summary>The tilemap this layer reads.</summary>
        public Tilemap Tilemap => tilemap;

        /// <summary>The type of every tile on this layer, or null to ask each tile.</summary>
        public TileType Type => type;

        /// <summary>The type a painted tile stands for on this layer, or null when the tile means nothing here.</summary>
        internal TileType Resolve(TileBase tile)
        {
            if (!tile) return null;
            if (type) return type;
            return tile is TerrainTile terrainTile ? terrainTile.Type : null;
        }
    }
}
