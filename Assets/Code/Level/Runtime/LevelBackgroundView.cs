using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// Draws the level's background (the sprite of its Background renderer, else its Key Art's default background) behind everything,
    /// scaled to cover the main camera's view and kept centred on it. Nothing is drawn when the level has no background.
    /// The sprite lives on its own root object, so the keyboard's screen fit does not scale it. Needs an orthographic main camera.
    /// </summary>
    [RequireComponent(typeof(LevelLoader))]
    public sealed class LevelBackgroundView : MonoBehaviour
    {
        [Tooltip("Sorting order of the background. The keyboard's own backdrop is -1, so keep this lower.")]
        [SerializeField] int sortingOrder = -100;

        LevelLoader loader;
        SpriteRenderer background;
        Vector3 fittedCamera;
        Vector2 fittedView;

        void Awake()
        {
            loader = GetComponent<LevelLoader>();
            background = new GameObject("Level Background").AddComponent<SpriteRenderer>();
        }

        void OnEnable() => loader.LevelChanged += Refresh;

        void OnDisable() => loader.LevelChanged -= Refresh;

        void Start() => Refresh();

        // Inspector changes during Play (sorting order) take effect at once.
        void OnValidate()
        {
            if (Application.isPlaying && background) Refresh();
        }

        void OnDestroy()
        {
            if (background) Destroy(background.gameObject);
        }

        void LateUpdate()
        {
            var camera = Camera.main;
            if (camera && (camera.transform.position != fittedCamera || ViewSize(camera) != fittedView)) Fit(camera);
        }

        void Refresh()
        {
            var level = loader.Current;
            var sprite = level ? level.Background : null;
            background.sortingOrder = sortingOrder;
            background.sprite = sprite;
            background.color = level ? level.BackgroundTint : Color.white;
            background.enabled = sprite;
            var camera = Camera.main;
            if (camera) Fit(camera);
        }

        // Cover: scale up until the sprite fills the view in both directions, cropping the overflow.
        void Fit(Camera camera)
        {
            fittedCamera = camera.transform.position;
            fittedView = ViewSize(camera);
            var sprite = background.sprite;
            if (!sprite || !camera.orthographic) return;
            var size = (Vector2)sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f) return;
            float scale = Mathf.Max(fittedView.x / size.x, fittedView.y / size.y);
            background.transform.position = new Vector3(fittedCamera.x, fittedCamera.y, 0f);
            background.transform.localScale = new Vector3(scale, scale, 1f);
        }

        static Vector2 ViewSize(Camera camera) => new Vector2(camera.orthographicSize * 2f * camera.aspect, camera.orthographicSize * 2f);
    }
}
