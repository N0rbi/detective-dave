using System;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    [Serializable]
    public struct GustLine
    {
        [SerializeField, Range(-1f, 1f)] [Tooltip("Sideways offset from the dash path, as a share of the player height. Positive is to the left of the dash direction.")] float offsetAcrossInHeights;
        [SerializeField, Min(0.005f)] [Tooltip("Thickness of the line, as a share of the player height. For a horizontal dash this is the line's vertical size.")] float thicknessInHeights;
        [SerializeField, Range(0f, 1f)] [Tooltip("Length of the line as a share of the distance travelled.")] float lengthShare;
        [SerializeField, Range(0f, 1f)] [Tooltip("Where along the distance travelled the line starts: 0 at the start of the dash, 1 at the end.")] float startShare;

        public GustLine(float offsetAcrossInHeights, float thicknessInHeights, float lengthShare, float startShare)
        {
            this.offsetAcrossInHeights = offsetAcrossInHeights;
            this.thicknessInHeights = thicknessInHeights;
            this.lengthShare = lengthShare;
            this.startShare = startShare;
        }

        public float OffsetAcrossInHeights => offsetAcrossInHeights;
        public float ThicknessInHeights => thicknessInHeights;
        public float LengthShare => lengthShare;
        public float StartShare => startShare;
    }
}
