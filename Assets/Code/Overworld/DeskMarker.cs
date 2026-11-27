using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public readonly struct DeskMarker
    {
        const float EdgeTolerance = 0.0001f;

        public string CaseId { get; }
        public Vector2 Centre { get; }
        public Vector2 Size { get; }
        public float RotationDegrees { get; }

        public DeskMarker(string caseId, Vector2 centre, Vector2 size, float rotationDegrees)
        {
            CaseId = caseId;
            Centre = centre;
            Size = size;
            RotationDegrees = rotationDegrees;
        }

        public bool Contains(Vector2 point)
        {
            var local = Quaternion.Euler(0f, 0f, -RotationDegrees) * (Vector3)(point - Centre);
            return Mathf.Abs(local.x) <= Size.x * 0.5f + EdgeTolerance && Mathf.Abs(local.y) <= Size.y * 0.5f + EdgeTolerance;
        }
    }
}
