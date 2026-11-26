using System.Collections.Generic;

namespace DetectiveDave.MusicPlay
{
    public sealed class MisclickPicker : IMisclickPicker
    {
        const float TieTolerance = 0.0001f;

        readonly IReadOnlyList<ChordSegment> segments;
        readonly IReadOnlyList<MisclickCandidate> candidates;
        readonly DissonanceWeights weights;
        readonly bool normalize;
        int last = -1;

        public MisclickPicker(IReadOnlyList<ChordSegment> segments, IReadOnlyList<MisclickCandidate> candidates, DissonanceWeights weights, bool normalize)
        {
            this.segments = segments;
            this.candidates = candidates;
            this.weights = weights;
            this.normalize = normalize;
        }

        public IReadOnlyList<MisclickCandidate> Candidates => candidates;

        public PickedNote Next(ChordMoment moment)
        {
            var silent = new PickedNote(NoteKind.Chord, PianoPicker.NoClip);
            if (!moment.IsInsideChart || moment.SegmentIndex >= segments.Count) return silent;
            var tied = MostDissonant(segments[moment.SegmentIndex], out _);
            if (tied.Count == 0) return silent;
            last = RotateAfterLast(tied);
            var chosen = candidates[last];
            return new PickedNote(chosen.Kind, chosen.ClipIndex);
        }

        public IReadOnlyList<int> MostDissonant(ChordSegment segment, out float score)
        {
            var best = new List<int>();
            score = 0f;
            if (segment.ChordTones.IsEmpty) return best;
            for (var index = 0; index < candidates.Count; index++)
            {
                if (candidates[index].Pitches.IsEmpty) continue;
                var value = DissonanceScorer.Score(candidates[index].Pitches, segment, weights, normalize);
                if (best.Count == 0 || value > score + TieTolerance)
                {
                    best.Clear();
                    best.Add(index);
                    score = value;
                }
                else if (value >= score - TieTolerance) best.Add(index);
            }
            return best;
        }

        int RotateAfterLast(IReadOnlyList<int> tied)
        {
            foreach (var index in tied)
                if (index > last) return index;
            return tied[0];
        }
    }
}
