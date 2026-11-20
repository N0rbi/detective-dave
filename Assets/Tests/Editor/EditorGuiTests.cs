using System.Collections;
using DetectiveDave.EditorTools;
using DetectiveDave.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DetectiveDave.Tests
{
    /// <summary>
    /// Draws the level tools' IMGUI for several editor frames (each frame is a Layout and a Repaint event) and fails on any error logged,
    /// which is how "EndLayoutGroup: BeginLayoutGroup must be called first" and friends show up.
    /// </summary>
    public class EditorGuiTests
    {
        const int Frames = 6;

        /// <summary>Hosts an inspector in a window so its OnInspectorGUI runs inside a real GUI pass.</summary>
        sealed class InspectorHost : EditorWindow
        {
            public UnityEditor.Editor Inspector;

            void OnGUI()
            {
                if (Inspector) Inspector.OnInspectorGUI();
            }

            void OnDestroy()
            {
                if (Inspector) Object.DestroyImmediate(Inspector);
            }
        }

        static IEnumerator DrawFrames(EditorWindow window)
        {
            for (int i = 0; i < Frames; i++)
            {
                window.Repaint();
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator LevelsWindow_DrawsWithoutLayoutErrors()
        {
            var window = EditorWindow.GetWindow<LevelsWindow>();
            try
            {
                yield return DrawFrames(window);
                Selection.activeObject = LevelTools.FindSequence();     // a selection change mid-way must not unbalance a pass
                yield return DrawFrames(window);
                LogAssert.NoUnexpectedReceived();
            }
            finally { window.Close(); }
        }

        [UnityTest]
        public IEnumerator LevelAndKeyArtInspectors_DrawWithoutLayoutErrors()
        {
            var sequence = LevelTools.FindSequence();
            var level = sequence ? sequence.First : null;
            if (!level) Assert.Ignore("No level in the sequence to inspect.");
            foreach (var target in new Object[] { level, level.KeyArt })
            {
                if (!target) continue;
                var host = ScriptableObject.CreateInstance<InspectorHost>();
                host.Inspector = UnityEditor.Editor.CreateEditor(target);
                host.Show();
                try { yield return DrawFrames(host); }
                finally { host.Close(); }
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
