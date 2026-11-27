using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class StrikeVariance
    {
        const int NoClipYet = -1;

        readonly IRandomSource random;
        readonly int clipCount;
        readonly float pitchMin;
        readonly float pitchMax;
        readonly float volumeShareMin;
        readonly float volumeShareMax;
        int lastClip = NoClipYet;

        public StrikeVariance(IRandomSource random, int clipCount, float pitchMin, float pitchMax, float volumeShareMin, float volumeShareMax)
        {
            this.random = random;
            this.clipCount = Math.Max(clipCount, 0);
            this.pitchMin = Math.Min(pitchMin, pitchMax);
            this.pitchMax = Math.Max(pitchMin, pitchMax);
            this.volumeShareMin = Math.Min(volumeShareMin, volumeShareMax);
            this.volumeShareMax = Math.Max(volumeShareMin, volumeShareMax);
        }

        public Strike Next()
        {
            lastClip = NextClip();
            return new Strike(lastClip, random.Range(pitchMin, pitchMax), random.Range(volumeShareMin, volumeShareMax));
        }

        int NextClip()
        {
            if (clipCount <= 1) return 0;
            if (lastClip == NoClipYet) return random.Index(clipCount);
            int pick = random.Index(clipCount - 1);
            return pick >= lastClip ? pick + 1 : pick;
        }
    }
}
