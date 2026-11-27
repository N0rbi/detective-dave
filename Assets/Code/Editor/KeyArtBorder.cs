using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class KeyArtBorder
    {
        public const float RimShareOfShortSide = 0.075f;

        public static int RimPixels(int width, int height) =>
            Mathf.Max(1, Mathf.RoundToInt(Mathf.Min(width, height) * RimShareOfShortSide));

        public static Vector4 BoxBorder(int width, int height, int topPixels)
        {
            int rim = RimPixels(width, height);
            return new Vector4(rim, rim, rim, Mathf.Max(rim, topPixels));
        }
    }
}
