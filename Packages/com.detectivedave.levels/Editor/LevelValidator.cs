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
                issues.AddRange(StyleIssues(keyArt, style, null));
                if (style.Tint.a <= 0f)
                    issues.Add(new LevelIssue(MessageType.Warning, $"'{style.Type.name}' has a fully transparent Tint: its keys are invisible.", keyArt));
            }

            foreach (var type in LevelAssets.FindAllTileTypes().Where(type => keyArt.FindStyle(type) == null))
                issues.Add(new LevelIssue(MessageType.Info, $"'{type.name}' has no style here: keys of that kind are drawn plain. Press 'Add Styles For All Tile Types'.", keyArt));
            return issues.OrderByDescending(issue => (int)issue.Severity).ToList();
        }

        /// <summary>The labels of the standard key shapes (whole-key Enter excluded) that nothing in <paramref name="keyArt"/> draws: no Box and no shape override.</summary>
        internal static List<string> MissingStandardShapes(KeyArt keyArt, TileType type) =>
            KeyShapePresets.Standard
                .Where(shape => !shape.WholeKey && keyArt.Resolve(type, shape.Size, false).Kind == KeySpriteKind.None)
                .Select(shape => shape.Label)
                .ToList();

        /// <summary>
        /// What is missing or wrong in the art of one style: no Box, a Box that is not 9-sliced, no Enter, shapes without a sprite on the old path.
        /// <paramref name="openKeyArt"/> adds an "Open Key Art" button to the issues that are fixed in the Key Art inspector.
        /// </summary>
        internal static List<LevelIssue> StyleIssues(KeyArt keyArt, TileStyle style, Action openKeyArt)
        {
            var issues = new List<LevelIssue>();
            var type = style.Type;
            string open = openKeyArt == null ? null : "Open Key Art";
            if (!style.Box)
            {
                var missing = MissingStandardShapes(keyArt, type);
                if (!style.HasShapeSprites)
                    issues.Add(new LevelIssue(MessageType.Warning, $"'{type.name}' has no Box sprite in '{keyArt.name}': its keys are drawn as flat squares. Give the style a 9-sliced Box and an Enter.", keyArt, open, openKeyArt));
                else if (missing.Count > 0)
                    issues.Add(new LevelIssue(MessageType.Warning, $"'{type.name}' has no sprite for: {string.Join(", ", missing)}. Those keys are drawn as flat squares. A Box sprite covers every size.", keyArt, open, openKeyArt));
                if (style.CanMigrateToBox)
                    issues.Add(new LevelIssue(MessageType.Info, $"'{type.name}' still draws keys from one sprite per size. It works as before. To use the two-sprite design, give it a Box and an Enter.", keyArt, open, openKeyArt));
                return issues;
            }

            var problem = BoxSpriteCheck.ProblemOf(style.Box);
            if (problem != BoxSpriteProblem.None)
                issues.Add(BoxProblemIssue(keyArt, style, problem, open, openKeyArt));
            if (!style.Enter)
                issues.Add(new LevelIssue(MessageType.Info, $"'{type.name}' has no Enter sprite: the L-shaped Enter is drawn with the Box, one piece per rectangle.", keyArt, open, openKeyArt));
            return issues;
        }

        static LevelIssue BoxProblemIssue(KeyArt keyArt, TileStyle style, BoxSpriteProblem problem, string open, Action openKeyArt)
        {
            var message = $"The Box '{style.Box.name}' of '{style.Type.name}': {BoxSpriteCheck.Describe(problem)}. Its keys are drawn stretched, not 9-sliced.";
            if (!BoxSpriteCheck.CanSetBorder(style.Box, out _))
                return new LevelIssue(MessageType.Warning, message, keyArt, open, openKeyArt);
            var sprite = style.Box;
            int pixels = BoxSpriteCheck.SuggestedBorderPixels((int)sprite.rect.width, (int)sprite.rect.height);
            return new LevelIssue(MessageType.Warning, message, keyArt, $"Set {pixels} px border", () => BoxSpriteCheck.TrySetBorder(sprite, pixels, out _));
        }
    }
}
