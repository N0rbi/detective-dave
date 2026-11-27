namespace DetectiveDave.Diagnostics
{
    public static class HitchDetector
    {
        public static bool IsEnabled(float thresholdMilliseconds) => thresholdMilliseconds > 0f;

        public static bool IsHitch(float frameMilliseconds, float thresholdMilliseconds) =>
            IsEnabled(thresholdMilliseconds) && frameMilliseconds > thresholdMilliseconds;
    }
}
