using UnityEngine;

namespace DetectiveDave.EditorTools
{
    public static class DeskCaseColors
    {
        const float GoldenRatio = 0.61803398875f;
        const int HashBuckets = 1000;
        const float Saturation = 0.7f;
        const float FillAlpha = 0.35f;

        public static readonly Color SharedKey = new Color(1f, 0.1f, 0.1f, 0.55f);

        public static Color Fill(string caseId)
        {
            var colour = Outline(caseId);
            colour.a = FillAlpha;
            return colour;
        }

        public static Color Outline(string caseId) => Color.HSVToRGB(Mathf.Repeat(Bucket(caseId) * GoldenRatio, 1f), Saturation, 1f);

        static int Bucket(string caseId)
        {
            unchecked
            {
                int hash = 17;
                foreach (char letter in caseId ?? string.Empty) hash = hash * 31 + letter;
                return Mathf.Abs(hash % HashBuckets);
            }
        }
    }
}
