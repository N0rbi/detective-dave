using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public interface IEndDecorations
    {
        void ShowStart(Vector2 world);

        void ShowGoal(Vector2 world);

        void ShowBurst(Vector2 world);

        void HideAll();
    }
}
