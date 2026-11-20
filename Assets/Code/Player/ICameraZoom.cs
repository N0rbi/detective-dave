using System;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public interface ICameraZoom
    {
        void ZoomIn(Vector2 focus, Action done);

        void SnapZoomed(Vector2 focus);

        void ZoomOut(Action done);

        void Restore();

        ScreenRect CurrentBounds();

        ScreenRect ReachableBounds();
    }
}
