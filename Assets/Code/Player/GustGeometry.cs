using System;
using System.Collections.Generic;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public static class GustGeometry
    {
        const float MinTravel = 0.0001f;

        public static GustSegment[] Build(Vector2 from, Vector2 to, IReadOnlyList<GustLine> lines, float playerHeight)
        {
            var travel = to - from;
            var distance = travel.magnitude;
            if (distance < MinTravel || lines == null) return Array.Empty<GustSegment>();
            var direction = travel / distance;
            var segments = new GustSegment[lines.Count];
            for (int index = 0; index < segments.Length; index++) segments[index] = SegmentOf(lines[index], from, direction, distance, playerHeight);
            return segments;
        }

        public static GustSegment Shrunk(GustSegment segment, float progress)
        {
            var start = Vector2.Lerp(segment.Start, segment.End, Mathf.Clamp01(progress));
            return new GustSegment(start, segment.End, segment.Thickness);
        }

        static GustSegment SegmentOf(GustLine line, Vector2 from, Vector2 direction, float distance, float playerHeight)
        {
            var across = new Vector2(-direction.y, direction.x);
            var startDistance = distance * line.StartShare;
            var length = Mathf.Min(distance * line.LengthShare, distance - startDistance);
            var start = from + direction * startDistance + across * (line.OffsetAcrossInHeights * playerHeight);
            return new GustSegment(start, start + direction * length, line.ThicknessInHeights * playerHeight);
        }
    }
}
