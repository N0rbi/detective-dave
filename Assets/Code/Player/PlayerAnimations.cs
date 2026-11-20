using System;
using DG.Tweening;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    [RequireComponent(typeof(PlayerView))]
    public sealed class PlayerAnimations : MonoBehaviour, IPlayerAnimations
    {
        const float MinSeconds = 0.01f;

        PlayerView view;
        Tween active;
        Tween breathing;
        bool missingSettingsLogged;

        void Awake() => view = GetComponent<PlayerView>();

        void OnDisable() => Cancel();

        public void ShowResetAt(Vector2 world)
        {
            Cancel();
            view.ResetVisual(world);
        }

        public void CancelAllWithoutCallbacks() => Cancel();

        public void MoveTo(Vector2 world, Action done)
        {
            if (!HasSettings()) return;
            Run(transform.DOMove(WithCurrentDepth(world), Seconds(view.Settings.StepSeconds)).SetEase(Ease.InOutSine), done);
        }

        public void SlipBy(Vector2 worldOffset, Action done)
        {
            if (!HasSettings()) return;
            var half = Seconds(view.Settings.SlipSeconds * 0.5f);
            var tilt = (worldOffset.x >= 0f ? -1f : 1f) * view.Settings.SlipTiltDegrees;
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(transform.position + (Vector3)worldOffset, half).SetEase(Ease.OutQuad));
            sequence.Join(transform.DORotate(new Vector3(0f, 0f, tilt), half).SetEase(Ease.OutQuad));
            sequence.Append(transform.DORotate(Vector3.zero, half).SetEase(Ease.OutBounce));
            Run(sequence, done);
        }

        public void BumpWall(Vector2 towards, Vector2 home, Action done)
        {
            if (!HasSettings()) return;
            var seconds = view.Settings.WallSquashSeconds;
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(WithCurrentDepth(towards), Seconds(seconds * 0.3f)).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(SquashedScale(towards - home), Seconds(seconds * 0.2f)).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOMove(WithCurrentDepth(home), Seconds(seconds * 0.5f)).SetEase(Ease.InOutSine));
            sequence.Join(transform.DOScale(view.BaseLocalScale, Seconds(seconds * 0.5f)).SetEase(Ease.OutElastic));
            Run(sequence, done);
        }

        public void StartBreathing()
        {
            if (!HasSettings()) return;
            if (breathing != null && breathing.IsActive()) return;
            transform.localScale = view.BaseLocalScale;
            var inhaled = view.BaseLocalScale.y * (1f + view.Settings.BreathingAmount);
            breathing = transform.DOScaleY(inhaled, Seconds(view.Settings.BreathingSeconds * 0.5f))
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        public void StopBreathing() => KillBreathing();

        public void FadeIntoGap(Action done)
        {
            if (!HasSettings()) return;
            var seconds = Seconds(view.Settings.GapFadeSeconds);
            var sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(1f, 0f, seconds, alpha => view.SetAlpha(alpha)));
            sequence.Join(transform.DOScale(view.BaseLocalScale * 0.6f, seconds).SetEase(Ease.InQuad));
            Run(sequence, done);
        }

        public void DropFromTop(Vector2 target, float marginWorld, Action done)
        {
            if (!HasSettings()) return;
            Cancel();
            view.ResetVisual(target);
            var height = GetComponent<Renderer>().bounds.size.y;
            var start = transform.position;
            start.y = TopEdgeY(target.y) + marginWorld + height;
            transform.position = start;
            Run(transform.DOMoveY(target.y, Seconds(view.Settings.DropSeconds)).SetEase(Ease.OutBounce), done);
        }

        static float TopEdgeY(float fallbackY)
        {
            var camera = Camera.main;
            if (!camera || !camera.orthographic) return fallbackY;
            return camera.transform.position.y + camera.orthographicSize;
        }

        Vector3 SquashedScale(Vector2 direction)
        {
            var baseScale = view.BaseLocalScale;
            var squash = view.Settings.SquashScale;
            var bulge = 1f + (1f - squash) * 0.5f;
            var horizontal = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);
            return horizontal
                ? new Vector3(baseScale.x * squash, baseScale.y * bulge, baseScale.z)
                : new Vector3(baseScale.x * bulge, baseScale.y * squash, baseScale.z);
        }

        static float Seconds(float seconds) => Mathf.Max(MinSeconds, seconds);

        bool HasSettings()
        {
            if (view.Settings) return true;
            Cancel();
            if (missingSettingsLogged) return false;
            missingSettingsLogged = true;
            Debug.LogError("PlayerAnimations: PlayerView has no settings applied.", this);
            return false;
        }

        void Run(Tween tween, Action done)
        {
            Cancel();
            active = tween;
            tween.SetLink(gameObject).SetAutoKill(true).OnComplete(() =>
            {
                active = null;
                done?.Invoke();
            });
        }

        void Cancel()
        {
            if (active != null && active.IsActive()) active.Kill(false);
            active = null;
            KillBreathing();
        }

        void KillBreathing()
        {
            if (breathing == null) return;
            if (breathing.IsActive()) breathing.Kill(false);
            breathing = null;
            transform.localScale = view.BaseLocalScale;
        }

        Vector3 WithCurrentDepth(Vector2 world) => new Vector3(world.x, world.y, transform.position.z);
    }
}
