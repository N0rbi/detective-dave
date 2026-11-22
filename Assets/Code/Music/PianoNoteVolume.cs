using DetectiveDave.OverworldPlay;
using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    public sealed class PianoNoteVolume
    {
        readonly IRandomSource random;
        readonly float baseVolume;
        readonly float variance;
        readonly float blockedShare;

        public PianoNoteVolume(IRandomSource random, float baseVolume, float variance, float blockedShare)
        {
            this.random = random;
            this.baseVolume = baseVolume;
            this.variance = Mathf.Max(variance, 0f);
            this.blockedShare = blockedShare;
        }

        public float For(MoveAttempt attempt)
        {
            var jitter = 1f + random.Range(-variance, variance);
            var share = attempt == MoveAttempt.Blocked ? blockedShare : 1f;
            return Mathf.Clamp01(baseVolume * jitter * share);
        }
    }
}
