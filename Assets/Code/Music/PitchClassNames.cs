using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public static class PitchClassNames
    {
        static readonly string[] Names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string Of(IEnumerable<int> pitchClasses) =>
            pitchClasses == null ? string.Empty : string.Join(" ", pitchClasses.Select(NameOf));

        static string NameOf(int pitchClass) => Names[((pitchClass % Names.Length) + Names.Length) % Names.Length];
    }
}
