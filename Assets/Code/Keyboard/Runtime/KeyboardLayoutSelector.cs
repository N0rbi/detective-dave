using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput
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
                if (Enum.TryParse<Key>(DigitKeyPrefix + layout.digit, out var key))
                    choices.Add(new LayoutChoice(layout, key, option.glyph));
                else
                    Debug.LogError($"Layout '{layout.id}' has digit {layout.digit}, which is not a number key.", this);
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

            var pressed = choices.FirstOrDefault(candidate => keyboard[candidate.Key].wasPressedThisFrame);
            if (pressed != null && KeyboardSession.Lock(pressed.Layout)) Proceed();
        }

        void Proceed()
        {
            proceeding = true;
            LayoutLocked?.Invoke(KeyboardSession.Layout);
            Invoke(nameof(LoadNext), delay);
        }

        void LoadNext() => SceneLoader.Load(nextScene);
    }
}
