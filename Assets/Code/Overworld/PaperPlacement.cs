using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [Serializable]
    public struct PaperPlacement
    {
        [SerializeField, Range(-0.5f, 0.5f)] [Tooltip("Sideways position of the paper centre as a share of the screen width. Negative is left of the middle.")] float centreXShare;
        [SerializeField, Range(-0.5f, 0.5f)] [Tooltip("Height of the paper centre as a share of the screen height. Negative is below the middle.")] float centreYShare;
        [SerializeField, Range(0.1f, 0.9f)] [Tooltip("Width of the paper as a share of the screen width.")] float widthShare;
        [SerializeField, Range(-15f, 15f)] [Tooltip("How far the paper is turned, in degrees. Positive turns it anticlockwise.")] float rotationDegrees;

        public PaperPlacement(float centreXShare, float centreYShare, float widthShare, float rotationDegrees)
        {
            this.centreXShare = centreXShare;
            this.centreYShare = centreYShare;
            this.widthShare = widthShare;
            this.rotationDegrees = rotationDegrees;
        }

        public float CentreXShare => centreXShare;
        public float CentreYShare => centreYShare;
        public float WidthShare => widthShare;
        public float RotationDegrees => rotationDegrees;
    }
}
