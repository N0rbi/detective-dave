using UnityEngine;

namespace DetectiveDave.GuardPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Guard Settings", fileName = "Guard Settings.config.asset")]
    public sealed class GuardSettings : ScriptableObject
    {
        [Header("Look")]
        [SerializeField, Min(0.1f)] [Tooltip("Guard height measured in key sizes.")] float heightInKeys = 1.1f;
        [Tooltip("Patrolling sprites in walking order. At least one.")]
        [SerializeField] Sprite[] walkFrames;
        [Tooltip("Chasing sprites in walking order. At least one.")]
        [SerializeField] Sprite[] chaseFrames;
        [SerializeField] [Tooltip("Turn on if the guard art looks to the right when not flipped.")] bool artFacesRight = false;

        [Header("Speed")]
        [SerializeField, Min(0.05f)] [Tooltip("Seconds a guard takes to walk onto the next key while patrolling. A Guard on a level can override this.")] float patrolSecondsPerKey = 0.6f;
        [SerializeField, Min(0.05f)] [Tooltip("Seconds a guard takes to walk onto the next key while chasing. A Guard on a level can override this.")] float chaseSecondsPerKey = 0.5f;

        public float HeightInKeys => heightInKeys;

        public Sprite[] WalkFrames => walkFrames;

        public Sprite[] ChaseFrames => chaseFrames;

        public bool ArtFacesRight => artFacesRight;

        public float PatrolSecondsPerKey => patrolSecondsPerKey;

        public float ChaseSecondsPerKey => chaseSecondsPerKey;

        public bool HasValidFrames => HasSprites(walkFrames) && HasSprites(chaseFrames);

        static bool HasSprites(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0) return false;
            foreach (var frame in frames)
                if (!frame) return false;
            return true;
        }
    }
}
