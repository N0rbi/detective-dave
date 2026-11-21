namespace DetectiveDave.OverworldPlay
{
    public readonly struct Strike
    {
        public readonly int ClipIndex;
        public readonly float Pitch;
        public readonly float VolumeShare;

        public Strike(int clipIndex, float pitch, float volumeShare)
        {
            ClipIndex = clipIndex;
            Pitch = pitch;
            VolumeShare = volumeShare;
        }
    }
}
