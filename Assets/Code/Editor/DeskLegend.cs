using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.EditorTools
{
    public readonly struct DeskLegendEntry
    {
        public string Label { get; }
        public Color Colour { get; }

        public DeskLegendEntry(string label, Color colour)
        {
            Label = label;
            Colour = colour;
        }
    }

    public static class DeskLegend
    {
        public const string CaseSampleId = "case";

        public static IReadOnlyList<DeskLegendEntry> Entries() => new[]
        {
            new DeskLegendEntry("Free key: a case file can go here", DeskCaseColors.FreeKeyFill),
            new DeskLegendEntry("Key covered by a case (its own colour)", DeskCaseColors.Outline(CaseSampleId)),
            new DeskLegendEntry("Key covered by two cases: move one", DeskCaseColors.SharedKey),
            new DeskLegendEntry("START: keep free", DeskCaseColors.StartKey),
        };
    }
}
