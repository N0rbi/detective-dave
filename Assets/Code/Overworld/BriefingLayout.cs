using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class BriefingLayout
    {
        public static PaperSpot Place(PaperPlacement placement, Vector2 cameraCentre, Vector2 viewSize)
        {
            var offset = new Vector2(placement.CentreXShare * viewSize.x, placement.CentreYShare * viewSize.y);
            return new PaperSpot(cameraCentre + offset, placement.WidthShare * viewSize.x, placement.RotationDegrees);
        }

        public static int LettersPerLine(float paperWidth, float paddingShare, float lineHeight, float advanceRatio)
        {
            float textWidth = paperWidth * (1f - 2f * paddingShare);
            float letterWidth = Mathf.Max(lineHeight * advanceRatio, 0.0001f);
            return Mathf.Max(Mathf.FloorToInt(textWidth / letterWidth), 1);
        }
    }
}
