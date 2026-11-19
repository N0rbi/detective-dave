using DetectiveDave.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
            Gizmos.matrix = matrix;
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
