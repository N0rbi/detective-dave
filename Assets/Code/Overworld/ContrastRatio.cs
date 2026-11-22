using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class ContrastRatio
    {
        const float DarkSegmentLimit = 0.03928f;
        const float DarkSegmentDivisor = 12.92f;
        const float BrightOffset = 0.055f;
        const float BrightDivisor = 1.055f;
        const float BrightExponent = 2.4f;
        const float RedWeight = 0.2126f;
        const float GreenWeight = 0.7152f;
        const float BlueWeight = 0.0722f;
        const float GlareAllowance = 0.05f;

        public static float Of(Color first, Color second)
        {
            float firstLuminance = Luminance(first);
            float secondLuminance = Luminance(second);
            float lighter = Math.Max(firstLuminance, secondLuminance);
            float darker = Math.Min(firstLuminance, secondLuminance);
            return (lighter + GlareAllowance) / (darker + GlareAllowance);
        }

        public static bool IsReadable(Color text, Color background, float minimumRatio) => Of(text, background) >= minimumRatio;

        static float Luminance(Color color) => RedWeight * Linear(color.r) + GreenWeight * Linear(color.g) + BlueWeight * Linear(color.b);

        static float Linear(float channel) => channel <= DarkSegmentLimit ? channel / DarkSegmentDivisor : (float)Math.Pow((channel + BrightOffset) / BrightDivisor, BrightExponent);
    }
}
