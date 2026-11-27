using DetectiveDave.OverworldPlay;

namespace DetectiveDave.MusicPlay
{
    public static class CaseMusicCheck
    {
        public const double LongerThanLoopTolerance = 1.0;

        public static string Problem(CaseMusic music, double loopSeconds)
        {
            if (music == null || !music.HasClip) return null;
            if (!music.SyncToLoop || loopSeconds <= 0.0) return null;
            if (music.Clip.length <= loopSeconds + LongerThanLoopTolerance) return null;
            return "The case music is " + music.Clip.length.ToString("0.0") + " s but the jazz loop is " + loopSeconds.ToString("0.0") + " s. Synced, the music restarts with every loop and its end is cut. Trim the clip or untick Sync To Loop.";
        }
    }
}
