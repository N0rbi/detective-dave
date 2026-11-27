using System;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>
    /// The link between the Level inspector's preview and the lights of the open scenes. The game's editor code sets <see cref="Apply"/>
    /// and <see cref="Restore"/> (this package knows nothing about the render pipeline). Without them the preview only animates the inspector's lamp.
    /// </summary>
    public static class LightingPreviewHost
    {
        public static Action<LevelLightingSettings, float> Apply;
        public static Action Restore;
    }
}
