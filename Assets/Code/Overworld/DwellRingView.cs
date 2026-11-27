using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DwellRingView : MonoBehaviour, IDwellIndicator
    {
        const int StepsPerTurn = 48;
        const int TrackSortingOrder = 19;
        const int FillSortingOrder = 20;
        const string TrackName = "Ring Track";
        const string FillName = "Ring Fill";

        Transform followed;
        float radiusWorld;
        SpriteRenderer track;
        LineRenderer fill;
        Material material;
        float progress;
        bool shown;

        public void Configure(Transform followedTransform, Sprite ringSprite, float radiusInWorld, float widthInWorld, Color trackColor, Color fillColor)
        {
            followed = followedTransform;
            radiusWorld = radiusInWorld;
            material = SpriteShaderMaterial.Create();
            track = CreateTrack(ringSprite, trackColor);
            fill = CreateFill(widthInWorld, fillColor);
            Hide();
        }

        public void Show(float value)
        {
            shown = true;
            progress = value;
            track.enabled = true;
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
            track.transform.position = new Vector3(centre.x, centre.y, 0f);
            Apply(fill, RingGeometry.ArcPoints(centre, radiusWorld, progress, StepsPerTurn));
        }

        static void Apply(LineRenderer line, Vector3[] points)
        {
            line.enabled = points.Length > 1;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }

        SpriteRenderer CreateTrack(Sprite ringSprite, Color color)
        {
            var trackObject = new GameObject(TrackName);
            trackObject.transform.SetParent(transform, false);
            var renderer = trackObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ringSprite;
            renderer.color = color;
            renderer.sortingOrder = TrackSortingOrder;
            float scale = 2f * radiusWorld / ringSprite.bounds.size.x;
            trackObject.transform.localScale = new Vector3(scale, scale, 1f) / Mathf.Max(transform.lossyScale.x, 0.0001f);
            return renderer;
        }

        LineRenderer CreateFill(float width, Color color)
        {
            var lineObject = new GameObject(FillName);
            lineObject.transform.SetParent(transform, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            line.numCapVertices = 4;
            line.sortingOrder = FillSortingOrder;
            return line;
        }
    }
}
