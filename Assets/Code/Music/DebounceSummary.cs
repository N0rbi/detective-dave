using System.Globalization;

namespace DetectiveDave.MusicPlay
{
    public static class DebounceSummary
    {
        public static string For(float debounceSeconds) => debounceSeconds <= 0f
            ? "Debounce is off: every note is a chord."
            : string.Format(CultureInfo.InvariantCulture, "First note after a pause plays a chord; moves within {0:0.##} s play single notes.", debounceSeconds);
    }
}
