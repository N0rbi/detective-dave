using System;
using System.Collections.Generic;
using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public sealed class FakeWipe : IScreenWipe
    {
        Action done;

        public List<string> Calls { get; } = new List<string>();

        public int CancelCount { get; private set; }

        public void Out(Vector2 centre, Action finished)
        {
            Calls.Add("Out " + centre);
            done = finished;
        }

        public void In(Vector2 centre, Action finished)
        {
            Calls.Add("In " + centre);
            done = finished;
        }

        public void Cancel() => CancelCount++;

        public void Finish() => done?.Invoke();
    }

    public sealed class FakeZoom : ICameraZoom
    {
        Action zoomInDone;
        Action zoomOutDone;

        public List<string> Calls { get; } = new List<string>();

        public int RestoreCount { get; private set; }

        public void ZoomIn(Vector2 focus, Action done)
        {
            Calls.Add("ZoomIn " + focus);
            zoomInDone = done;
        }

        public void SnapZoomed(Vector2 focus) => Calls.Add("Snap " + focus);

        public void ZoomOut(Action done)
        {
            Calls.Add("ZoomOut");
            zoomOutDone = done;
        }

        public void Restore() => RestoreCount++;

        public ScreenRect CurrentBounds() => new ScreenRect(-Vector2.one, Vector2.one);

        public ScreenRect ReachableBounds() => new ScreenRect(-Vector2.one, Vector2.one);

        public void FinishZoomIn() => zoomInDone?.Invoke();

        public void FinishZoomOut() => zoomOutDone?.Invoke();
    }
}
