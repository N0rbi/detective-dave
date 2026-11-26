using System.Collections.Generic;

namespace DetectiveDave.EditorTools
{
    public static class SetupReport
    {
        public static string Summary(IReadOnlyList<string> changes, string unchangedText) =>
            changes.Count == 0 ? "Nothing needed changing. " + unchangedText : "Set up:\n- " + string.Join("\n- ", changes);
    }
}
