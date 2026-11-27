namespace DetectiveDave.DogPlay
{
    public static class DogFrames
    {
        public static int PingPongIndex(int step, int frameCount)
        {
            if (frameCount <= 1) return 0;
            var period = 2 * (frameCount - 1);
            var position = step % period;
            return position < frameCount ? position : period - position;
        }

        public static int StepAt(float seconds, float framesPerSecond) => (int)(seconds * framesPerSecond);
    }
}
