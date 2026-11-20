using System;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public interface IEvidencePresenter
    {
        void Present(Vector2 hand, Action done);

        void Emphasise();

        void Hide();
    }
}
