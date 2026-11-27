using System.Collections.Generic;
using UnityEditor;

namespace DetectiveDave.Levels.Editor
{
    internal sealed class LevelBackgroundCheck : ILevelCheck
    {
        public void Check(Level level, List<LevelIssue> issues)
        {
            var renderer = level.BackgroundRenderer;
            if (!renderer || !renderer.sprite) return;
            if (!BackgroundNames.IsPlaceholder(renderer.sprite.name)) return;
            var art = level.KeyArt;
            if (!art || !art.DefaultBackground) return;
            issues.Add(new LevelIssue(MessageType.Warning,
                $"The level's Background renderer holds the placeholder '{renderer.sprite.name}', which hides the Key Art's background '{art.DefaultBackground.name}'. Clear it to see the Key Art's background.",
                level, "Use the Key Art's background", () => ClearSprite(level)));
        }

        static void ClearSprite(Level level)
        {
            var serialized = new SerializedObject(level.BackgroundRenderer);
            serialized.FindProperty("m_Sprite").objectReferenceValue = null;
            serialized.ApplyModifiedProperties();
        }
    }

    internal static class BackgroundNames
    {
        const string PlaceholderPrefix = "placeholder_";

        public static bool IsPlaceholder(string spriteName) =>
            !string.IsNullOrEmpty(spriteName) && spriteName.StartsWith(PlaceholderPrefix, System.StringComparison.OrdinalIgnoreCase);
    }
}
