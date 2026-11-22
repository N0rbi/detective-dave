using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [Serializable]
    public sealed class ColorOverride
    {
        [SerializeField] [Tooltip("Tick to use the colour on this line for this case. Left unticked, the case takes the colour from Overworld Settings.")] bool use;
        [SerializeField] [Tooltip("The colour used for this case when the box on the left is ticked.")] Color color = Color.black;

        public ColorOverride()
        {
        }

        public ColorOverride(bool use, Color color)
        {
            this.use = use;
            this.color = color;
        }

        public bool Use => use;

        public Color Color => color;

        public Color Resolve(Color fromSettings) => use ? color : fromSettings;
    }
}
