using System;
using DG.Tweening;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class EvidenceView : MonoBehaviour, IEvidencePresenter
    {
        const int SortingOrder = PlayerView.SortingOrder + 2;

        PlayerMovementSettings settings;
        float keyScale;
        SpriteRenderer picture;
        Sequence presentation;
        Tween emphasis;

        public void Configure(PlayerMovementSettings movementSettings, float scaleOfKeys)
        {
            settings = movementSettings;
            keyScale = scaleOfKeys;
            if (!settings.GoalIcon) return;
            var part = new GameObject("Evidence");
            part.transform.SetParent(transform, false);
            picture = part.AddComponent<SpriteRenderer>();
            picture.sprite = settings.GoalIcon;
            picture.sortingOrder = SortingOrder;
            part.SetActive(false);
        }

        public void Present(Vector2 hand, Action done)
        {
            if (!picture)
            {
                done?.Invoke();
                return;
            }
            Stop();
            var part = picture.transform;
            part.position = new Vector3(hand.x, hand.y, part.position.z);
            part.localScale = Vector3.zero;
            part.gameObject.SetActive(true);
            presentation = BuildPresentation(part);
            presentation.OnComplete(() => FinishPresenting(done));
        }

        public void Emphasise()
        {
            if (!picture || !picture.gameObject.activeSelf) return;
            emphasis?.Kill();
            emphasis = picture.transform.DOScale(HeldScale() * settings.EvidenceZoomGrowth, settings.CameraZoomSeconds)
                .SetEase(Ease.InOutCubic)
                .SetLink(picture.gameObject);
        }

        public void Hide()
        {
            Stop();
            if (picture) picture.gameObject.SetActive(false);
        }

        void OnDisable() => Hide();

        void OnDestroy() => Stop();

        Sequence BuildPresentation(Transform part)
        {
            return DOTween.Sequence()
                .SetLink(picture.gameObject)
                .Append(part.DOScale(HeldScale(), settings.EvidencePopSeconds).SetEase(Ease.OutBack))
                .AppendInterval(settings.EvidenceHoldSeconds);
        }

        void FinishPresenting(Action done)
        {
            presentation = null;
            done?.Invoke();
        }

        void Stop()
        {
            presentation?.Kill();
            presentation = null;
            emphasis?.Kill();
            emphasis = null;
        }

        float HeldScale() => DecorationScale.For(settings.EvidenceSizeInKeys, keyScale, picture.sprite.bounds.size.x);
    }
}
