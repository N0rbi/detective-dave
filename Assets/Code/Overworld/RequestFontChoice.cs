using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class RequestFontChoice
    {
        public static Font For(Font caseOverride, Font settingsFont) => caseOverride ? caseOverride : settingsFont;
    }
}
