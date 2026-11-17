using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Scales the grid object so the keyboard spans the width of the screen, edge to edge, keeping its proportions.
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
        void LateUpdate()
        {
            var cam = Camera.main;
            if (view.Layout == null || !cam) return;
            float visibleWidth = 2f * cam.orthographicSize * cam.aspect;
            transform.localScale = Vector3.one * (visibleWidth * (1f - 2f * sideMargin) / view.ContentWidth);
        }
    }
}
