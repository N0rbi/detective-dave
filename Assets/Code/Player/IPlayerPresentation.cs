namespace DetectiveDave.PlayerPlay
{
    public interface IPlayerPresentation
    {
        void SetVisible(bool visible);

        void SetFrame(int frame);

        void SetFacing(Facing facing);

        void SetClimbing(bool climbing);

        void SetDeathPose(bool shown);
    }
}
