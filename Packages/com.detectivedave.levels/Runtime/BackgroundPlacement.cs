using System;
using UnityEngine;

namespace DetectiveDave.Levels
{
    /// <summary>Where a level's background sits on the screen. The picture always covers the whole screen: it is never moved far enough to show an edge.</summary>
    [Serializable]
    public sealed class BackgroundPlacement
    {
        public const float MinZoom = 1f;

        [Tooltip("Turn off to draw no background in this level.")]
        [SerializeField] bool visible = true;

        [Tooltip("Moves the picture. 0 keeps it centred. 1 moves it one screen width to the right and one screen height up, as far as the picture allows without showing an edge.")]
        [SerializeField] Vector2 offset = Vector2.zero;

        [Min(MinZoom)]
        [Tooltip("1 shows the picture just large enough to cover the screen. Higher values zoom in, which also gives more room to move it.")]
        [SerializeField] float zoom = MinZoom;

        [Tooltip("Mirror the picture left to right.")]
        [SerializeField] bool flipX;

        [Tooltip("Mirror the picture top to bottom.")]
        [SerializeField] bool flipY;

        public bool Visible => visible;

        public Vector2 Offset => offset;

        public float Zoom => Mathf.Max(MinZoom, zoom);

        public bool FlipX => flipX;

        public bool FlipY => flipY;
    }
}
