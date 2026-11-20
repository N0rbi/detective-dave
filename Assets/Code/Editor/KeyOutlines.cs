using System.Linq;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace DetectiveDave.EditorTools
{
    /// <summary>
    /// While a level is open in Prefab Mode, draws every key of the preview layout as an outline, with a dot at the point the game reads
    /// the key's kind (in that kind's editor colour, grey when empty). Editor-only: the level package knows nothing about keyboards.
    /// </summary>
    static class KeyOutlines
    {
        static readonly Color OutlineColor = new Color(1f, 1f, 1f, 0.6f);
        static readonly Color EmptyColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        static readonly Color StartColor = new Color(0.25f, 0.55f, 1f, 1f);
        static readonly Color EndColor = new Color(0.65f, 0.3f, 0.9f, 1f);
        const float DotSize = 0.08f;     // board units at scale 1

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawKeys(Level level, GizmoType gizmoType)
        {
            if (Application.isPlaying || PrefabStageUtility.GetPrefabStage(level.gameObject) == null) return;
            var layout = KeyboardLayouts.Preview;
            if (layout == null) return;
            var shape = level.Shape;
            float scale = KeyGeometry.Scale(layout);

            var matrix = Gizmos.matrix;
            Gizmos.matrix = level.BoardSpace.localToWorldMatrix;
            foreach (var key in layout.UniqueKeys())
            {
                Gizmos.color = OutlineColor;
                foreach (var rect in KeyGeometry.Rects(layout, key)) DrawRect(rect);
                var tile = shape ? shape.GetTile(KeyGeometry.ReadCell(shape, level.BoardSpace, layout, key)) as TileType : null;
                Gizmos.color = tile ? tile.EditorColor : EmptyColor;
                Gizmos.DrawCube(KeyGeometry.Center(layout, key), Vector3.one * (DotSize * scale));
            }
            DrawShiftEnds(level, layout);
            Gizmos.matrix = matrix;
        }

        static void DrawShiftEnds(Level level, KeyboardLayout layout)
        {
            ShiftEnds.Of(level.StartKey, out var start, out var end);
            DrawMarker(layout, start, "START", StartColor);
            DrawMarker(layout, end, "END", EndColor);
        }

        static void DrawMarker(KeyboardLayout layout, Key shift, string text, Color color)
        {
            var key = layout.UniqueKeys().FirstOrDefault(def => def.InputKey == shift);
            if (key == null) return;
            Gizmos.color = color;
            foreach (var rect in KeyGeometry.Rects(layout, key)) Gizmos.DrawWireCube(rect.center, rect.size);
            Handles.Label(Gizmos.matrix.MultiplyPoint3x4(KeyGeometry.Center(layout, key)), text, MarkerStyle(color));
        }

        static GUIStyle MarkerStyle(Color color)
        {
            var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = color;
            return style;
        }

        static void DrawRect(Rect rect)
        {
            Vector3 a = new Vector3(rect.xMin, rect.yMin), b = new Vector3(rect.xMax, rect.yMin);
            Vector3 c = new Vector3(rect.xMax, rect.yMax), d = new Vector3(rect.xMin, rect.yMax);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }
}
