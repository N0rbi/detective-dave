using System;

namespace DetectiveDave.OverworldPlay
{
    public sealed class DetectiveDrop
    {
        public DetectiveDrop(float restY, float distance)
        {
            if (distance < 0f) throw new ArgumentOutOfRangeException(nameof(distance), distance, "The drop distance cannot be negative");
            RestY = restY;
            LowY = restY - distance;
        }

        public float RestY { get; }
        public float LowY { get; }
        public DropPhase Phase { get; private set; } = DropPhase.AtRest;

        public float BeginDrop()
        {
            Phase = DropPhase.Dropping;
            return LowY;
        }

        public bool TryStartReturn(out float targetY)
        {
            targetY = RestY;
            if (Phase != DropPhase.Dropping) return false;
            Phase = DropPhase.Returning;
            return true;
        }

        public void FinishReturn()
        {
            if (Phase == DropPhase.Returning) Phase = DropPhase.AtRest;
        }

        public void ResetToRest() => Phase = DropPhase.AtRest;
    }
}
