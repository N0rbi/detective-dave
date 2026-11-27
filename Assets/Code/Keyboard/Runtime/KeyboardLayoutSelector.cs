using System;
using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardGrid
{
    /// <summary>
    /// Layout picker, no visuals. The first press of a layout's number key locks that layout for the session,
    /// then the next scene loads after a delay. If a layout is already locked it just moves on.
    /// </summary>
    public class KeyboardLayoutSelector : MonoBehaviour
    {
        const string DigitKeyPrefix = "Digit";          // Key.Digit1 ... Key.Digit9

        [SerializeField] LayoutOption[] options;
        [SerializeField] GameScene nextScene = GameScene.KeyboardGrid;
        [SerializeField, Min(0f)] float delay = 0.8f;

        readonly List<LayoutChoice> choices = new List<LayoutChoice>();
        bool proceeding;

        public IReadOnlyList<LayoutChoice> Choices => choices;

        /// <summary>Raised once, with the locked layout, when a layout gets chosen or turns out to be chosen already.</summary>
        public event Action<KeyboardLayout> LayoutLocked;

        void Awake()
        {
            foreach (var option in options.Where(candidate => candidate != null && candidate.layout))
            {
                var layout = KeyboardLayout.Parse(option.layout);
                if (Enum.TryParse<Key>(DigitKeyPrefix + layout.Digit, out var key))
                    choices.Add(new LayoutChoice(layout, key, option.glyph));
                else
                    Debug.LogError($"Layout '{layout.Id}' has digit {layout.Digit}, which is not a number key.", this);
            }
        }

        void Start()
        {
            if (KeyboardSession.IsLocked) Proceed();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (proceeding || keyboard == null) return;

            // Read straight from the Input System, not through a KeyboardKeySource: there is no layout to read yet, and only a
            // press during this frame counts (a digit already held when the scene starts must not pick a layout).
            foreach (var choice in choices)          // the first choice pressed this frame wins
            {
                if (!keyboard[choice.Key].wasPressedThisFrame) continue;
                if (KeyboardSession.Lock(choice.Layout)) Proceed();
                return;
            }
        }

        void Proceed()
        {
            proceeding = true;
            LayoutLocked?.Invoke(KeyboardSession.Layout);
            Invoke(nameof(LoadNext), delay);
        }

        void LoadNext()
        {
            var scene = KeyboardSession.ReturnTo ?? nextScene;
            KeyboardSession.ReturnTo = null;
            SceneLoader.Load(scene);
        }
    }
}
