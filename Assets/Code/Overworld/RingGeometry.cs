using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public static class RingGeometry
    {
        const float TopAngle = Mathf.PI * 0.5f;

        public static Vector3[] ArcPoints(Vector2 centre, float radius, float progress, int stepsPerTurn)
        {
            float clamped = Mathf.Clamp01(progress);
            if (clamped <= 0f || stepsPerTurn < 3) return new Vector3[0];
            int segments = Math.Max((int)Math.Ceiling(stepsPerTurn * clamped), 1);
            var points = new Vector3[segments + 1];
            for (int index = 0; index <= segments; index++)
            {
                float angle = TopAngle - Mathf.PI * 2f * clamped * index / segments;
                points[index] = new Vector3(centre.x + Mathf.Cos(angle) * radius, centre.y + Mathf.Sin(angle) * radius, 0f);
            }
            return points;
        }
    }
}
