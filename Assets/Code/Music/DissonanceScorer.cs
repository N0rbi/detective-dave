namespace DetectiveDave.MusicPlay
{
    public static class DissonanceScorer
    {
        const int PitchClassesPerOctave = 12;

        public static float Score(PitchClassSet clip, ChordSegment segment, DissonanceWeights weights, bool normalizeByNoteCount)
        {
            if (clip.IsEmpty) return 0f;
            var chord = segment.ChordTones;
            var scale = segment.ScaleTones;
            var total = 0f;
            foreach (var note in clip.Notes) total += NoteScore(note, chord, scale, weights);
            return normalizeByNoteCount ? total / clip.Count : total;
        }

        public static float NoteScore(int note, PitchClassSet chord, PitchClassSet scale, DissonanceWeights weights)
        {
            if (chord.Contains(note)) return 0f;
            if (scale.Contains(note)) return weights.ScaleNonChordTone;
            return ClashWeight(note, chord, weights);
        }

        static float ClashWeight(int note, PitchClassSet chord, DissonanceWeights weights)
        {
            var nearest = PitchClassesPerOctave;
            var weight = weights.Other;
            foreach (var tone in chord.Notes)
            {
                var above = ((note - tone) % PitchClassesPerOctave + PitchClassesPerOctave) % PitchClassesPerOctave;
                var distance = System.Math.Min(above, PitchClassesPerOctave - above);
                var candidate = weights.ForInterval(above);
                if (distance < nearest || (distance == nearest && candidate > weight))
                {
                    nearest = distance;
                    weight = candidate;
                }
            }
            return weight;
        }
    }
}
