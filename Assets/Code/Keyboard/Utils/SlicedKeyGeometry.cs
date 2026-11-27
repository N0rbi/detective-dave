using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    public readonly struct SlicedKeyShape
    {
        public readonly float Scale;
        public readonly Vector2 RendererSize;

        public SlicedKeyShape(float scale, Vector2 rendererSize)
        {
            Scale = scale;
            RendererSize = rendererSize;
        }
    }

    public static class SlicedKeyGeometry
    {
        public static bool HasBorder(Vector4 borderPixels) =>
            borderPixels.x > 0f || borderPixels.y > 0f || borderPixels.z > 0f || borderPixels.w > 0f;

        public static float ReferenceBorderPixels(Vector4 borderPixels) =>
            Mathf.Min(Mathf.Min(PositiveOrMax(borderPixels.x), PositiveOrMax(borderPixels.z)), Mathf.Min(PositiveOrMax(borderPixels.y), PositiveOrMax(borderPixels.w)));

        static float PositiveOrMax(float pixels) => pixels > 0f ? pixels : float.MaxValue;

        public static SlicedKeyShape Fit(Vector2 targetSize, Vector4 borderPixels, float pixelsPerUnit, float borderThickness)
        {
            float reference = ReferenceBorderPixels(borderPixels);
            if (reference >= float.MaxValue || pixelsPerUnit <= 0f || targetSize.x <= 0f || targetSize.y <= 0f)
                return new SlicedKeyShape(1f, targetSize);

            float scale = borderThickness * pixelsPerUnit / reference;
            scale = Mathf.Min(scale, LargestScaleThatFits(targetSize.x, borderPixels.x + borderPixels.z, pixelsPerUnit));
            scale = Mathf.Min(scale, LargestScaleThatFits(targetSize.y, borderPixels.y + borderPixels.w, pixelsPerUnit));
            return new SlicedKeyShape(scale, targetSize / scale);
        }

        static float LargestScaleThatFits(float length, float borderPixelsAcross, float pixelsPerUnit) =>
            borderPixelsAcross <= 0f ? float.MaxValue : length * pixelsPerUnit / borderPixelsAcross;
    }
}
