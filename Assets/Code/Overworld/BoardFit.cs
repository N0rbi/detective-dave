using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class BoardFit
    {
        public static float Scale(Vector2 spriteSize, Vector2 viewSize, BoardFitMode mode, float marginShare, float scaleMultiplier)
        {
            var usable = viewSize * (1f - 2f * marginShare);
            var ratios = new Vector2(usable.x / spriteSize.x, usable.y / spriteSize.y);
            var baseScale = mode == BoardFitMode.Cover ? Mathf.Max(ratios.x, ratios.y) : Mathf.Min(ratios.x, ratios.y);
            return baseScale * scaleMultiplier;
        }

        public static Vector2 Position(Vector2 viewCentre, Vector2 viewSize, Vector2 offsetShare) =>
            viewCentre + new Vector2(offsetShare.x * viewSize.x, offsetShare.y * viewSize.y);
    }
}
