using UnityEngine;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>The prompt on the layout selection screen (IMGUI).</summary>
    public class KeyboardSelectionOverlay : MonoBehaviour
    {
        const float FontPerScreen = 0.035f;     // shares of the screen height
        const float TopPerScreen = 0.015f;
        const float HeightPerScreen = 0.07f;

        [SerializeField] KeyboardLayoutSelector selector;

        GUIStyle style;

        void OnGUI()
        {
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            style.fontSize = Mathf.RoundToInt(Screen.height * FontPerScreen);
            style.normal.textColor = Color.white;

            var text = KeyboardSession.IsLocked
                ? $"Layout: {KeyboardSession.Layout.Name}"
                : $"Press 1 - {selector.Choices.Count} to pick your keyboard layout";
            var area = new Rect(0f, Screen.height * TopPerScreen, Screen.width, Screen.height * HeightPerScreen);
            GUI.Label(area, text, style);
        }
    }
}
