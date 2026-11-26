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
        [Header("Animation pace")]
        [SerializeField] [Tooltip("Timer: every dog steps at Frames Per Second. Bass Notes: every dog steps when the upright bass starts a note in the loop (needs a Bass Chart on Music Settings; without one the Timer is used and one warning is logged).")] DogStepMode stepMode = DogStepMode.BassNotes;
        [SerializeField, Range(0f, 0.9f)] [Tooltip("Chance that a dog skips a step, so the dogs drift apart. Every dog also starts on its own random frame. 0 keeps them in the same rhythm.")] float skipChance = 0.2f;
        [SerializeField, Min(1)] [Tooltip("Bass Notes only. A calm dog steps on every Nth bass note. An angry dog steps on every note. 2 makes calm dogs move at half speed.")] int calmEveryNotes = 2;

        [Header("Key art")]
        [SerializeField] [Tooltip("Hide the key art of the key a dog stands on, so only the dog is drawn there.")] bool hideKeyArt = true;

        public float HeightInKeys => heightInKeys;

        public float OffsetYInKeys => offsetYInKeys;

        public Sprite[] AngryFrames => angryFrames;

        public Sprite[] CalmFrames => calmFrames;

        public float FramesPerSecond => framesPerSecond;

        public bool HideKeyArt => hideKeyArt;

        public DogStepMode StepMode => stepMode;

        public DogPacing Pacing => new DogPacing(skipChance, calmEveryNotes);

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
