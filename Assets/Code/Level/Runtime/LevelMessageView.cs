using UnityEngine;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// A banner across the top of the screen (IMGUI) when the level cannot be played as it is: no level at all, or a level whose shape is empty.
    /// Without it such a level just shows plain keys and looks like a bug. Says nothing while the level is fine.
    /// </summary>
    [RequireComponent(typeof(LevelLoader))]
    public sealed class LevelMessageView : MonoBehaviour
    {
        const float FontPerScreen = 0.03f;      // shares of the screen height
        const float HeightPerScreen = 0.06f;

        static readonly Color BannerColor = new Color(0.75f, 0.2f, 0.15f, 0.9f);

        LevelLoader loader;
        string message;
        GUIStyle style;
        Texture2D fill;

        public string PlayerMessage { get; set; }

        void Awake() => loader = GetComponent<LevelLoader>();

        void OnEnable() => loader.LevelChanged += Refresh;

        void OnDisable() => loader.LevelChanged -= Refresh;

        void Start() => Refresh();

        void OnDestroy()
        {
            if (fill) Destroy(fill);
        }

        void Refresh()
        {
            var level = loader.Current;
            if (!level) message = loader.Sequence ? "No level to play: the level sequence is empty." : "No level to play: the Level Loader has no level sequence.";
            else if (loader.Terrain.IsEmpty) message = $"Level '{level.DisplayName}': shape is empty. Paint its keys in the Levels window.";
            else message = null;
        }

        void OnGUI()
        {
            var text = string.IsNullOrEmpty(message) ? PlayerMessage : message;
            if (string.IsNullOrEmpty(text)) return;
            if (!fill)
            {
                fill = new Texture2D(1, 1);
                fill.SetPixel(0, 0, BannerColor);
                fill.Apply();
            }
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            style.fontSize = Mathf.RoundToInt(Screen.height * FontPerScreen);
            style.normal.textColor = Color.white;
            var area = new Rect(0f, 0f, Screen.width, Screen.height * HeightPerScreen);
            GUI.DrawTexture(area, fill);
            GUI.Label(area, text, style);
        }
    }
}
