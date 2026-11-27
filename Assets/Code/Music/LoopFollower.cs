using System;

namespace DetectiveDave.MusicPlay
{
    public static class LoopFollower
    {
        public static double WantedClipTime(double loopTime, double loopLength, double clipLength) =>
            loopTime % Math.Min(loopLength, clipLength);

        public static double WantedClipTime(double loopTime, double loopLength, double clipLength, double offsetSeconds)
        {
            var cycle = Math.Min(loopLength, clipLength);
            var wanted = (loopTime + offsetSeconds) % cycle;
            return wanted < 0.0 ? wanted + cycle : wanted;
        }

        public static bool NeedsCorrection(double clipTime, double wantedClipTime, double cycleLength, double toleranceSeconds)
        {
            var difference = Math.Abs(clipTime - wantedClipTime);
            return Math.Min(difference, cycleLength - difference) > toleranceSeconds;
        }
    }
}
