using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public readonly struct PaperSpot
    {
        public Vector2 Centre { get; }
        public float Width { get; }
        public float RotationDegrees { get; }

        public PaperSpot(Vector2 centre, float width, float rotationDegrees)
        {
            Centre = centre;
            Width = width;
            RotationDegrees = rotationDegrees;
        }
    }
}
