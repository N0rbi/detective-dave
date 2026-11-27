using System.Globalization;

namespace DetectiveDave.Diagnostics
{
    public static class AudioEventText
    {
        public static string Of(AudioEvent audioEvent)
        {
            var clip = $"'{audioEvent.ClipName}'";
            var seconds = audioEvent.Seconds.ToString("0.000", CultureInfo.InvariantCulture);
            switch (audioEvent.Kind)
            {
                case AudioEventKind.PianoNote: return $"piano note {clip} at volume {seconds}";
                case AudioEventKind.ClipLoadedOnPlay: return $"clip {clip} was not loaded when it had to play (decoded on the main thread)";
                case AudioEventKind.LoopSeek: return $"seek of {clip} to {seconds} s";
                default: return $"stem start {clip} at {seconds} s";
            }
        }
    }
}
