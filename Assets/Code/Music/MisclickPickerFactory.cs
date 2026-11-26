using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public static class MisclickPickerFactory
    {
        public static MisclickPicker Create(MusicSettings settings)
        {
            var chart = settings.ChordChart;
            if (!settings.DissonantMisclick || !chart || chart.Segments.Count == 0) return null;
            var chordClips = NaturalNameOrder.Sorted(settings.PianoClips.Where(clip => clip), clip => clip.name);
            var candidates = new List<MisclickCandidate>();
            for (var index = 0; index < chordClips.Count; index++)
            {
                var entry = chart.ClipPitches.FirstOrDefault(pitch => pitch.Clip == chordClips[index]);
                candidates.Add(new MisclickCandidate(NoteKind.Chord, index, chordClips[index].name, entry == null ? default : entry.PitchClasses));
            }
            var singles = SingleNoteOrder.Sorted(settings.SingleNoteClips);
            for (var index = 0; index < singles.Count; index++)
                candidates.Add(new MisclickCandidate(NoteKind.Single, index, singles[index].Clip.name, PitchClassSet.Of(new[] { singles[index].PitchClass })));
            return new MisclickPicker(chart.Segments, candidates, settings.DissonanceWeights, settings.NormalizeByNoteCount);
        }
    }
}

namespace DetectiveDave.MusicPlay
{
    public sealed class SilentMisclickPicker : IMisclickPicker
    {
        public PickedNote Next(ChordMoment moment) => new PickedNote(NoteKind.Chord, PianoPicker.NoClip);
    }
}
