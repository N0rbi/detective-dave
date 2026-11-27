using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class LineScales
    {
        readonly float[] shares;

        LineScales(float[] lineShares)
        {
            shares = lineShares;
        }

        public static LineScales Uniform { get; } = new LineScales(Array.Empty<float>());

        public static LineScales Report(string wrappedDescription, string wrappedFindings, float findingsShare)
        {
            int descriptionLines = TextFit.LineCount(wrappedDescription);
            int findingsLines = TextFit.LineCount(wrappedFindings);
            int dividerLines = descriptionLines > 0 && findingsLines > 0 ? 1 : 0;
            var lineShares = new float[descriptionLines + dividerLines + findingsLines];
            Array.Fill(lineShares, 1f, 0, descriptionLines + dividerLines);
            Array.Fill(lineShares, findingsShare, descriptionLines + dividerLines, findingsLines);
            return new LineScales(lineShares);
        }

        public float ShareOf(int lineIndex) => lineIndex >= 0 && lineIndex < shares.Length ? shares[lineIndex] : 1f;

        public float OffsetOf(int lineIndex)
        {
            float offset = 0f;
            for (int index = 0; index < lineIndex; index++) offset += ShareOf(index);
            return offset;
        }
    }
}
