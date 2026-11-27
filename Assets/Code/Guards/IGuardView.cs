using UnityEngine;

namespace DetectiveDave.GuardPlay
{
    public interface IGuardView
    {
        void Show(Vector2 world, bool chasing, int frame, bool facesRight);

        int FrameCount(bool chasing);

        void Release();
    }
}
