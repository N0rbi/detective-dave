using System;
using System.Collections.Generic;

namespace DetectiveDave.Diagnostics
{
    public sealed class AudioEventLog : IAudioEvents
    {
        readonly AudioEvent[] ring;
        readonly Func<int> frameNow;
        int next;
        int count;

        public AudioEventLog(int capacity, Func<int> frameNow)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity), "The log needs room for at least one event.");
            ring = new AudioEvent[capacity];
            this.frameNow = frameNow;
        }

        public void Record(AudioEventKind kind, string clipName, float seconds)
        {
            ring[next] = new AudioEvent(frameNow(), kind, clipName, seconds);
            next = (next + 1) % ring.Length;
            count = Math.Min(count + 1, ring.Length);
        }

        public IReadOnlyList<AudioEvent> Between(int firstFrame, int lastFrame)
        {
            var found = new List<AudioEvent>();
            for (var age = count; age >= 1; age--)
            {
                var audioEvent = ring[(next - age + ring.Length) % ring.Length];
                if (audioEvent.Frame >= firstFrame && audioEvent.Frame <= lastFrame) found.Add(audioEvent);
            }
            return found;
        }
    }
}
