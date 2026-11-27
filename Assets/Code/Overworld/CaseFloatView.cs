using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public sealed class CaseFloatView : MonoBehaviour
    {
        FloatDrift drift;
        float keyWorldSize;
        Vector3 restLocalPosition;
        Quaternion restLocalRotation;
        bool configured;

        public void Configure(FloatDrift floatDrift, float keyWidthInWorld)
        {
            drift = floatDrift;
            keyWorldSize = keyWidthInWorld;
            restLocalPosition = transform.localPosition;
            restLocalRotation = transform.localRotation;
            configured = true;
        }

        void LateUpdate()
        {
            if (!configured) return;
            float seconds = Time.time;
            transform.localPosition = restLocalPosition + LocalOffset(drift.OffsetInKeys(seconds) * keyWorldSize);
            transform.localRotation = restLocalRotation * Quaternion.Euler(0f, 0f, drift.SwayDegreesAt(seconds));
        }

        void OnDisable()
        {
            if (!configured) return;
            transform.localPosition = restLocalPosition;
            transform.localRotation = restLocalRotation;
        }

        Vector3 LocalOffset(Vector2 worldOffset)
        {
            var parent = transform.parent;
            return parent ? parent.InverseTransformVector(worldOffset) : (Vector3)worldOffset;
        }
    }
}
