using UnityEngine;

namespace DetectiveDave.DogPlay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DogView : MonoBehaviour, IDogView
    {
        public const int SortingOrder = 8;

        SpriteRenderer sprite;
        DogSettings settings;
        float keyScale;

        public static DogView Create(Transform parent, DogSettings settings, float keyScale)
        {
            var view = new GameObject("Dog").AddComponent<DogView>();
            view.transform.SetParent(parent, false);
            view.Configure(settings, keyScale);
            return view;
        }

        void Configure(DogSettings dogSettings, float scale)
        {
            settings = dogSettings;
            keyScale = scale;
            sprite = GetComponent<SpriteRenderer>();
            sprite.sortingOrder = SortingOrder;
            transform.localScale = Vector3.one * (settings.HeightInKeys * keyScale / settings.CalmFrames[0].bounds.size.y);
        }

        public void Show(Vector2 world, DogMood mood, int step)
        {
            var frames = mood == DogMood.Angry ? settings.AngryFrames : settings.CalmFrames;
            sprite.sprite = frames[DogFrames.PingPongIndex(step, frames.Length)];
            transform.position = new Vector3(world.x, world.y + settings.OffsetYInKeys * keyScale, transform.position.z);
        }

        public void Release() => Destroy(gameObject);
    }
}
