using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Editor
{
    /// <summary>Small drawing helpers shared by the level inspectors.</summary>
    internal static class LevelEditorGUI
    {
        const float MaxPreviewHeight = 160f;
        static readonly Color PreviewBackground = new Color(0.12f, 0.12f, 0.12f);
        static readonly Color MissingColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);

        public static void DrawIssues(IReadOnlyList<LevelIssue> issues)
        {
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("All checks passed.", MessageType.None);
                return;
            }
            foreach (var issue in issues.OrderByDescending(issue => (int)issue.Severity))
                EditorGUILayout.HelpBox(issue.Message, issue.Severity);
        }

        /// <summary>Draws a sprite into a rect (atlas-aware), or a red box when it is missing.</summary>
        public static void DrawSprite(Rect rect, Sprite sprite, Color tint)
        {
            if (!sprite || !sprite.texture)
            {
                EditorGUI.DrawRect(rect, MissingColor);
                return;
            }
            var texture = sprite.texture;
            var source = sprite.textureRect;
            var coords = new Rect(source.x / texture.width, source.y / texture.height, source.width / texture.width, source.height / texture.height);
            var previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(FitAspect(rect, source.size), texture, coords);
            GUI.color = previous;
        }

        /// <summary>A map of the baked terrain: one coloured square per cell (the type's editor colour), y up, plus a legend.</summary>
        public static void DrawTerrainPreview(LevelTerrain terrain)
        {
            if (terrain == null || terrain.IsEmpty) return;
            var bounds = terrain.CellBounds;
            float width = EditorGUIUtility.currentViewWidth - 40f;
            float cell = Mathf.Max(1f, Mathf.Min(width / bounds.width, MaxPreviewHeight / bounds.height));
            var area = GUILayoutUtility.GetRect(bounds.width * cell, bounds.height * cell, GUILayout.ExpandWidth(false));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(area, PreviewBackground);
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                    for (int x = bounds.xMin; x < bounds.xMax; x++)
                    {
                        var type = terrain.GetTile(new Vector2Int(x, y)).Type;
                        if (!type) continue;
                        float left = area.x + (x - bounds.xMin) * cell;
                        float top = area.y + (bounds.yMax - 1 - y) * cell;
                        EditorGUI.DrawRect(new Rect(left, top, cell, cell), type.EditorColor);
                    }
            }
            foreach (var type in terrain.Types.Where(type => type))
            {
                var line = EditorGUILayout.GetControlRect();
                EditorGUI.DrawRect(new Rect(line.x, line.y + 3f, 10f, 10f), type.EditorColor);
                EditorGUI.LabelField(new Rect(line.x + 16f, line.y, line.width - 16f, line.height), type.name);
            }
        }

        public static void Header(string text)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        static Rect FitAspect(Rect rect, Vector2 size)
        {
            if (size.x <= 0f || size.y <= 0f) return rect;
            float scale = Mathf.Min(rect.width / size.x, rect.height / size.y);
            var fitted = size * scale;
            return new Rect(rect.center - fitted * 0.5f, fitted);
        }
    }
}
