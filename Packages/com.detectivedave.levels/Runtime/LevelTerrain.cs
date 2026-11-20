using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Levels
{
    /// <summary>
    /// A level's shape as a lookup: what kind of key every painted cell is, in board space, plus paths between cells.
    /// Built from a level's Shape tilemap with <see cref="FromTilemap"/> (or <see cref="Level.BuildTerrain"/>) when the level is
    /// loaded; it is derived data, never stored. Read-only once built.
    /// </summary>
    public sealed class LevelTerrain
    {
        const int MaxTypes = byte.MaxValue;     // cell value 0 is "empty", 1..255 index the type palette

        readonly RectInt cellBounds;
        readonly Vector2 origin;                       // board position of the lower-left corner of cellBounds.min
        readonly Vector2 cellSize = Vector2.one;
        readonly TileType[] types = Array.Empty<TileType>();
        readonly byte[] cells = Array.Empty<byte>();   // row-major from cellBounds.min, 0 = empty, n = types[n - 1]
        readonly int paintedCells;                     // cells with a type

        /// <summary>An empty terrain: every query answers "nothing here".</summary>
        public LevelTerrain() { }

        /// <summary>
        /// Reads <paramref name="tilemap"/> into a terrain. Only <see cref="TileType"/> tiles count; other tiles are skipped.
        /// Positions are in the local space of <paramref name="boardSpace"/> (the keyboard object; see <see cref="Level.BoardSpace"/>).
        /// The terrain covers the painted cells only. Assumes the tilemap is not rotated relative to <paramref name="boardSpace"/>.
        /// Allocates and reads every cell of the tilemap's bounds: call it when a level is loaded or changed, not every frame.
        /// </summary>
        public static LevelTerrain FromTilemap(Tilemap tilemap, Transform boardSpace)
        {
            if (!tilemap) throw new ArgumentNullException(nameof(tilemap));
            if (!boardSpace) throw new ArgumentNullException(nameof(boardSpace));

            int xMin = int.MaxValue, yMin = int.MaxValue, xMax = int.MinValue, yMax = int.MinValue;
            var area = tilemap.cellBounds;
            for (int y = area.yMin; y < area.yMax; y++)
                for (int x = area.xMin; x < area.xMax; x++)
                {
                    if (!tilemap.GetTile<TileType>(new Vector3Int(x, y, 0))) continue;
                    xMin = Mathf.Min(xMin, x);
                    yMin = Mathf.Min(yMin, y);
                    xMax = Mathf.Max(xMax, x + 1);
                    yMax = Mathf.Max(yMax, y + 1);
                }
            if (xMin > xMax) return new LevelTerrain();

            var bounds = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            Vector2 BoardCenter(Vector2Int cell) => boardSpace.InverseTransformPoint(tilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)));
            var first = BoardCenter(bounds.min);
            var step = new Vector2(
                Mathf.Abs(BoardCenter(bounds.min + Vector2Int.right).x - first.x),
                Mathf.Abs(BoardCenter(bounds.min + Vector2Int.up).y - first.y));
            if (step.x <= 0f || step.y <= 0f) return new LevelTerrain();
            return new LevelTerrain(bounds, first - step * 0.5f, step, cell => tilemap.GetTile<TileType>(new Vector3Int(cell.x, cell.y, 0)));
        }

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
            int painted = 0;
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
                    painted++;
                }
            types = palette.ToArray();
            paintedCells = painted;
        }

        /// <summary>True when nothing is painted.</summary>
        public bool IsEmpty => paintedCells == 0;

        /// <summary>How many cells have a type.</summary>
        public int PaintedCellCount => paintedCells;

        /// <summary>The painted area in tilemap cell coordinates.</summary>
        public RectInt CellBounds => cellBounds;

        /// <summary>The size of one cell in board units (0.25 when a key is 4 x 4 tiles).</summary>
        public Vector2 CellSize => cellSize;

        /// <summary>Every type that appears in the terrain, each once.</summary>
        public IReadOnlyList<TileType> Types => Array.AsReadOnly(types);

        byte[] Cells => cells;

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
            return value == 0 || value > types.Length ? null : types[value - 1];
        }
    }
}
