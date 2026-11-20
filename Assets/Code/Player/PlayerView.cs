using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerView : MonoBehaviour, IPlayerPresentation
    {
        public const int SortingOrder = 10;

        const string DrawingName = "Drawing";

        SpriteRenderer sprite;
        Transform drawing;
        float spriteHeight;
        Sprite[] walkFrames;
        Sprite[] climbFrames;
        bool climbing;

        public Vector3 BaseLocalScale { get; private set; } = Vector3.one;

        public PlayerMovementSettings Settings { get; private set; }

        void Awake()
        {
            sprite = CreateDrawingRenderer();
        }

        SpriteRenderer CreateDrawingRenderer()
        {
            var root = GetComponent<SpriteRenderer>();
            root.enabled = false;
            drawing = new GameObject(DrawingName).transform;
            drawing.SetParent(transform, false);
            var renderer = drawing.gameObject.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = root.sharedMaterial;
            renderer.sortingLayerID = root.sortingLayerID;
            renderer.sortingOrder = SortingOrder;
            return renderer;
        }

        public void Configure(PlayerMovementSettings settings, float keyScale)
        {
            Settings = settings;
            walkFrames = settings.WalkFrames;
            climbFrames = settings.ClimbFrames;
            spriteHeight = walkFrames[0].bounds.size.y;
            SetClimbing(false);
            SetFrame(0);
            BaseLocalScale = Vector3.one * (settings.HeightInKeys * keyScale / spriteHeight);
            transform.localScale = BaseLocalScale;
        }

        public void ResetVisual(Vector2 world)
        {
            SetPosition(world);
            transform.localScale = BaseLocalScale;
            transform.rotation = Quaternion.identity;
            SetAlpha(1f);
            sprite.enabled = true;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void SetFrame(int frame)
        {
            var frames = climbing ? climbFrames : walkFrames;
            if (frames == null || frame < 0 || frame >= frames.Length)
            {
                Debug.LogError($"PlayerView: frame {frame} is outside the configured frames.", this);
                return;
            }
            sprite.sprite = frames[frame];
        }

        void ApplyClimbDrop() => drawing.localPosition = new Vector3(0f, climbing ? -Settings.ClimbDropInHeights * spriteHeight : 0f, 0f);

        public void SetFacing(Facing facing) => sprite.flipX = Settings.FlipsWithFacing && (Settings.ArtFacesRight ? facing == Facing.Left : facing == Facing.Right);

        public void SetClimbing(bool isClimbing)
        {
            climbing = isClimbing;
            ApplyClimbDrop();
        }

        public void SetAlpha(float alpha)
        {
            var color = sprite.color;
            color.a = alpha;
            sprite.color = color;
        }

        void SetPosition(Vector2 world) => transform.position = new Vector3(world.x, world.y, transform.position.z);
    }
}
