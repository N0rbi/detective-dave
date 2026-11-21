namespace DetectiveDave.OverworldPlay
{
    public sealed class UnityRandomSource : IRandomSource
    {
        public float Range(float min, float max) => UnityEngine.Random.Range(min, max);

        public int Index(int count) => UnityEngine.Random.Range(0, count);
    }
}
