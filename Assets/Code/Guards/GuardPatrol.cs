using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.GuardPlay
{
    [DisallowMultipleComponent]
    public sealed class GuardPatrol : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField] Key startKey = Key.None;

        [HideInInspector]
        [SerializeField] Key endKey = Key.None;

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

        public void PlaceStart(Key key) => startKey = key;

        public void PlaceEnd(Key key) => endKey = key;

        public GuardPatrolData ToData() => new GuardPatrolData(startKey, endKey, waitBeforeMoveSeconds, waitAtEndSeconds, patrolSecondsPerKey, chaseSecondsPerKey);
    }
}
