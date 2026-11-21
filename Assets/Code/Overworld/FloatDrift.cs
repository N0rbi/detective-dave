using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    public readonly struct FloatDrift
    {
        const float FullTurn = Mathf.PI * 2f;
        const float SidewaysSpeedRatio = 0.7f;
        const float SidewaysAmplitudeShare = 0.5f;
        const float SwaySpeedRatio = 0.45f;

        readonly float phase;
        readonly float sidewaysPhase;
        readonly float swayPhase;
        readonly float turnsPerSecond;
        readonly float amplitudeInKeys;
        readonly float swayDegrees;

        public FloatDrift(float phase, float sidewaysPhase, float swayPhase, float secondsPerCycle, float amplitudeInKeys, float swayDegrees)
        {
            this.phase = phase;
            this.sidewaysPhase = sidewaysPhase;
            this.swayPhase = swayPhase;
            turnsPerSecond = 1f / Mathf.Max(secondsPerCycle, 0.01f);
            this.amplitudeInKeys = amplitudeInKeys;
            this.swayDegrees = swayDegrees;
        }

        public static FloatDrift Roll(CaseFloatSettings settings, IRandomSource random)
        {
            return new FloatDrift(
                random.Range(0f, FullTurn),
                random.Range(0f, FullTurn),
                random.Range(0f, FullTurn),
                random.Range(settings.SecondsPerCycleMin, settings.SecondsPerCycleMax),
                random.Range(settings.AmplitudeMinInKeys, settings.AmplitudeMaxInKeys),
                random.Range(0f, settings.SwayDegreesMax));
        }

        public Vector2 OffsetInKeys(float seconds)
        {
            float angle = seconds * turnsPerSecond * FullTurn;
            return new Vector2(Mathf.Sin(angle * SidewaysSpeedRatio + sidewaysPhase) * amplitudeInKeys * SidewaysAmplitudeShare, Mathf.Sin(angle + phase) * amplitudeInKeys);
        }

        public float SwayDegreesAt(float seconds) => Mathf.Sin(seconds * turnsPerSecond * FullTurn * SwaySpeedRatio + swayPhase) * swayDegrees;
    }
}
