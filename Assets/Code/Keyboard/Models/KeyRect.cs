using System;
using UnityEngine;

namespace DetectiveDave.KeyboardInput
{
    /// <summary>A rectangle in key units: origin at the top-left of the board, y pointing down.</summary>
    [Serializable]
    public class KeyRect
    {
        public float x, y, w, h;

        public Vector2 Center => new Vector2(x + w * 0.5f, y + h * 0.5f);
        public Vector2 Size => new Vector2(w, h);

        public bool Contains(Vector2 point) =>
            point.x >= x && point.x <= x + w && point.y >= y && point.y <= y + h;
    }
}
