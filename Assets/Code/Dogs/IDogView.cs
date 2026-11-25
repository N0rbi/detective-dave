using UnityEngine;

namespace DetectiveDave.DogPlay
{
    public interface IDogView
    {
        void Show(Vector2 world, DogMood mood, int step);

        void Release();
    }
}
