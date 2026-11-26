using System.Collections.Generic;
using System.Linq;
using DetectiveDave.MusicPlay;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    static class MisclickTable
    {
        public readonly struct Row
        {
            public Row(string chord, string clip, float score, int tiedCount)
            {
                Chord = chord;
                Clip = clip;
                Score = score;
                TiedCount = tiedCount;
            }

            public string Chord { get; }

            public string Clip { get; }

            public float Score { get; }

            public int TiedCount { get; }
        }

        public static IReadOnlyList<Row> For(MusicSettings settings)
        {
            var picker = MisclickPickerFactory.Create(settings);
            if (picker == null) return new Row[0];
            var rows = new List<Row>();
            var seen = new HashSet<string>();
            foreach (var segment in settings.ChordChart.Segments)
            {
                if (!seen.Add(segment.ChordName)) continue;
                var tied = picker.MostDissonant(segment, out var score);
                if (tied.Count == 0) continue;
                rows.Add(new Row(segment.ChordName, picker.Candidates[tied[0]].Name, score, tied.Count));
            }
            return rows;
        }
    }
}
