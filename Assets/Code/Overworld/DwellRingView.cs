using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DwellRingView : MonoBehaviour, IDwellIndicator
    {
        const int StepsPerTurn = 48;
        const int SortingOrder = 20;
        const string TrackName = "Ring Track";
        const string FillName = "Ring Fill";

        Transform followed;
        float radiusWorld;
        LineRenderer track;
        LineRenderer fill;
        Material material;
        float progress;
        bool shown;

        public void Configure(Transform followedTransform, float radiusInWorld, float widthInWorld, Color trackColor, Color fillColor)
        {
            followed = followedTransform;
            radiusWorld = radiusInWorld;
            material = SpriteShaderMaterial.Create();
            track = CreateLine(TrackName, widthInWorld, trackColor);
            fill = CreateLine(FillName, widthInWorld, fillColor);
            Hide();
        }

        public void Show(float value)
        {
            shown = true;
            progress = value;
            Draw();
        }

        public void Hide()
        {
            shown = false;
            track.enabled = false;
            fill.enabled = false;
        }

        void LateUpdate()
        {
            if (shown) Draw();
        }

        void OnDestroy()
        {
            if (material) Destroy(material);
        }

        void Draw()
        {
            Vector2 centre = followed.position;
            Apply(track, RingGeometry.ArcPoints(centre, radiusWorld, 1f, StepsPerTurn));
            Apply(fill, RingGeometry.ArcPoints(centre, radiusWorld, progress, StepsPerTurn));
        }

        static void Apply(LineRenderer line, Vector3[] points)
        {
            line.enabled = points.Length > 1;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }

        LineRenderer CreateLine(string lineName, float width, Color color)
        {
            var lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(transform, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            line.numCapVertices = 4;
            line.sortingOrder = SortingOrder;
            return line;
        }
    }
}
