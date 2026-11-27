using UnityEngine;

namespace DetectiveDave.GuardPlay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GuardView : MonoBehaviour, IGuardView
    {
        public const int SortingOrder = 9;

        SpriteRenderer sprite;
        GuardSettings settings;

        public static GuardView Create(Transform parent, GuardSettings settings, float keyScale)
        {
            var view = new GameObject("Guard").AddComponent<GuardView>();
            view.transform.SetParent(parent, false);
            view.Configure(settings, keyScale);
            return view;
        }

        void Configure(GuardSettings guardSettings, float keyScale)
        {
            settings = guardSettings;
            sprite = GetComponent<SpriteRenderer>();
            sprite.sortingOrder = SortingOrder;
            transform.localScale = Vector3.one * (settings.HeightInKeys * keyScale / settings.WalkFrames[0].bounds.size.y);
        }

        public int FrameCount(bool chasing) => (chasing ? settings.ChaseFrames : settings.WalkFrames).Length;

        public void Show(Vector2 world, bool chasing, int frame, bool facesRight)
        {
            var frames = chasing ? settings.ChaseFrames : settings.WalkFrames;
            sprite.sprite = frames[Mathf.Clamp(frame, 0, frames.Length - 1)];
            sprite.flipX = settings.ArtFacesRight ? !facesRight : facesRight;
            transform.position = new Vector3(world.x, world.y, transform.position.z);
        }

        public void Release() => Destroy(gameObject);
    }
}
