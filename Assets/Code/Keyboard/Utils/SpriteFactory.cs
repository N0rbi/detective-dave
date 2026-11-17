using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    public static class SpriteFactory
    {
        const int TexturePixels = 1;
        const float PixelsPerUnit = 1f;                      // 1 pixel per unit makes the 1x1 texture exactly one world unit
        static readonly Vector2 CenterPivot = new Vector2(0.5f, 0.5f);

        /// <summary>A white sprite one world unit square, pivot in the middle. Scale the transform to size a rectangle.</summary>
        public static Sprite CreateUnitSquare()
        {
            var texture = new Texture2D(TexturePixels, TexturePixels, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            var area = new Rect(0f, 0f, TexturePixels, TexturePixels);
            return Sprite.Create(texture, area, CenterPivot, PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }
    }
}
