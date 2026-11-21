using System;
using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEditor;

namespace DetectiveDave.EditorTools
{
    static class DeskPreview
    {
        const string ModeKey = "DetectiveDave.DeskPreview.Mode";

        public static readonly string[] ModeNames = { "Open", "Locked", "Solved", "By progress file" };

        public static DeskPreviewMode Mode
        {
            get => (DeskPreviewMode)SessionState.GetInt(ModeKey, (int)DeskPreviewMode.Open);
            set => SessionState.SetInt(ModeKey, (int)value);
        }

        public static Func<CaseFile, CaseState> StateFunction()
        {
            var mode = Mode;
            var progress = mode == DeskPreviewMode.ByProgressFile ? new CaseProgress(JsonProgressStore.AtDefaultPath()) : null;
            return file => DeskPreviewState.Of(mode, file.Id, file.RequiredIds, progress);
        }
    }
}
