using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace DetectiveDave.Level
{
    [System.Serializable]
    public class GridItems
    {
        public Tilemap Ground;
        public Tilemap Wall;
        public Tilemap Ledge;
        public Tilemap Gap;
        public Tilemap Start;
        public Tilemap Goal;

        public TileKind GetKindFor(Vector2 position)
        {
            TileKind[] groundTiles = GetTilesBlock(Ground, position, TileKind.Walkable);
            TileKind[] wallTiles = GetTilesBlock(Wall, position, TileKind.Wall);
            //TileBase[] ledgeTiles = GetTilesBlock(Ledge, position, );
            TileKind[] gapTiles = GetTilesBlock(Gap, position, TileKind.Gap);
            TileKind[] startTiles = GetTilesBlock(Start, position, TileKind.Start);
            TileKind[] goalTiles = GetTilesBlock(Goal, position, TileKind.Goal);
            
            TileKind[] allSurroundingTiles = goalTiles.Concat(startTiles)
                .Concat(gapTiles).Concat(wallTiles)
                .Concat(groundTiles).ToArray();

            TileKind chosen = allSurroundingTiles.First();
            
            return chosen;
        }

        private TileKind[] GetTilesBlock(Tilemap map, Vector2 position, TileKind kind)
        {
            return map.GetTilesBlock(new BoundsInt(new Vector3Int((int)position.x, (int)position.y, 0), new Vector3Int(1, 1, 0))).Select((_ => kind)).ToArray();
        }
    }
}