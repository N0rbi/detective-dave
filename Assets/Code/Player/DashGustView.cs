using System.Collections.Generic;
using DetectiveDave.KeyboardGrid;
using DG.Tweening;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class DashGustView : MonoBehaviour, IDashGust
    {
        const int SortingOrder = PlayerView.SortingOrder - 1;
        const float ArrivalToleranceSquared = 0.000001f;
        const float FollowTimeoutSeconds = 2f;

        readonly List<SpriteRenderer> lines = new List<SpriteRenderer>();
        readonly List<GustSegment> segments = new List<GustSegment>();
        PlayerMovementSettings settings;
        Transform player;
        float keyScale;
        Sprite square;
        Tween fade;
        bool following;
        bool climbing;
        Vector2 start;
        Vector2 limit;
        float followStartedAt;

        public void Configure(PlayerMovementSettings movementSettings, float scaleOfKeys, Transform followed)
        {
            settings = movementSettings;
            keyScale = scaleOfKeys;
            player = followed;
            square = SpriteFactory.CreateUnitSquare();
        }

        public void Show(Vector2 from, Vector2 to, bool climbingPose)
        {
            Clear();
            if (!settings.DashGustEnabled) return;
            climbing = climbingPose;
            start = from;
            limit = to;
            followStartedAt = Time.time;
            following = true;
        }

        public void Clear()
        {
            fade?.Kill();
            fade = null;
            following = false;
            foreach (var line in lines) Destroy(line.gameObject);
            lines.Clear();
            segments.Clear();
        }

        void LateUpdate()
        {
            if (!following) return;
            segments.Clear();
            segments.AddRange(GustGeometry.Follow(start, limit, player.position, settings.GustLines, PlayerHeightInWorld, BandLift));
            MatchLineCount();
            Apply(0f);
            if (HasArrived || Time.time - followStartedAt > FollowTimeoutSeconds) FadeOut();
        }

        void OnDisable() => Clear();

        void OnDestroy()
        {
            Clear();
            if (!square) return;
            Destroy(square.texture);
            Destroy(square);
        }

        bool HasArrived => ((Vector2)player.position - limit).sqrMagnitude < ArrivalToleranceSquared;

        Vector2 BandLift => GustGeometry.Lift(settings.GustBandCenterInHeights, settings.ClimbDropInHeights, climbing, PlayerHeightInWorld);

        float PlayerHeightInWorld => settings.HeightInKeys * keyScale * transform.lossyScale.x;

        void FadeOut()
        {
            following = false;
            fade = DOVirtual.Float(0f, 1f, settings.GustFadeSeconds, Apply)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnComplete(Clear);
        }

        void MatchLineCount()
        {
            while (lines.Count < segments.Count) lines.Add(CreateLine());
            while (lines.Count > segments.Count)
            {
                Destroy(lines[lines.Count - 1].gameObject);
                lines.RemoveAt(lines.Count - 1);
            }
        }

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
                var segment = GustGeometry.Shrunk(segments[index], progress);
                lines[index].enabled = segment.Length > 0f;
                Place(lines[index].transform, segment);
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
