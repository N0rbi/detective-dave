namespace DetectiveDave.GuardPlay
{
    public static class GuardFrames
    {
        public static int At(float progress, int frameCount)
        {
            if (frameCount <= 1) return 0;
            int cycleLength = 2 * (frameCount - 1);
            int position = (int)(System.Math.Clamp(progress, 0f, 0.9999f) * cycleLength);
            return position < frameCount ? position : cycleLength - position;
        }
    }
}
