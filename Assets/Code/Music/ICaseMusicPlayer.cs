using DetectiveDave.OverworldPlay;

namespace DetectiveDave.MusicPlay
{
    public interface ICaseMusicPlayer
    {
        void FadeIn(CaseMusic music);

        void FadeOut();

        void StopNow();
    }
}
