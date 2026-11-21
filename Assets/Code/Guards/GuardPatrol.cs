using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    [DisallowMultipleComponent]
    public sealed class GuardPatrol : MonoBehaviour
    {
        [Tooltip("The key the guard starts on and comes back to.")]
        [SerializeField] Key startKey = Key.A;

        [Tooltip("The key where the guard turns around.")]
        [SerializeField] Key endKey = Key.G;

        [Tooltip("Seconds the guard stands still on the start key before walking.")]
        [Min(0f)]
        [SerializeField] float waitBeforeMoveSeconds = 1f;

        [Tooltip("Seconds the guard stands still on the end key before walking back.")]
        [Min(0f)]
        [SerializeField] float waitAtEndSeconds = 1f;

        [Tooltip("Seconds per key while patrolling. 0 uses the Guard Settings value.")]
        [Min(0f)]
        [SerializeField] float patrolSecondsPerKey;

        [Tooltip("Seconds per key while chasing. 0 uses the Guard Settings value.")]
        [Min(0f)]
        [SerializeField] float chaseSecondsPerKey;

        public Key StartKey => startKey;

        public Key EndKey => endKey;

        public float WaitBeforeMoveSeconds => waitBeforeMoveSeconds;

        public float WaitAtEndSeconds => waitAtEndSeconds;

        public float PatrolSecondsPerKey => patrolSecondsPerKey;

        public float ChaseSecondsPerKey => chaseSecondsPerKey;

        public GuardPatrolData ToData() => new GuardPatrolData(startKey, endKey, waitBeforeMoveSeconds, waitAtEndSeconds, patrolSecondsPerKey, chaseSecondsPerKey);
    }
}
