namespace DetectiveDave.MusicPlay
{
    public interface IDeathHookOutput
    {
        bool HasClip { get; }

        void Apply(DeathHookMix mix);
    }
}
