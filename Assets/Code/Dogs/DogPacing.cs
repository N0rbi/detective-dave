using UnityEngine;

namespace DetectiveDave.DogPlay
{
    public readonly struct DogPacing
    {
        public DogPacing(float skipChance, int calmEveryNotes)
        {
            SkipChance = Mathf.Clamp01(skipChance);
            CalmEveryNotes = Mathf.Max(1, calmEveryNotes);
        }

        public float SkipChance { get; }

        public int CalmEveryNotes { get; }
    }
}
