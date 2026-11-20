using System.Collections.Generic;

namespace DetectiveDave.OverworldPlay
{
    public interface IProgressStore
    {
        IReadOnlyCollection<string> LoadSolved();

        void SaveSolved(IReadOnlyCollection<string> solvedIds);
    }
}
