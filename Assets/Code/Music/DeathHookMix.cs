namespace DetectiveDave.MusicPlay
{
    public readonly struct DeathHookMix
    {
        public static readonly DeathHookMix Neutral = new DeathHookMix(0f, 1f, 1f);

        public readonly float HookVolume;
        public readonly float LoopShare;
        public readonly float CaseMusicShare;

        public DeathHookMix(float hookVolume, float loopShare, float caseMusicShare)
        {
            HookVolume = hookVolume;
            LoopShare = loopShare;
            CaseMusicShare = caseMusicShare;
        }
    }
}
