using DetectiveDave.KeyboardInput;
using UnityEngine;

namespace DetectiveDave.CheatPlay
{
    public sealed class CheatLink : MonoBehaviour
    {
        [SerializeField] CheatSettings settings;

        IKeyEventSource source;
        CheatDirector director;

        void OnEnable()
        {
            source = GetComponent<IKeyEventSource>();
            if (source == null)
            {
                Debug.LogError("CheatLink needs a Keyboard Key Source on the same object.", this);
                enabled = false;
                return;
            }
            if (!settings)
            {
                Debug.LogError("CheatLink has no Cheat Settings. Run Window > Detective Dave > Set Up Cheats.", this);
                enabled = false;
                return;
            }
            director = CheatDirector.Ensure(settings);
            source.RawKeyPressed += director.OnRawKeyPressed;
        }

        void OnDisable()
        {
            if (source != null && director) source.RawKeyPressed -= director.OnRawKeyPressed;
            director = null;
        }
    }
}
