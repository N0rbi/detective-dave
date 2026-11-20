using DG.Tweening;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class EndDecorationsView : MonoBehaviour, IEndDecorations
    {
        const int IconSortingOrder = 1;
        const int BurstSortingOrder = PlayerView.SortingOrder - 1;

        PlayerMovementSettings settings;
        float keyScale;
        SpriteRenderer startGrate;
        SpriteRenderer goalIcon;
        SpriteRenderer burst;
        Tween growth;
        Tween spin;

        public void Configure(PlayerMovementSettings movementSettings, float scaleOfKeys)
        {
            settings = movementSettings;
            keyScale = scaleOfKeys;
            startGrate = CreateRenderer("Start Grate", settings.StartGrate, settings.StartGrateSizeInKeys, IconSortingOrder);
            goalIcon = CreateRenderer("Goal Icon", settings.GoalIcon, settings.GoalIconSizeInKeys, IconSortingOrder);
            burst = CreateRenderer("Goal Burst", settings.GoalBurst, settings.GoalBurstSizeInKeys, BurstSortingOrder);
        }

        public void ShowStart(Vector2 world) => Place(startGrate, world);

        public void ShowGoal(Vector2 world) => Place(goalIcon, world);

        public void ShowBurst(Vector2 world)
        {
            if (!burst) return;
            Place(burst, world);
            StartBurstMotion();
        }

        public void HideAll()
        {
            StopBurstMotion();
            Hide(startGrate);
            Hide(goalIcon);
            Hide(burst);
        }

        void OnDisable() => HideAll();

        void OnDestroy() => StopBurstMotion();

        SpriteRenderer CreateRenderer(string rendererName, Sprite sprite, float sizeInKeys, int sortingOrder)
        {
            if (!sprite) return null;
            var part = new GameObject(rendererName);
            part.transform.SetParent(transform, false);
            part.transform.localScale = Vector3.one * DecorationScale.For(sizeInKeys, keyScale, sprite.bounds.size.x);
            var renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            part.SetActive(false);
            return renderer;
        }

        static void Place(SpriteRenderer target, Vector2 world)
        {
            if (!target) return;
            var part = target.transform;
            part.position = new Vector3(world.x, world.y, part.position.z);
            target.gameObject.SetActive(true);
        }

        static void Hide(SpriteRenderer target)
        {
            if (target) target.gameObject.SetActive(false);
        }

        void StartBurstMotion()
        {
            StopBurstMotion();
            var target = burst.transform;
            var fullScale = Vector3.one * DecorationScale.For(settings.GoalBurstSizeInKeys, keyScale, burst.sprite.bounds.size.x);
            target.localScale = Vector3.zero;
            target.rotation = Quaternion.identity;
            growth = target.DOScale(fullScale, settings.GoalBurstGrowSeconds).SetEase(Ease.OutBack).SetLink(burst.gameObject);
            spin = target.DORotate(new Vector3(0f, 0f, -360f), DecorationScale.SecondsPerTurn(settings.GoalBurstDegreesPerSecond), RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental)
                .SetLink(burst.gameObject);
        }

        void StopBurstMotion()
        {
            growth?.Kill();
            spin?.Kill();
            growth = null;
            spin = null;
        }
    }
}
