namespace DetectiveDave.OverworldPlay
{
    public readonly struct LightFade
    {
        public const float DarkIntensity = 0f;

        public LightFade(float fullIntensity, float fadeInSeconds, float fadeOutSeconds)
        {
            FullIntensity = fullIntensity;
            FadeInSeconds = fadeInSeconds;
            FadeOutSeconds = fadeOutSeconds;
        }

        public float FullIntensity { get; }
        public float FadeInSeconds { get; }
        public float FadeOutSeconds { get; }
        public float FadeInFrom => DarkIntensity;
        public float FadeInTo => FullIntensity;
        public float FadeOutTo => DarkIntensity;
    }
}
