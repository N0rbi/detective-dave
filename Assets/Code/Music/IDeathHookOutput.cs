namespace DetectiveDave.MusicPlay
{
    public interface IDeathHookOutput
    {
        bool HasClip { get; }

        bool HookIsPlaying { get; }

        void StartHook(bool loop);

        void StopHook();

        void Apply(DeathHookMix mix);
    }
}
