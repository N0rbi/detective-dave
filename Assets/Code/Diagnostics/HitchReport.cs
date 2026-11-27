using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DetectiveDave.Diagnostics
{
    public static class HitchReport
    {
        public static string Format(int frame, float frameMilliseconds, float thresholdMilliseconds, int garbageCollections, IReadOnlyList<AudioEvent> audioEvents)
        {
            var text = new StringBuilder();
            text.Append($"[Hitch] frame {frame} took {Number(frameMilliseconds)} ms (threshold {Number(thresholdMilliseconds)} ms), {garbageCollections} garbage collections since the frame before.");
            if (audioEvents.Count == 0)
            {
                text.Append(" No audio event in this or the frame before: the audio is not the cause of this one.");
                return text.ToString();
            }
            foreach (var audioEvent in audioEvents)
                text.Append($"\n  frame {audioEvent.Frame}: {AudioEventText.Of(audioEvent)}");
            return text.ToString();
        }

        static string Number(float value) => value.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
