using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DG.Tweening;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class DashGustView : MonoBehaviour, IDashGust
    {
        const int SortingOrder = PlayerView.SortingOrder - 1;

        readonly List<SpriteRenderer> lines = new List<SpriteRenderer>();
        readonly List<GustSegment> segments = new List<GustSegment>();
        PlayerMovementSettings settings;
        float keyScale;
        Sprite square;
        Tween fade;

        public void Configure(PlayerMovementSettings movementSettings, float scaleOfKeys)
        {
            settings = movementSettings;
            keyScale = scaleOfKeys;
            square = SpriteFactory.CreateUnitSquare();
        }

        public void Show(Vector2 from, Vector2 to)
        {
            Clear();
            foreach (var segment in GustGeometry.Build(from, to, settings.GustLines, PlayerHeightInWorld))
            {
                if (segment.Length <= 0f) continue;
                segments.Add(segment);
                lines.Add(CreateLine());
            }
            if (lines.Count == 0) return;
            Apply(0f);
            fade = DOVirtual.Float(0f, 1f, settings.GustFadeSeconds, Apply)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnComplete(Clear);
        }

        public void Clear()
        {
            fade?.Kill();
            fade = null;
            foreach (var line in lines) Destroy(line.gameObject);
            lines.Clear();
            segments.Clear();
        }

        void OnDisable() => Clear();

        void OnDestroy()
        {
            Clear();
            if (!square) return;
            Destroy(square.texture);
            Destroy(square);
        }

        float PlayerHeightInWorld => settings.HeightInKeys * keyScale * transform.lossyScale.x;

        SpriteRenderer CreateLine()
        {
            var line = new GameObject("Gust Line");
            line.transform.SetParent(transform, false);
            var renderer = line.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.sortingOrder = SortingOrder;
            return renderer;
        }

        void Apply(float progress)
        {
            var color = settings.GustColor;
            color.a *= 1f - progress;
            for (int index = 0; index < lines.Count; index++)
            {
                Place(lines[index].transform, GustGeometry.Shrunk(segments[index], progress));
                lines[index].color = color;
            }
        }

        void Place(Transform line, GustSegment segment)
        {
            var middle = (segment.Start + segment.End) * 0.5f;
            var direction = segment.End - segment.Start;
            var worldToLocal = 1f / transform.lossyScale.x;
            line.position = new Vector3(middle.x, middle.y, transform.position.z);
            line.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            line.localScale = new Vector3(segment.Length * worldToLocal, segment.Thickness * worldToLocal, 1f);
        }
    }
}
