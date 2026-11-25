using UnityEngine;

namespace DetectiveDave.DogPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Dog Settings", fileName = "Dog Settings.config.asset")]
    public sealed class DogSettings : ScriptableObject
    {
        [Header("Look")]
        [SerializeField, Min(0.1f)] [Tooltip("Dog height measured in key sizes.")] float heightInKeys = 1.2f;
        [SerializeField] [Tooltip("Moves the dog up (positive) or down on its key, measured in key sizes.")] float offsetYInKeys = 0f;
        [Tooltip("Angry sprites in order. They play 1-2-3-2-1 and loop. At least one.")]
        [SerializeField] Sprite[] angryFrames;
        [Tooltip("Calm sprites in order. They play 1-2-3-2-1 and loop. At least one.")]
        [SerializeField] Sprite[] calmFrames;
        [SerializeField, Min(0.5f)] [Tooltip("How many animation frames the dogs show per second.")] float framesPerSecond = 8f;
        [SerializeField] [Tooltip("Hide the key art of the key a dog stands on, so only the dog is drawn there.")] bool hideKeyArt = true;

        public float HeightInKeys => heightInKeys;

        public float OffsetYInKeys => offsetYInKeys;

        public Sprite[] AngryFrames => angryFrames;

        public Sprite[] CalmFrames => calmFrames;

        public float FramesPerSecond => framesPerSecond;

        public bool HideKeyArt => hideKeyArt;

        public bool HasValidFrames => HasSprites(angryFrames) && HasSprites(calmFrames);

        static bool HasSprites(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0) return false;
            foreach (var frame in frames)
                if (!frame) return false;
            return true;
        }
    }
}
