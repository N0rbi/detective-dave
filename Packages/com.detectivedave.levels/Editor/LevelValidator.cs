using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>Runs the level and key-art checks. The inspectors cache the results and re-run them only when something changed.</summary>
    public static class LevelValidator
    {
        /// <summary>
        /// Every <see cref="ILevelCheck"/> on <paramref name="level"/>, most severe first. A check that throws is logged and becomes one error issue.
        /// Reads every cell of the Shape tilemap and the project's tile types: not for every frame.
        /// </summary>
        public static List<LevelIssue> Validate(Level level)
        {
            var issues = new List<LevelIssue>();
            if (!level) return issues;
            foreach (var checkType in TypeCache.GetTypesDerivedFrom<ILevelCheck>())
            {
                if (checkType.IsAbstract || checkType.IsInterface || checkType.GetConstructor(Type.EmptyTypes) == null) continue;
                try
                {
                    ((ILevelCheck)Activator.CreateInstance(checkType)).Check(level, issues);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, level);
                    issues.Add(new LevelIssue(MessageType.Error, $"The check {checkType.Name} failed ({exception.Message}). See the Console.", level));
                }
            }
            return issues.OrderByDescending(issue => (int)issue.Severity).ToList();
        }

        /// <summary>The checks of a Key Art asset: styles without a type, duplicate styles, missing sprites for the standard key shapes, invisible tints.</summary>
        public static List<LevelIssue> Validate(KeyArt keyArt)
        {
            var issues = new List<LevelIssue>();
            if (!keyArt) return issues;
            var styles = keyArt.TileStyles;
            for (int i = 0; i < styles.Count; i++)
                if (styles[i] == null || !styles[i].Type)
                    issues.Add(new LevelIssue(MessageType.Error, $"Style {i + 1} has no Tile Type. Pick one, or remove the style.", keyArt));

            foreach (var duplicate in styles.Where(style => style != null && style.Type).GroupBy(style => style.Type).Where(group => group.Count() > 1))
                issues.Add(new LevelIssue(MessageType.Warning, $"'{duplicate.Key.name}' has {duplicate.Count()} styles. Only the first one is used: merge them.", keyArt));

            foreach (var style in styles.Where(style => style != null && style.Type))
            {
                var missing = MissingStandardShapes(keyArt, style.Type);
                if (missing.Count > 0)
                    issues.Add(new LevelIssue(MessageType.Warning, $"'{style.Type.name}' has no sprite for: {string.Join(", ", missing)}. Those keys are drawn as flat squares.", keyArt));
                if (style.Tint.a <= 0f)
                    issues.Add(new LevelIssue(MessageType.Warning, $"'{style.Type.name}' has a fully transparent Tint: its keys are invisible.", keyArt));
            }

            foreach (var type in LevelAssets.FindAllTileTypes().Where(type => keyArt.FindStyle(type) == null))
                issues.Add(new LevelIssue(MessageType.Info, $"'{type.name}' has no style here: keys of that kind are drawn plain. Press 'Add Styles For All Tile Types'.", keyArt));
            return issues.OrderByDescending(issue => (int)issue.Severity).ToList();
        }

        /// <summary>The labels of the standard key shapes (whole-key Enter excluded) that <paramref name="keyArt"/> has no sprite for.</summary>
        internal static List<string> MissingStandardShapes(KeyArt keyArt, TileType type) =>
            KeyShapePresets.Standard
                .Where(shape => !shape.WholeKey && !keyArt.FindSprite(type, shape.Size))
                .Select(shape => shape.Label)
                .ToList();
    }
}
