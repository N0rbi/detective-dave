using System.Collections.Generic;
using System.Linq;
using DetectiveDave.MusicPlay;

namespace DetectiveDave.EditorTools
{
    public static class ChordChartSummary
    {
        public static string SegmentLine(ChordSegment segment) =>
            $"{segment.ChordName}: chord {PitchClassNames.Of(segment.ChordToneNumbers)}, scale {PitchClassNames.Of(segment.ScaleToneNumbers)}";

        public static string ClipLine(PianoClipPitch pitch, IReadOnlyList<ChordSegment> segments)
        {
            var chords = segments
                .Where(segment => !pitch.PitchClasses.IsEmpty && segment.ScaleTones.ContainsAll(pitch.PitchClasses))
                .Select(segment => segment.ChordName)
                .Distinct()
                .ToList();
            var name = pitch.Clip ? pitch.Clip.name : "(no clip)";
            var playsOver = chords.Count == 0 ? "no chord" : string.Join(", ", chords);
            return $"{name}: {PitchClassNames.Of(pitch.PitchClassNumbers)}, plays over {playsOver}";
        }
    }
}
