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
        readonly float misclickShare;

        public PianoNoteVolume(IRandomSource random, float baseVolume, float variance, float blockedShare) : this(random, baseVolume, variance, blockedShare, blockedShare)
        {
        }

        public PianoNoteVolume(IRandomSource random, float baseVolume, float variance, float blockedShare, float misclickShare)
        {
            this.misclickShare = misclickShare;
            this.random = random;
            this.baseVolume = baseVolume;
            this.variance = Mathf.Max(variance, 0f);
            this.blockedShare = blockedShare;
        }

        public float For(MoveAttempt attempt)
        {
            var jitter = 1f + random.Range(-variance, variance);
            var share = ShareOf(attempt);
            return Mathf.Clamp01(baseVolume * jitter * share);
        }

        float ShareOf(MoveAttempt attempt)
        {
            if (attempt == MoveAttempt.Blocked) return blockedShare;
            if (attempt == MoveAttempt.WrongKey) return misclickShare;
            return 1f;
        }
    }
}
