using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    public sealed class MusicLink : MonoBehaviour
    {
        [SerializeField] MusicSettings settings;

        IMoveAttemptSource source;
        IMusicReaction reaction;

        void OnEnable()
        {
            source = GetComponent<IMoveAttemptSource>();
            if (source == null)
            {
                Debug.LogError("MusicLink needs a PlayerController or an OverworldController on the same object.", this);
                enabled = false;
                return;
            }
            if (!settings)
            {
                Debug.LogError("MusicLink has no Music Settings. Run Window > Detective Dave > Set Up Music.", this);
                enabled = false;
                return;
            }
            reaction = MusicDirector.Ensure(settings).Reaction;
            source.MoveAttempted += reaction.OnMoveAttempted;
        }

        void OnDisable()
        {
            if (source != null && reaction != null) source.MoveAttempted -= reaction.OnMoveAttempted;
            reaction = null;
        }
    }
}
