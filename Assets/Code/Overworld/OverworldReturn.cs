using DetectiveDave.PlayerPlay;

namespace DetectiveDave.OverworldPlay
{
    public sealed class OverworldReturn : ILevelAdvance
    {
        public void Advance()
        {
            CaseSession.MarkLevelSolved();
            SceneLoader.Load(GameScene.Overworld);
        }
    }
}
