using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class KeyArtBorder
    {
        public const float RimShareOfShortSide = 0.075f;

        public static int RimPixels(int width, int height) =>
            Mathf.Max(1, Mathf.RoundToInt(Mathf.Min(width, height) * RimShareOfShortSide));
    }
}
