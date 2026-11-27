using System;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public interface IScreenWipe
    {
        void Out(Vector2 centre, Action done);

        void In(Vector2 centre, Action done);

        void Cancel();
    }
}
