using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public static class SingleNoteOrder
    {
        public static IReadOnlyList<SingleNoteClip> Sorted(IEnumerable<SingleNoteClip> singles) =>
            singles
                .Where(single => single != null && single.Clip)
                .OrderBy(single => single.MidiPitch)
                .ThenBy(single => single.Clip.name, StringComparer.Ordinal)
                .ToList();
    }
}
