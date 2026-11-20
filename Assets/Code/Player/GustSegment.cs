using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public readonly struct GustSegment
    {
        public Vector2 Start { get; }

        public Vector2 End { get; }

        public float Thickness { get; }

        public float Length => (End - Start).magnitude;

        public GustSegment(Vector2 start, Vector2 end, float thickness)
        {
            Start = start;
            End = end;
            Thickness = thickness;
        }
    }
}
