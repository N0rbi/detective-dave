using System;

namespace DetectiveDave.OverworldPlay
{
    public static class GlyphCell
    {
        public static float CentreX(int column, float cellWidth) => (column + 0.5f) * cellWidth;

        public static float RightEdge(int column, float cellWidth) => (column + 1) * cellWidth;

        public static int Columns(float areaWidth, float cellWidth) => Math.Max((int)Math.Floor(areaWidth / Math.Max(cellWidth, 0.0001f)), 1);

        public static float HorizontalScale(float glyphWidth, float cellWidth) => glyphWidth <= cellWidth || glyphWidth <= 0f ? 1f : cellWidth / glyphWidth;

        public static float CharacterSize(float cellWidth, float rowStep, float advancePixels, int fontSize, float emScale) => Math.Min(cellWidth / (advancePixels * emScale), rowStep / (fontSize * emScale));

        public static float TopY(float rowOffsetInLines, float lineStep, float baselineShare, float rowShare) => -(rowOffsetInLines * lineStep + baselineShare * lineStep * rowShare);
    }
}
