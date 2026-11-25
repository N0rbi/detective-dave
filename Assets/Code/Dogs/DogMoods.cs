using DetectiveDave.KeyboardGrid;
using UnityEngine.InputSystem;

namespace DetectiveDave.DogPlay
{
    public static class DogMoods
    {
        public static DogMood Of(IKeyboardBoard board, Key dog, Key player)
        {
            if (player == dog) return DogMood.Calm;
            return board.IsNeighbour(dog, player) ? DogMood.Angry : DogMood.Calm;
        }
    }
}
