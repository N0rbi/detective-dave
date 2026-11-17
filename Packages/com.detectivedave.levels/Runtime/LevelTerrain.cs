using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// The baked shape of a level: what type every painted cell is, in board space (see <see cref="TileMapGroup"/>).
    /// Read-only at runtime. Made by baking a <see cref="TileMapGroup"/> in the editor (the Bake button), stored in the <see cref="LevelDefinition"/>.
    /// </summary>
    [Serializable]
    public sealed class LevelTerrain
    {
        const int MaxTypes = byte.MaxValue;     // cell value 0 is "empty", 1..255 index the type palette

        [SerializeField] RectInt cellBounds;
        [SerializeField] Vector2 origin;                       // board position of the lower-left corner of cellBounds.min
        [SerializeField] Vector2 cellSize = Vector2.one;
        [SerializeField] TileType[] types = Array.Empty<TileType>();
        [SerializeField] byte[] cells = Array.Empty<byte>();   // row-major from cellBounds.min, 0 = empty, n = types[n - 1]
        [SerializeField] int paintedCells;                     // cells with a type, counted at bake

        /// <summary>An empty terrain: every query answers "nothing here".</summary>
        public LevelTerrain() { }

        /// <summary>
        /// A terrain over <paramref name="cellBounds"/> whose cell <c>cellBounds.min</c> has its lower-left corner at <paramref name="origin"/> in board space.
        /// <paramref name="typeAt"/> is asked once per cell.
        /// </summary>
        internal LevelTerrain(RectInt cellBounds, Vector2 origin, Vector2 cellSize, Func<Vector2Int, TileType> typeAt)
        {
            if (cellSize.x <= 0f || cellSize.y <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be positive.");
            this.cellBounds = new RectInt(cellBounds.position, Vector2Int.Max(cellBounds.size, Vector2Int.zero));
            this.origin = origin;
            this.cellSize = cellSize;

            var palette = new List<TileType>();
            cells = new byte[this.cellBounds.width * this.cellBounds.height];
            for (int y = 0; y < this.cellBounds.height; y++)
                for (int x = 0; x < this.cellBounds.width; x++)
                {
                    var type = typeAt(this.cellBounds.min + new Vector2Int(x, y));
                    if (!type) continue;
                    int index = palette.IndexOf(type);
                    if (index < 0)
                    {
                        if (palette.Count == MaxTypes) throw new InvalidOperationException($"A level can use at most {MaxTypes} tile types.");
                        palette.Add(type);
                        index = palette.Count - 1;
                    }
                    cells[y * this.cellBounds.width + x] = (byte)(index + 1);
                    paintedCells++;
                }
            types = palette.ToArray();
        }

        /// <summary>True when nothing is painted (or the level was never baked).</summary>
        public bool IsEmpty => paintedCells == 0;

        /// <summary>How many cells have a type.</summary>
        public int PaintedCellCount => paintedCells;

        /// <summary>The painted area in tilemap cell coordinates.</summary>
        public RectInt CellBounds => cellBounds;

        /// <summary>The size of one cell in board units (0.25 when a key is 4 x 4 tiles).</summary>
        public Vector2 CellSize => cellSize;

        /// <summary>Every type that appears in the terrain, each once.</summary>
        public IReadOnlyList<TileType> Types => Array.AsReadOnly(types ?? Array.Empty<TileType>());

        byte[] Cells => cells ?? Array.Empty<byte>();

        /// <summary>The cell that contains a board position. Positions on a border belong to the cell above and to the right.</summary>
        public Vector2Int ToCell(Vector2 boardPosition)
        {
            var offset = (boardPosition - origin) / cellSize;
            return cellBounds.min + new Vector2Int(Mathf.FloorToInt(offset.x), Mathf.FloorToInt(offset.y));
        }

        /// <summary>The board position of a cell's center.</summary>
        public Vector2 CellCenter(Vector2Int cell) => origin + (cell - cellBounds.min + new Vector2(0.5f, 0.5f)) * cellSize;

        /// <summary>The tile at a board position: empty outside the painted area.</summary>
        public MapTile GetTile(Vector2 boardPosition) => GetTile(ToCell(boardPosition));

        /// <summary>The tile in a cell: empty outside the painted area.</summary>
        public MapTile GetTile(Vector2Int cell) => new MapTile(TypeAt(cell), cell);

        /// <summary>
        /// The board positions (cell centers) from <paramref name="start"/> to <paramref name="goal"/>, both included,
        /// or an empty list when there is no way. A* over cells, 8 directions, no cutting corners past blocked cells.
        /// <paramref name="passable"/> decides which tiles can be walked on: <see cref="MapTile.IsWalkable"/> when null.
        /// </summary>
        public IReadOnlyList<Vector2> FindPath(Vector2 start, Vector2 goal, Func<MapTile, bool> passable = null)
        {
            passable ??= tile => tile.IsWalkable;
            var cellsOnPath = GridPathfinder.FindPath(ToCell(start), ToCell(goal), cellBounds, cell => passable(GetTile(cell)));
            var path = new Vector2[cellsOnPath.Count];
            for (int i = 0; i < path.Length; i++) path[i] = CellCenter(cellsOnPath[i]);
            return path;
        }

        TileType TypeAt(Vector2Int cell)
        {
            if (!cellBounds.Contains(cell)) return null;
            var local = cell - cellBounds.min;
            int index = local.y * cellBounds.width + local.x;
            var data = Cells;
            if (index >= data.Length) return null;
            int value = data[index];
            return value == 0 || types == null || value > types.Length ? null : types[value - 1];
        }

        /// <summary>True when both terrains hold the same types in the same cells at the same place. Used to tell whether a bake is out of date.</summary>
        internal bool SameAs(LevelTerrain other)
        {
            if (other == null) return IsEmpty;
            if (IsEmpty && other.IsEmpty) return true;
            if (paintedCells != other.paintedCells || !cellBounds.Equals(other.cellBounds) || origin != other.origin || cellSize != other.cellSize) return false;
            for (int y = cellBounds.yMin; y < cellBounds.yMax; y++)
                for (int x = cellBounds.xMin; x < cellBounds.xMax; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (TypeAt(cell) != other.TypeAt(cell)) return false;
                }
            return true;
        }
    }
}
