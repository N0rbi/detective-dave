using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public sealed class ChordPalette
    {
        static readonly IReadOnlyList<int> NoClips = new int[0];

        readonly string[] chordNames;
        readonly int[][] allowedBySegment;
        readonly bool[][] chordToneBySegment;
        readonly PitchClassSet[] scaleBySegment;
        readonly PitchClassSet[] clipPitches;

        public ChordPalette(IReadOnlyList<ChordSegment> segments, IReadOnlyList<PitchClassSet> clipPitches)
        {
            ClipCount = clipPitches.Count;
            this.clipPitches = clipPitches.ToArray();
            scaleBySegment = segments.Select(segment => segment.ScaleTones).ToArray();
            chordNames = segments.Select(segment => segment.ChordName).ToArray();
            allowedBySegment = segments.Select(segment => AllowedClips(segment, clipPitches)).ToArray();
            chordToneBySegment = segments.Select(segment => ChordToneFlags(segment, clipPitches)).ToArray();
        }

        public int ClipCount { get; }

        public IReadOnlyList<int> Allowed(int segmentIndex) => IsKnown(segmentIndex) ? allowedBySegment[segmentIndex] : NoClips;

        public bool IsChordTone(int segmentIndex, int clipIndex) => IsKnown(segmentIndex) && chordToneBySegment[segmentIndex][clipIndex];

        public int Closest(int segmentIndex, int excludedClip)
        {
            var best = PianoPicker.NoClip;
            var bestScore = -1;
            for (var clip = 0; clip < ClipCount; clip++)
            {
                if (clip == excludedClip && ClipCount > 1) continue;
                var score = clipPitches[clip].CountIn(scaleBySegment[segmentIndex]);
                if (score <= bestScore) continue;
                best = clip;
                bestScore = score;
            }
            return best;
        }

        public string ChordName(int segmentIndex) => IsKnown(segmentIndex) ? chordNames[segmentIndex] : string.Empty;

        bool IsKnown(int segmentIndex) => segmentIndex >= 0 && segmentIndex < allowedBySegment.Length;

        static int[] AllowedClips(ChordSegment segment, IReadOnlyList<PitchClassSet> clipPitches) =>
            Enumerable.Range(0, clipPitches.Count).Where(clip => !clipPitches[clip].IsEmpty && segment.ScaleTones.ContainsAll(clipPitches[clip])).ToArray();

        static bool[] ChordToneFlags(ChordSegment segment, IReadOnlyList<PitchClassSet> clipPitches) =>
            clipPitches.Select(pitches => !pitches.IsEmpty && segment.ChordTones.ContainsAll(pitches)).ToArray();
    }
}
