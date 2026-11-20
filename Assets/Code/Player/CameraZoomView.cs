using System;
using DG.Tweening;
using DetectiveDave.KeyboardGrid;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class CameraZoomView : MonoBehaviour, ICameraZoom
    {
        const float FallbackHalfExtent = 20f;
        const float ZoomedProgress = 1f;
        const float HomeProgress = 0f;

        PlayerMovementSettings settings;
        KeyboardGridScreenFit screenFit;
        Tween travel;
        bool homeKnown;
        Vector3 homePosition;
        float homeSize;
        Vector2 zoomFocus;

        public void Configure(PlayerMovementSettings movementSettings, KeyboardGridScreenFit fit)
        {
            settings = movementSettings;
            screenFit = fit;
        }

        public void ZoomIn(Vector2 focus, Action done)
        {
            if (!TryCaptureHome())
            {
                done?.Invoke();
                return;
            }
            zoomFocus = focus;
            Travel(HomeProgress, ZoomedProgress, done);
        }

        public void SnapZoomed(Vector2 focus)
        {
            if (!TryCaptureHome()) return;
            zoomFocus = focus;
            KillTravel();
            Apply(ZoomedProgress);
        }

        public void ZoomOut(Action done)
        {
            if (!homeKnown)
            {
                done?.Invoke();
                return;
            }
            Travel(ZoomedProgress, HomeProgress, () => FinishZoomOut(done));
        }

        public void Restore()
        {
            KillTravel();
            ReturnHome();
        }

        public ScreenRect CurrentBounds()
        {
            var camera = Camera.main;
            if (!camera || !camera.orthographic) return new ScreenRect(Vector2.one * -FallbackHalfExtent, Vector2.one * FallbackHalfExtent);
            return BoundsOf(camera.transform.position, camera.orthographicSize, camera.aspect);
        }

        public ScreenRect ReachableBounds()
        {
            var current = CurrentBounds();
            var camera = Camera.main;
            if (!homeKnown || !camera) return current;
            return current.Union(BoundsOf(homePosition, homeSize, camera.aspect));
        }

        void OnDisable() => Restore();

        void OnDestroy() => Restore();

        static ScreenRect BoundsOf(Vector2 position, float orthographicSize, float aspect)
        {
            var half = new Vector2(orthographicSize * aspect, orthographicSize);
            return new ScreenRect(position - half, position + half);
        }

        bool TryCaptureHome()
        {
            var camera = Camera.main;
            if (!camera || !camera.orthographic) return false;
            if (homeKnown) return true;
            if (screenFit) screenFit.Apply();
            homeKnown = true;
            homePosition = camera.transform.position;
            homeSize = camera.orthographicSize;
            if (screenFit) screenFit.enabled = false;
            return true;
        }

        void Travel(float from, float to, Action done)
        {
            KillTravel();
            travel = DOVirtual.Float(from, to, settings.CameraZoomSeconds, Apply)
                .SetEase(Ease.InOutCubic)
                .SetLink(gameObject)
                .OnComplete(() => FinishTravel(done));
        }

        void FinishTravel(Action done)
        {
            travel = null;
            done?.Invoke();
        }

        void FinishZoomOut(Action done)
        {
            ReturnHome();
            done?.Invoke();
        }

        void Apply(float progress)
        {
            var camera = Camera.main;
            if (!camera) return;
            var target = new Vector3(zoomFocus.x, zoomFocus.y, homePosition.z);
            camera.transform.position = Vector3.Lerp(homePosition, target, progress);
            camera.orthographicSize = Mathf.Lerp(homeSize, homeSize * settings.CameraZoomFactor, progress);
        }

        void ReturnHome()
        {
            if (!homeKnown) return;
            homeKnown = false;
            var camera = Camera.main;
            if (camera)
            {
                camera.transform.position = homePosition;
                camera.orthographicSize = homeSize;
            }
            if (screenFit) screenFit.enabled = true;
        }

        void KillTravel()
        {
            travel?.Kill();
            travel = null;
        }
    }
}
