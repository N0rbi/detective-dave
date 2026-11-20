using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Scales the grid object so the keyboard spans the width of the screen, edge to edge, keeping its proportions, unless it would then be taller than the screen: then it fits the height.
    /// It scales the whole object, so a level (tilemaps) that is a child of it stays lined up with the keys.
    /// Needs an orthographic camera tagged MainCamera.
    /// </summary>
    [RequireComponent(typeof(KeyboardGridView))]
    public class KeyboardGridScreenFit : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.2f)] float sideMargin = 0f;     // share of the screen width left empty on each side

        KeyboardGridView view;

        void Awake() => view = GetComponent<KeyboardGridView>();

        // Every frame: cheap, and it follows window resizes and camera changes without any events.
        void LateUpdate() => Apply();

        public void Apply()
        {
            var cam = Camera.main;
            if (view.Layout == null || !cam) return;
            float visibleHeight = 2f * cam.orthographicSize;
            float visibleWidth = visibleHeight * cam.aspect;
            float fitWidth = visibleWidth * (1f - 2f * sideMargin) / view.ContentWidth;
            float fitHeight = visibleHeight / view.ContentHeight;
            transform.localScale = Vector3.one * Mathf.Min(fitWidth, fitHeight);
        }
    }
}
