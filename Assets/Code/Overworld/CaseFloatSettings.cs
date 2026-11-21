namespace DetectiveDave.OverworldPlay
{
    public readonly struct CaseFloatSettings
    {
        public bool Enabled { get; }
        public float AmplitudeMinInKeys { get; }
        public float AmplitudeMaxInKeys { get; }
        public float SecondsPerCycleMin { get; }
        public float SecondsPerCycleMax { get; }
        public float SwayDegreesMax { get; }

        public CaseFloatSettings(bool enabled, float amplitudeMinInKeys, float amplitudeMaxInKeys, float secondsPerCycleMin, float secondsPerCycleMax, float swayDegreesMax)
        {
            Enabled = enabled;
            AmplitudeMinInKeys = amplitudeMinInKeys;
            AmplitudeMaxInKeys = amplitudeMaxInKeys;
            SecondsPerCycleMin = secondsPerCycleMin;
            SecondsPerCycleMax = secondsPerCycleMax;
            SwayDegreesMax = swayDegreesMax;
        }
    }
}
