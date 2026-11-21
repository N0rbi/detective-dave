using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class RouteHintView : MonoBehaviour
    {
        const int DashSortingOrder = 6;
        const int RoundCapVertices = 2;

        KeyboardGridView grid;
        Material material;
        float dashLength;
        float gapLength;
        float width;
        float flowSpeed;
        Color color;
        float flowOffset;
        IReadOnlyList<Key> keys;
        readonly List<LineRenderer> lines = new List<LineRenderer>();
        readonly List<Vector2> points = new List<Vector2>();

        public void Configure(KeyboardGridView gridView, OverworldSettings settings, float keyWorldSize)
        {
            grid = gridView;
            material = SpriteShaderMaterial.Create();
            dashLength = settings.RouteDashLengthInKeys * keyWorldSize;
            gapLength = settings.RouteGapLengthInKeys * keyWorldSize;
            width = settings.RouteWidthInKeys * keyWorldSize;
            flowSpeed = settings.RouteFlowKeysPerSecond * keyWorldSize;
            color = settings.RouteColor;
        }

        public void Show(IReadOnlyList<Key> routeKeys) => keys = routeKeys;

        public void Hide() => keys = null;

        void LateUpdate()
        {
            flowOffset += flowSpeed * Time.deltaTime;
            Draw(keys == null ? new List<Vector2[]>() : DashGeometry.Dashes(WorldPoints(keys), dashLength, gapLength, flowOffset));
        }

        void OnDestroy()
        {
            if (material) Destroy(material);
        }

        List<Vector2> WorldPoints(IReadOnlyList<Key> route)
        {
            points.Clear();
            foreach (var key in route)
            {
                if (grid.TryGetWorldCenter(key, out var centre)) points.Add(centre);
            }
            return points;
        }

        void Draw(List<Vector2[]> dashes)
        {
            while (lines.Count < dashes.Count) lines.Add(CreateLine());
            for (int index = 0; index < lines.Count; index++)
            {
                bool used = index < dashes.Count;
                lines[index].enabled = used;
                if (used) Apply(lines[index], dashes[index]);
            }
        }

        static void Apply(LineRenderer line, Vector2[] dash)
        {
            line.positionCount = dash.Length;
            for (int index = 0; index < dash.Length; index++) line.SetPosition(index, dash[index]);
        }

        LineRenderer CreateLine()
        {
            var lineObject = new GameObject("Route Dash");
            lineObject.transform.SetParent(transform, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            line.numCapVertices = RoundCapVertices;
            line.sortingOrder = DashSortingOrder;
            return line;
        }
    }
}
