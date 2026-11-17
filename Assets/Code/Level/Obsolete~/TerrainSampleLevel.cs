using UnityEngine;
using UnityEngine.Tilemaps;

namespace DetectiveDave.Level
{
    /// <summary>
    /// Paints a small sample level into a tilemap when the scene starts, so the board API has something to read.
    /// The pattern is one character per key: . walkable, ~ gap, # wall, H wall with a gutter. Each key is 4 x 4 tiles,
    /// and the pattern is centered on the tilemap's origin. Replace it by painting a level by hand and removing this component.
    /// </summary>
    public class TerrainSampleLevel : MonoBehaviour
    {
        const int TilesPerKey = 4;

        static readonly string[] Pattern =
        {
            "..~.....#H#....",
            "...#...~~~.....",
            ".#H#.......##..",
            "....~~.......#.",
        };

        [SerializeField] Tilemap tilemap;
        [SerializeField] TerrainTile walkable;
        [SerializeField] TerrainTile gap;
        [SerializeField] TerrainTile wall;
        [SerializeField] TerrainTile wallGutter;

        void Start() => Paint();

        [ContextMenu("Paint")]
        void Paint()
        {
            tilemap.ClearAllTiles();
            int left = -Pattern[0].Length * TilesPerKey / 2;
            int top = Pattern.Length * TilesPerKey / 2;
            for (int row = 0; row < Pattern.Length; row++)
                for (int column = 0; column < Pattern[row].Length; column++)
                    PaintKey(TileFor(Pattern[row][column]), left + column * TilesPerKey, top - (row + 1) * TilesPerKey);
        }

        void PaintKey(TerrainTile tile, int x, int y)
        {
            for (int dx = 0; dx < TilesPerKey; dx++)
                for (int dy = 0; dy < TilesPerKey; dy++)
                    tilemap.SetTile(new Vector3Int(x + dx, y + dy, 0), tile);
        }

        TerrainTile TileFor(char symbol)
        {
            switch (symbol)
            {
                case '~': return gap;
                case '#': return wall;
                case 'H': return wallGutter;
                default: return walkable;
            }
        }
    }
}
