using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>One cell of a level's terrain. The default value is an empty cell (no type).</summary>
    public readonly struct MapTile
    {
        /// <summary>What the cell is, or null when nothing is painted there.</summary>
        public TileType Type { get; }

        /// <summary>The tilemap cell coordinates, as painted.</summary>
        public Vector2Int Cell { get; }

        /// <summary>True when nothing is painted in this cell.</summary>
        public bool IsEmpty => !Type;

        /// <summary>True when the cell has a type and that type is walkable.</summary>
        public bool IsWalkable => Type && Type.IsWalkable;

        /// <summary>A cell at <paramref name="cell"/> of type <paramref name="type"/> (null for empty).</summary>
        public MapTile(TileType type, Vector2Int cell)
        {
            Type = type;
            Cell = cell;
        }
    }
}
