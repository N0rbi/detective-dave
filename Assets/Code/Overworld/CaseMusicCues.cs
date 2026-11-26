using System;
using DetectiveDave.MusicPlay;

namespace DetectiveDave.OverworldPlay
{
    public sealed class CaseMusicCues : ICaseMusicCues
    {
        readonly Func<string, CaseMusic> musicOf;
        readonly Func<ICaseMusicPlayer> player;

        public CaseMusicCues(Func<string, CaseMusic> musicOf, Func<ICaseMusicPlayer> player)
        {
            this.musicOf = musicOf;
            this.player = player;
        }

        public void CaseOpened(string caseId)
        {
            var music = musicOf(caseId);
            if (music == null || !music.HasClip)
            {
                CaseEnded();
                return;
            }
            player().FadeIn(music);
        }

        public void CaseEnded() => player().FadeOut();
    }
}
