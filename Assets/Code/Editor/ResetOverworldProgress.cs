using DetectiveDave.OverworldPlay;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    public static class ResetOverworldProgress
    {
        const string Title = "Reset Overworld Progress";

        [MenuItem("Window/Detective Dave/Reset Overworld Progress")]
        static void Reset()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(Title, "Stop Play Mode first.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog(Title, "Mark every case as unsolved?", "Reset", "Cancel")) return;
            JsonProgressStore.AtDefaultPath().Reset();
            EditorUtility.DisplayDialog(Title, "All cases are unsolved again.", "OK");
        }
    }
}
