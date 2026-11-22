using System;
using System.Collections.Generic;
using System.Linq;
using DetectiveDave.OverworldPlay;

namespace DetectiveDave.MusicPlay
{
    public sealed class ChordAwarePicker : IPianoPicker
    {
        readonly IRandomSource random;
        readonly ChordPalette palette;
        readonly float chordToneWeight;
        int last = PianoPicker.NoClip;

        public ChordAwarePicker(IRandomSource random, ChordPalette palette, float chordToneWeight)
        {
            this.random = random;
            this.palette = palette;
            this.chordToneWeight = Math.Max(chordToneWeight, 1f);
        }

        public int Next(ChordMoment moment)
        {
            var candidates = CandidatesAvoidingLast(palette.Allowed(moment.SegmentIndex));
            if (candidates.Count == 0) return PianoPicker.NoClip;
            var weights = candidates.Select(clip => WeightOf(moment, clip)).ToList();
            return last = candidates[PickByWeight(weights)];
        }

        IReadOnlyList<int> CandidatesAvoidingLast(IReadOnlyList<int> allowed)
        {
            var others = allowed.Where(clip => clip != last).ToList();
            return others.Count > 0 ? others : allowed.ToList();
        }

        float WeightOf(ChordMoment moment, int clip)
        {
            if (!palette.IsChordTone(moment.SegmentIndex, clip)) return 1f;
            return moment.FavoursChordTones ? chordToneWeight : 1f + (chordToneWeight - 1f) / 2f;
        }

        int PickByWeight(IReadOnlyList<float> weights)
        {
            var target = random.Range(0f, weights.Sum());
            var cumulative = 0f;
            for (var index = 0; index < weights.Count; index++)
            {
                cumulative += weights[index];
                if (target < cumulative) return index;
            }
            return weights.Count - 1;
        }
    }
}
