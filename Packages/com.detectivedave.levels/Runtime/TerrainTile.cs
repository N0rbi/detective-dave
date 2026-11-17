using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// A paintable tile that carries a <see cref="TileType"/>. Drag these into a Tile Palette and paint the level's shape.
    /// A layer with its own type (<see cref="TileMapLayer.Type"/>) overrides the tile's type, so plain tiles work there too.
    /// </summary>
    [CreateAssetMenu(menuName = "Detective Dave/Level/Terrain Tile", fileName = "NewTerrainTile", order = 2)]
    public class TerrainTile : Tile
    {
        [Tooltip("What a key painted with this tile becomes, unless the tilemap layer sets its own type.")]
        [SerializeField] TileType type;

        /// <summary>The type this tile paints, or null when none is set.</summary>
        public TileType Type => type;
    }
}
