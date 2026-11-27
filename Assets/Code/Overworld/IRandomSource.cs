namespace DetectiveDave.OverworldPlay
{
    public interface IRandomSource
    {
        float Range(float min, float max);

        int Index(int count);
    }
}
