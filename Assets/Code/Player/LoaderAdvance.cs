using DetectiveDave.LevelPlay;

namespace DetectiveDave.PlayerPlay
{
    public sealed class LoaderAdvance : ILevelAdvance
    {
        readonly LevelLoader loader;

        public LoaderAdvance(LevelLoader loader) => this.loader = loader;

        public void Advance() => loader.Advance();
    }
}
