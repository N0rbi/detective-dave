using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public interface IDashGust
    {
        void Show(Vector2 from, Vector2 to);

        void Clear();
    }
}
