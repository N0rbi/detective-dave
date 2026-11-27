using DetectiveDave.PlayerPlay;
using UnityEngine;

namespace DetectiveDave.MusicPlay
{
    public sealed class MusicLink : MonoBehaviour
    {
        [SerializeField] MusicSettings settings;

        IMoveAttemptSource source;
        IMusicReaction reaction;
        IPlayerLifeSource life;
        DeathHookController hook;

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
            var director = MusicDirector.Ensure(settings);
            reaction = director.Reaction;
            source.MoveAttempted += reaction.OnMoveAttempted;
            life = GetComponent<IPlayerLifeSource>();
            if (life == null) return;
            hook = director.DeathHook;
            life.Died += hook.PlayerDied;
            life.Respawned += hook.PlayerRespawned;
        }

        void OnDisable()
        {
            if (source != null && reaction != null) source.MoveAttempted -= reaction.OnMoveAttempted;
            reaction = null;
            if (life != null && hook != null)
            {
                life.Died -= hook.PlayerDied;
                life.Respawned -= hook.PlayerRespawned;
                hook.Stop();
            }
            life = null;
            hook = null;
        }
    }
}
