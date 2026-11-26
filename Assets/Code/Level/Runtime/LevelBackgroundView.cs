using System.IO;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.Levels;
using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// Draws the level's background (the sprite of its Background renderer, else its Key Art's default background) behind everything.
    /// The picture stays where it is in the world and is scaled to cover everything the camera can ever show on this level:
    /// the normal view, the zoom out at the start and the zoom in on the player at the goal, wherever the player stands. It follows window resizes.
    /// The level's Background Placement (offset, zoom, mirrored, hidden) is applied on top. Nothing is drawn when the level has no background.
    /// The sprite lives on its own root object, so the keyboard's screen fit does not scale it. Needs an orthographic main camera.
    /// </summary>
    [RequireComponent(typeof(LevelLoader))]
    public sealed class LevelBackgroundView : MonoBehaviour
    {
        const float DashLength = 0.5f;

        [Tooltip("Sorting order of the background. The keyboard's own backdrop is -1, so keep this lower.")]
        [SerializeField] int sortingOrder = -100;

        [Tooltip("Extra picture beyond everything the camera can show, as a share of that area's size on each side. Keeps the edge out of view.")]
        [SerializeField, Range(0f, 0.5f)] float overscanShare = 0.05f;

        [Tooltip("A warning is logged when the picture has to be drawn larger than this many times its imported size to cover the camera. A larger texture avoids a blurry background.")]
        [SerializeField, Min(1f)] float maxUpscale = 3f;

        [Tooltip("Writes Temp/background-trace.log every frame while the camera is zoomed or zooming: the camera, the view, the planned coverage, the picture's rectangle and whether the keyboard panel shows. A line ends with OUTSIDE when the view shows beyond the picture.")]
        [SerializeField] bool traceBackground;

        const string TracePath = "Temp/background-trace.log";

        LevelLoader loader;
        bool traceStarted;
        KeyboardGridView grid;
        SpriteRenderer background;
        ScreenRect fittedCoverage;
        Vector2 fittedReference;
        bool fitted;
        bool upscaleReported;

        void Awake()
        {
            loader = GetComponent<LevelLoader>();
            grid = GetComponent<KeyboardGridView>();
            background = new GameObject("Level Background").AddComponent<SpriteRenderer>();
        }

        void OnEnable() => loader.LevelChanged += Refresh;

        void OnDisable() => loader.LevelChanged -= Refresh;

        void Start() => Refresh();

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
            FitIfChanged();
            if (traceBackground) TraceFrame();
        }

        void OnDrawGizmosSelected()
        {
            if (!fitted) return;
            Gizmos.color = Color.yellow;
            DrawDashedRect(fittedCoverage);
        }

        void Refresh()
        {
            var level = loader.Current;
            var sprite = level ? level.Background : null;
            background.sortingOrder = sortingOrder;
            background.sprite = sprite;
            background.color = level ? level.BackgroundTint : Color.white;
            var placement = level ? level.BackgroundPlacement : null;
            background.enabled = sprite && (placement == null || placement.Visible);
            background.flipX = placement != null && placement.FlipX;
            background.flipY = placement != null && placement.FlipY;
            upscaleReported = false;
            fitted = false;
            FitIfChanged();
        }

        void FitIfChanged()
        {
            if (!TryMeasure(out var coverage, out var reference)) return;
            if (fitted && SameAsFitted(coverage, reference)) return;
            Fit(coverage, reference);
        }

        bool TryMeasure(out ScreenRect coverage, out Vector2 reference)
        {
            coverage = default;
            reference = default;
            var camera = Camera.main;
            if (!camera || !camera.orthographic) return false;
            var center = (Vector2)camera.transform.position;
            var size = camera.orthographicSize;
            var zoom = CameraZoomView.Current;
            if (zoom && zoom.TryGetHome(out var homeCenter, out var homeSize))
            {
                center = homeCenter;
                size = homeSize;
            }
            reference = new Vector2(size * 2f * camera.aspect, size * 2f);
            var planned = zoom
                ? BackgroundCoverage.Zooming(center, size, camera.aspect, BoardBounds(), zoom.ZoomFactor, overscanShare)
                : BackgroundCoverage.Still(center, size, camera.aspect, overscanShare);
            coverage = BackgroundCoverage.IncludingLiveView(planned, LiveView(camera), overscanShare);
            return true;
        }

        static ScreenRect LiveView(Camera camera) =>
            ScreenRect.Around(camera.transform.position, new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize));

        void TraceFrame()
        {
            var camera = Camera.main;
            if (!camera || !camera.orthographic || !background.sprite) return;
            var zoom = CameraZoomView.Current;
            if (!zoom || !zoom.TryGetHome(out var home, out var homeSize)) return;
            var live = LiveView(camera);
            var atHome = (Vector2)camera.transform.position == home && Mathf.Approximately(camera.orthographicSize, homeSize);
            if (atHome) return;
            if (!traceStarted)
            {
                File.WriteAllText(TracePath, BackgroundTraceLine.Header + "\n");
                traceStarted = true;
            }
            var bounds = background.bounds;
            var sprite = new ScreenRect(bounds.min, bounds.max);
            var line = BackgroundTraceLine.Format(Time.time, camera.transform.position, camera.orthographicSize, camera.aspect, live, fittedCoverage, sprite, grid.BackdropVisible);
            File.AppendAllText(TracePath, line + (BackgroundTraceLine.ViewIsInside(live, sprite) ? "" : ";OUTSIDE") + "\n");
        }

        ScreenRect BoardBounds()
        {
            var scale = grid.transform.lossyScale;
            var half = new Vector2(grid.ContentWidth * scale.x, grid.ContentHeight * scale.y) * 0.5f;
            return ScreenRect.Around(grid.transform.position, half);
        }

        bool SameAsFitted(ScreenRect coverage, Vector2 reference) =>
            coverage.Min == fittedCoverage.Min && coverage.Max == fittedCoverage.Max && reference == fittedReference;

        void Fit(ScreenRect coverage, Vector2 reference)
        {
            fittedCoverage = coverage;
            fittedReference = reference;
            fitted = true;
            var sprite = background.sprite;
            if (!sprite) return;
            var size = (Vector2)sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f) return;
            var level = loader.Current;
            var fit = BackgroundFit.Of(size, coverage, reference, level ? level.BackgroundPlacement : new BackgroundPlacement());
            var pivot = BackgroundFit.PivotPosition(fit.Center, sprite.bounds.center, background.flipX, background.flipY, fit.Scale);
            background.transform.position = new Vector3(pivot.x, pivot.y, 0f);
            background.transform.localScale = new Vector3(fit.Scale, fit.Scale, 1f);
            ReportUpscale(fit.Scale, sprite);
        }

        void ReportUpscale(float scale, Sprite sprite)
        {
            if (scale <= maxUpscale || upscaleReported) return;
            upscaleReported = true;
            Debug.LogWarning($"Background '{sprite.name}' is drawn {scale:0.0} times its imported size to cover the camera (limit {maxUpscale:0.0}): it will look blurry. Use a larger picture.", this);
        }

        static void DrawDashedRect(ScreenRect rect)
        {
            var bottomLeft = rect.Min;
            var topRight = rect.Max;
            var bottomRight = new Vector2(topRight.x, bottomLeft.y);
            var topLeft = new Vector2(bottomLeft.x, topRight.y);
            DrawDashedLine(bottomLeft, bottomRight);
            DrawDashedLine(bottomRight, topRight);
            DrawDashedLine(topRight, topLeft);
            DrawDashedLine(topLeft, bottomLeft);
        }

        static void DrawDashedLine(Vector2 from, Vector2 to)
        {
            var length = Vector2.Distance(from, to);
            var direction = (to - from) / Mathf.Max(length, Mathf.Epsilon);
            for (float start = 0f; start < length; start += DashLength * 2f)
                Gizmos.DrawLine(from + direction * start, from + direction * Mathf.Min(start + DashLength, length));
        }
    }
}
