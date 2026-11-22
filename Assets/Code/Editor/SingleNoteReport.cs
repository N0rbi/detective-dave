using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.EditorTools
{
    sealed class SingleNoteReport
    {
        const string NothingYet = "No single note clips yet: export \"Piano Single - <Note><Octave>.wav\" files into Assets/Staging and run Set Up Music again. Until then every note is a chord.";

        public static readonly SingleNoteReport Empty = new SingleNoteReport(0, new string[0], new string[0]);

        readonly int found;
        readonly IReadOnlyList<string> moved;
        readonly IReadOnlyList<string> problems;

        public SingleNoteReport(int found, IReadOnlyList<string> moved, IReadOnlyList<string> problems)
        {
            this.found = found;
            this.moved = moved;
            this.problems = problems;
        }

        public string Text => found == 0 && moved.Count == 0 && problems.Count == 0 ? NothingYet : string.Join("\n", Lines());

        IEnumerable<string> Lines()
        {
            yield return $"Single note clips on Music Settings: {found}.";
            if (moved.Count > 0) yield return "Moved from Assets/Staging: " + string.Join(", ", moved) + ".";
            foreach (var problem in problems) yield return "Not used: " + problem;
        }
    }
}
