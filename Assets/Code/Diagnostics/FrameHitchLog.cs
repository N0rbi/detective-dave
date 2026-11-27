using System;
using UnityEngine;

namespace DetectiveDave.Diagnostics
{
    [DefaultExecutionOrder(-32000)]
    public sealed class FrameHitchLog : MonoBehaviour
    {
        const float MillisecondsPerSecond = 1000f;

        float thresholdMilliseconds;
        AudioEventLog audioEvents;
        float lastFrameStart;
        int lastCollections;

        public void Configure(float threshold, AudioEventLog events)
        {
            thresholdMilliseconds = threshold;
            audioEvents = events;
            lastFrameStart = Time.realtimeSinceStartup;
            lastCollections = GC.CollectionCount(0);
        }

        void Update()
        {
            var now = Time.realtimeSinceStartup;
            var frameMilliseconds = (now - lastFrameStart) * MillisecondsPerSecond;
            var collections = GC.CollectionCount(0);
            if (HitchDetector.IsHitch(frameMilliseconds, thresholdMilliseconds))
            {
                var frame = Time.frameCount;
                var events = audioEvents.Between(frame - 1, frame);
                Debug.LogWarning(HitchReport.Format(frame, frameMilliseconds, thresholdMilliseconds, collections - lastCollections, events), this);
            }
            lastFrameStart = now;
            lastCollections = collections;
        }
    }
}
