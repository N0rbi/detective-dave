using System;

namespace DetectiveDave.OverworldPlay
{
    public static class ClickSamples
    {
        const int NoiseSeed = 7;
        const float DecayPower = 6f;

        public static float[] Generate(int sampleRate, float seconds)
        {
            var samples = new float[Math.Max((int)(sampleRate * seconds), 1)];
            var noise = new Random(NoiseSeed);
            for (int index = 0; index < samples.Length; index++)
            {
                float remaining = 1f - (float)index / samples.Length;
                samples[index] = (float)(noise.NextDouble() * 2.0 - 1.0) * (float)Math.Pow(remaining, DecayPower);
            }
            return samples;
        }
    }
}
