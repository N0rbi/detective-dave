using DG.Tweening;

namespace DetectiveDave.OverworldPlay
{
    public readonly struct DetectiveDropSettings
    {
        public DetectiveDropSettings(float distance, float dropSeconds, float returnSeconds, Ease dropEase, Ease returnEase)
        {
            Distance = distance;
            DropSeconds = dropSeconds;
            ReturnSeconds = returnSeconds;
            DropEase = dropEase;
            ReturnEase = returnEase;
        }

        public float Distance { get; }
        public float DropSeconds { get; }
        public float ReturnSeconds { get; }
        public Ease DropEase { get; }
        public Ease ReturnEase { get; }
    }
}
