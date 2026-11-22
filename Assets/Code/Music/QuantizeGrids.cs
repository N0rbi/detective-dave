using System;

namespace DetectiveDave.MusicPlay
{
    public static class QuantizeGrids
    {
        public static float Beats(QuantizeGrid grid)
        {
            switch (grid)
            {
                case QuantizeGrid.Quarter: return 1f;
                case QuantizeGrid.Eighth: return 0.5f;
                case QuantizeGrid.TripletEighth: return 1f / 3f;
                default: throw new ArgumentOutOfRangeException(nameof(grid), grid, "Off has no grid.");
            }
        }
    }
}
