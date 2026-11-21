using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public static class GustGeometry
    {
        const float MinTravel = 0.0001f;

        public static GustSegment[] Follow(Vector2 start, Vector2 limit, Vector2 current, IReadOnlyList<GustLine> lines, float playerHeight, Vector2 lift = default)
        {
            var path = limit - start;
            float reach = path.magnitude;
            if (reach < MinTravel || lines == null) return Array.Empty<GustSegment>();
            var direction = path / reach;
            float travelled = Mathf.Clamp(Vector2.Dot(current - start, direction), 0f, reach);
            if (travelled < MinTravel) return Array.Empty<GustSegment>();
            var segments = new GustSegment[lines.Count];
            for (int index = 0; index < segments.Length; index++) segments[index] = SegmentOf(lines[index], start, direction, travelled, playerHeight, lift);
            return segments;
        }

        public static GustSegment Shrunk(GustSegment segment, float progress)
        {
            var tail = Vector2.Lerp(segment.Start, segment.End, Mathf.Clamp01(progress));
            return new GustSegment(tail, segment.End, segment.Thickness);
        }

        static GustSegment SegmentOf(GustLine line, Vector2 start, Vector2 direction, float travelled, float playerHeight, Vector2 lift)
        {
            var across = new Vector2(-direction.y, direction.x) * (line.OffsetAcrossInHeights * playerHeight);
            var head = start + lift + direction * travelled + across;
            float length = Mathf.Min(travelled * line.LengthShare, travelled);
            return new GustSegment(head - direction * length, head, line.ThicknessInHeights * playerHeight);
        }
    }
}
