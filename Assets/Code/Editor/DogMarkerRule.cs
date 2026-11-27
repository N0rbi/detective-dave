using DetectiveDave.Levels;

namespace DetectiveDave.EditorTools
{
    static class DogMarkerRule
    {
        public static bool Marks(bool showDogMarkers, TileType tile, TileType gapType) => showDogMarkers && tile != null && tile == gapType;
    }
}
