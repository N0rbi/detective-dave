using UnityEngine;

namespace DetectiveDave.EditorTools
{
    readonly struct SmokingSprites
    {
        public readonly Sprite Body;
        public readonly Sprite Arm;

        public SmokingSprites(Sprite body, Sprite arm)
        {
            Body = body;
            Arm = arm;
        }
    }
}
